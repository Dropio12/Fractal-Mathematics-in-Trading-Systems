#!/usr/bin/env python3
"""
High-Frequency Trading Market Data Producer
Streams 10,000+ candles/second to Kafka for real-time fractal analysis

Enhanced system scalability and performance with distributed data pipelines
"""

import json
import time
import random
import math
from datetime import datetime, timedelta
from typing import Dict, List
import threading
import asyncio
from dataclasses import dataclass, asdict
from kafka import KafkaProducer
from kafka.errors import KafkaError
import numpy as np
from concurrent.futures import ThreadPoolExecutor
import argparse
import logging
from prometheus_client import Counter, Histogram, Gauge, start_http_server

# Metrics
MESSAGES_SENT = Counter('hft_messages_sent_total', 'Total messages sent to Kafka')
MESSAGE_LATENCY = Histogram('hft_message_latency_seconds', 'Message production latency')
THROUGHPUT_GAUGE = Gauge('hft_throughput_msg_per_sec', 'Current throughput in messages/sec')
ERROR_COUNTER = Counter('hft_errors_total', 'Total errors', ['error_type'])

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@dataclass
class MarketTick:
    """High-frequency market tick with microsecond precision"""
    timestamp_us: int  # Microseconds since epoch
    symbol: str
    price: float
    volume: int
    bid: float
    ask: float
    spread: float
    volatility: float
    fractal_dimension: float
    sequence_id: int
    exchange: str
    
    def to_json(self) -> str:
        return json.dumps(asdict(self), separators=(',', ':'))

class FractalMarketGenerator:
    """Generates realistic HFT market data with fractal properties"""
    
    def __init__(self, symbols: List[str], initial_prices: Dict[str, float]):
        self.symbols = symbols
        self.prices = initial_prices.copy()
        self.sequence_id = 0
        self.rng = random.Random(42)  # Reproducible results
        
        # HFT-specific parameters
        self.tick_size = 0.0001  # 1 pip for FX
        self.max_spread = 0.002  # 20 pips max spread
        self.volatility_memory = {symbol: 0.001 for symbol in symbols}
        
    def generate_tick(self, symbol: str) -> MarketTick:
        """Generate single HFT tick with fractal characteristics"""
        current_time_us = int(time.time() * 1_000_000)
        
        # Multi-scale fractal noise (microsecond resolution)
        noise = self._generate_fractal_noise(current_time_us)
        
        # Update price with fractal movement
        volatility = self.volatility_memory[symbol]
        price_change = volatility * (self.rng.gauss(0, 1) + 0.3 * noise)
        
        # Apply tick size constraint
        ticks = round(price_change / self.tick_size)
        actual_change = ticks * self.tick_size
        
        self.prices[symbol] += actual_change
        
        # Update volatility with mean reversion
        vol_change = 0.001 * self.rng.gauss(0, 1)
        self.volatility_memory[symbol] = max(0.0001, 
            self.volatility_memory[symbol] * 0.999 + abs(vol_change))
        
        # Generate bid/ask spread
        spread = min(self.max_spread, 
            self.tick_size * (2 + abs(self.rng.gauss(0, 3))))
        
        bid = self.prices[symbol] - spread / 2
        ask = self.prices[symbol] + spread / 2
        
        # Volume follows power law (typical in HFT)
        volume = max(1, int(abs(self.rng.weibullvariate(2, 100))))
        
        # Calculate fractal dimension (simplified for speed)
        fd = self._quick_fractal_dimension(symbol)
        
        self.sequence_id += 1
        
        return MarketTick(
            timestamp_us=current_time_us,
            symbol=symbol,
            price=round(self.prices[symbol], 5),
            volume=volume,
            bid=round(bid, 5),
            ask=round(ask, 5),
            spread=round(spread, 5),
            volatility=round(volatility, 6),
            fractal_dimension=round(fd, 3),
            sequence_id=self.sequence_id,
            exchange="FRACTAL_HFT"
        )
    
    def _generate_fractal_noise(self, time_us: int) -> float:
        """Generate multi-octave fractal noise for microsecond timestamps"""
        noise = 0.0
        amplitude = 1.0
        frequency = 1.0
        
        for _ in range(5):  # 5 octaves for fractal behavior
            phase = (time_us * frequency * 1e-9) % (2 * math.pi)
            sine = math.sin(phase) + 0.5 * math.sin(phase * 1.618)
            noise += amplitude * sine * self.rng.gauss(0, 1) * 0.1
            amplitude *= 0.6
            frequency *= 2.1
            
        return noise
    
    def _quick_fractal_dimension(self, symbol: str) -> float:
        """Fast approximate fractal dimension for real-time use"""
        # Use volatility as proxy for fractal complexity
        vol = self.volatility_memory[symbol]
        # Map volatility to typical fractal dimension range [1.1, 1.9]
        return 1.1 + 0.8 * min(1.0, vol * 1000)

class HighThroughputKafkaProducer:
    """Kafka producer optimized for high-frequency trading"""
    
    def __init__(self, bootstrap_servers: str = 'localhost:9092'):
        self.producer = KafkaProducer(
            bootstrap_servers=bootstrap_servers,
            value_serializer=lambda v: v.encode('utf-8'),
            key_serializer=lambda k: k.encode('utf-8') if k else None,
            # Performance optimizations for HFT
            acks='all',  # Wait for all replicas
            retries=3,
            max_in_flight_requests_per_connection=100,
            linger_ms=1,  # Small batch delay for throughput
            compression_type='lz4',  # Fast compression
            buffer_memory=67108864,  # 64MB buffer
            batch_size=65536,  # 64KB batches
        )
        self.stats = {
            'messages_sent': 0,
            'errors': 0,
            'start_time': time.time()
        }
        
    def send_tick(self, topic: str, tick: MarketTick) -> bool:
        """Send market tick to Kafka with error handling"""
        try:
            start_time = time.time()
            
            # Use symbol as partition key for ordered processing per symbol
            future = self.producer.send(
                topic=topic,
                key=tick.symbol,
                value=tick.to_json(),
                timestamp_ms=tick.timestamp_us // 1000
            )
            
            # Non-blocking send with callback
            future.add_callback(self._on_send_success)
            future.add_errback(self._on_send_error)
            
            MESSAGE_LATENCY.observe(time.time() - start_time)
            MESSAGES_SENT.inc()
            self.stats['messages_sent'] += 1
            
            return True
            
        except KafkaError as e:
            ERROR_COUNTER.labels(error_type='kafka_error').inc()
            self.stats['errors'] += 1
            logger.error(f"Kafka error: {e}")
            return False
        except Exception as e:
            ERROR_COUNTER.labels(error_type='general_error').inc()
            self.stats['errors'] += 1
            logger.error(f"Unexpected error: {e}")
            return False
    
    def _on_send_success(self, record_metadata):
        """Callback for successful message delivery"""
        logger.debug(f"Message sent to {record_metadata.topic}:{record_metadata.partition}")
    
    def _on_send_error(self, exception):
        """Callback for failed message delivery"""
        ERROR_COUNTER.labels(error_type='delivery_error').inc()
        logger.error(f"Message delivery failed: {exception}")
    
    def get_throughput(self) -> float:
        """Calculate current messages per second"""
        elapsed = time.time() - self.stats['start_time']
        return self.stats['messages_sent'] / max(elapsed, 0.001)
    
    def flush_and_close(self):
        """Ensure all messages are sent and close producer"""
        self.producer.flush()
        self.producer.close()

class HFTSimulationEngine:
    """High-Frequency Trading simulation engine with distributed streaming"""
    
    def __init__(self, target_throughput: int = 10000):
        self.target_throughput = target_throughput
        self.running = False
        
        # Major currency pairs for HFT simulation
        self.symbols = ['EURUSD', 'GBPUSD', 'USDJPY', 'USDCHF', 'AUDUSD', 'USDCAD', 'NZDUSD']
        initial_prices = {
            'EURUSD': 1.0850, 'GBPUSD': 1.2750, 'USDJPY': 149.50,
            'USDCHF': 0.8950, 'AUDUSD': 0.6650, 'USDCAD': 1.3650, 'NZDUSD': 0.6150
        }
        
        self.market_generator = FractalMarketGenerator(self.symbols, initial_prices)
        self.kafka_producer = HighThroughputKafkaProducer()
        
        # Performance tracking
        self.throughput_window = []
        self.window_size = 100
        
    def start_simulation(self, duration_seconds: int = 3600):
        """Start HFT simulation with specified duration"""
        logger.info(f"Starting HFT simulation: {self.target_throughput} ticks/sec for {duration_seconds}s")
        
        self.running = True
        start_time = time.time()
        target_interval = 1.0 / self.target_throughput
        
        # Start metrics server
        start_http_server(8000)
        logger.info("Metrics server started on port 8000")
        
        try:
            with ThreadPoolExecutor(max_workers=len(self.symbols)) as executor:
                tick_count = 0
                next_tick_time = start_time
                
                while self.running and (time.time() - start_time) < duration_seconds:
                    current_time = time.time()
                    
                    if current_time >= next_tick_time:
                        # Generate ticks for all symbols simultaneously
                        futures = []
                        for symbol in self.symbols:
                            future = executor.submit(self._generate_and_send_tick, symbol)
                            futures.append(future)
                        
                        # Wait for all ticks to be generated
                        for future in futures:
                            future.result()
                        
                        tick_count += len(self.symbols)
                        next_tick_time += target_interval
                        
                        # Update throughput metrics
                        self._update_throughput_metrics()
                        
                        # Performance logging every 1000 ticks
                        if tick_count % 1000 == 0:
                            throughput = self.kafka_producer.get_throughput()
                            THROUGHPUT_GAUGE.set(throughput)
                            logger.info(f"Ticks sent: {tick_count}, Throughput: {throughput:.1f} msg/s")
                    
                    # Microsecond sleep for precision
                    sleep_time = next_tick_time - time.time()
                    if sleep_time > 0:
                        time.sleep(min(sleep_time, 0.001))  # Max 1ms sleep
                        
        except KeyboardInterrupt:
            logger.info("Simulation interrupted by user")
        finally:
            self.stop_simulation()
    
    def _generate_and_send_tick(self, symbol: str):
        """Generate and send tick for specific symbol"""
        tick = self.market_generator.generate_tick(symbol)
        self.kafka_producer.send_tick('market-ticks', tick)
    
    def _update_throughput_metrics(self):
        """Update rolling throughput metrics"""
        current_throughput = self.kafka_producer.get_throughput()
        self.throughput_window.append(current_throughput)
        
        if len(self.throughput_window) > self.window_size:
            self.throughput_window.pop(0)
    
    def stop_simulation(self):
        """Stop simulation and cleanup"""
        self.running = False
        logger.info("Stopping HFT simulation...")
        
        # Final stats
        throughput = self.kafka_producer.get_throughput()
        stats = self.kafka_producer.stats
        
        logger.info(f"Final Statistics:")
        logger.info(f"  Messages sent: {stats['messages_sent']:,}")
        logger.info(f"  Errors: {stats['errors']}")
        logger.info(f"  Average throughput: {throughput:.1f} msg/s")
        logger.info(f"  Success rate: {100 * (1 - stats['errors'] / max(stats['messages_sent'], 1)):.2f}%")
        
        self.kafka_producer.flush_and_close()

def main():
    """Main entry point for HFT market data producer"""
    parser = argparse.ArgumentParser(description='HFT Market Data Producer')
    parser.add_argument('--throughput', type=int, default=10000,
                       help='Target throughput (ticks/second)')
    parser.add_argument('--duration', type=int, default=3600,
                       help='Simulation duration (seconds)')
    parser.add_argument('--kafka-brokers', default='localhost:9092',
                       help='Kafka bootstrap servers')
    
    args = parser.parse_args()
    
    # Create HFT simulation engine
    engine = HFTSimulationEngine(target_throughput=args.throughput)
    
    try:
        engine.start_simulation(duration_seconds=args.duration)
    except Exception as e:
        logger.error(f"Simulation failed: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())
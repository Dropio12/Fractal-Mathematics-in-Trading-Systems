#!/usr/bin/env python3
"""
Real-Time Fractal Pattern Detection Consumer
Processes streaming market data with millisecond-latency pattern recognition

Enhanced system performance for high-frequency trading analysis
"""

import json
import time
import threading
import queue
from collections import deque, defaultdict
from dataclasses import dataclass, asdict
from typing import Dict, List, Optional, Tuple
import statistics
import math
from kafka import KafkaConsumer, KafkaProducer
from kafka.errors import KafkaError
import logging
import argparse
from datetime import datetime, timedelta
import asyncio
import signal
import sys
from prometheus_client import Counter, Histogram, Gauge, start_http_server

# Performance Metrics
PATTERNS_DETECTED = Counter('fractal_patterns_detected_total', 'Total fractal patterns detected')
PROCESSING_LATENCY = Histogram('pattern_processing_latency_seconds', 'Pattern processing latency')
CURRENT_THROUGHPUT = Gauge('consumer_throughput_msg_per_sec', 'Current message processing throughput')
ANOMALY_ALERTS = Counter('fractal_anomaly_alerts_total', 'Total fractal anomaly alerts')
BUFFER_SIZE_GAUGE = Gauge('processing_buffer_size', 'Current processing buffer size')

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@dataclass
class MarketTick:
    """Market tick data structure"""
    timestamp_us: int
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

@dataclass
class FractalPattern:
    """Detected fractal pattern"""
    symbol: str
    pattern_type: str
    start_time_us: int
    end_time_us: int
    duration_ms: int
    fractal_dimension: float
    confidence: float
    price_range: Tuple[float, float]
    volatility_avg: float
    pattern_strength: float
    prediction_signal: str
    risk_score: float

class RealTimeFractalDetector:
    """Real-time fractal pattern detection with millisecond precision"""
    
    def __init__(self, window_sizes: List[int] = None):
        self.window_sizes = window_sizes or [20, 50, 100]
        self.price_buffers = defaultdict(lambda: defaultdict(deque))
        self.pattern_cache = defaultdict(list)
        self.last_analysis_time = defaultdict(int)
        
        # Pattern detection parameters
        self.min_pattern_strength = 0.6
        self.anomaly_threshold = 2.0
        self.volatility_threshold = 0.05
        
        # Performance tracking
        self.processing_times = deque(maxlen=1000)
        self.patterns_detected = 0
    
    def add_tick(self, tick: MarketTick) -> List[FractalPattern]:
        """Add market tick and detect patterns in real-time"""
        start_time = time.time()
        patterns = []
        
        try:
            # Update price buffers for all window sizes
            for window_size in self.window_sizes:
                buffer = self.price_buffers[tick.symbol][window_size]
                buffer.append((tick.timestamp_us, tick.price, tick.volatility))
                
                # Maintain window size
                while len(buffer) > window_size:
                    buffer.popleft()
                
                # Detect patterns if buffer is full
                if len(buffer) == window_size:
                    pattern = self._detect_pattern_in_window(
                        tick.symbol, buffer, window_size
                    )
                    if pattern:
                        patterns.append(pattern)
                        self.patterns_detected += 1
                        PATTERNS_DETECTED.inc()
            
            # Update performance metrics
            processing_time = time.time() - start_time
            self.processing_times.append(processing_time)
            PROCESSING_LATENCY.observe(processing_time)
            
            # Update buffer size metrics
            max_buffer_size = max(
                len(self.price_buffers[tick.symbol][ws]) 
                for ws in self.window_sizes
            ) if tick.symbol in self.price_buffers else 0
            BUFFER_SIZE_GAUGE.set(max_buffer_size)
            
            return patterns
            
        except Exception as e:
            logger.error(f"Pattern detection error: {e}")
            return []
    
    def _detect_pattern_in_window(self, symbol: str, buffer: deque, 
                                window_size: int) -> Optional[FractalPattern]:
        """Detect fractal pattern in price window"""
        if len(buffer) < window_size:
            return None
        
        # Extract prices and timestamps
        timestamps = [item[0] for item in buffer]
        prices = [item[1] for item in buffer]
        volatilities = [item[2] for item in buffer]
        
        # Calculate fractal dimension using fast box-counting
        fractal_dim = self._fast_box_counting(prices)
        
        # Pattern classification
        pattern_type = self._classify_pattern(prices, volatilities, fractal_dim)
        
        if pattern_type == "NO_PATTERN":
            return None
        
        # Calculate pattern metrics
        confidence = self._calculate_confidence(prices, fractal_dim, volatilities)
        
        if confidence < self.min_pattern_strength:
            return None
        
        # Pattern strength and prediction
        strength = self._calculate_pattern_strength(prices, volatilities)
        signal = self._generate_prediction_signal(pattern_type, fractal_dim, strength)
        risk_score = self._calculate_risk_score(volatilities, fractal_dim)
        
        # Check for anomalies
        if risk_score > self.anomaly_threshold:
            ANOMALY_ALERTS.inc()
            logger.warning(f"Fractal anomaly detected: {symbol} - Risk: {risk_score:.2f}")
        
        return FractalPattern(
            symbol=symbol,
            pattern_type=pattern_type,
            start_time_us=timestamps[0],
            end_time_us=timestamps[-1],
            duration_ms=(timestamps[-1] - timestamps[0]) // 1000,
            fractal_dimension=fractal_dim,
            confidence=confidence,
            price_range=(min(prices), max(prices)),
            volatility_avg=statistics.mean(volatilities),
            pattern_strength=strength,
            prediction_signal=signal,
            risk_score=risk_score
        )
    
    def _fast_box_counting(self, prices: List[float]) -> float:
        """Fast box-counting algorithm optimized for real-time use"""
        if len(prices) < 10:
            return 1.0
        
        # Normalize prices
        min_price = min(prices)
        max_price = max(prices)
        price_range = max_price - min_price
        
        if price_range == 0:
            return 1.0
        
        normalized = [(p - min_price) / price_range for p in prices]
        
        # Use fewer box sizes for speed
        box_sizes = [1, 2, 4, 8, 16]
        log_sizes = []
        log_counts = []
        
        for box_size in box_sizes:
            if box_size >= len(prices) // 3:
                break
            
            # Fast box counting with set
            boxes = set()
            for i, price in enumerate(normalized[:-1]):
                x = i // box_size
                y = int(price * box_size)
                boxes.add((x, y))
            
            if len(boxes) > 1:
                log_sizes.append(math.log(1.0 / box_size))
                log_counts.append(math.log(len(boxes)))
        
        if len(log_sizes) < 2:
            return 1.0
        
        # Linear regression (simplified)
        n = len(log_sizes)
        sum_x = sum(log_sizes)
        sum_y = sum(log_counts)
        sum_xy = sum(x * y for x, y in zip(log_sizes, log_counts))
        sum_xx = sum(x * x for x in log_sizes)
        
        denom = n * sum_xx - sum_x * sum_x
        if abs(denom) < 1e-10:
            return 1.0
        
        slope = (n * sum_xy - sum_x * sum_y) / denom
        return max(1.0, min(2.0, slope))
    
    def _classify_pattern(self, prices: List[float], volatilities: List[float], 
                         fractal_dim: float) -> str:
        """Classify fractal pattern type"""
        avg_vol = statistics.mean(volatilities)
        
        # Price trend analysis
        returns = [(prices[i] - prices[i-1]) / prices[i-1] 
                  for i in range(1, len(prices))]
        trend = sum(returns[-10:]) if len(returns) >= 10 else 0
        
        # Pattern classification logic
        if fractal_dim < 1.2:
            return "SMOOTH_TREND" if abs(trend) > 0.01 else "SIDEWAYS"
        elif fractal_dim > 1.8:
            return "VOLATILE_BREAKOUT" if avg_vol > self.volatility_threshold else "CHOPPY"
        elif fractal_dim > 1.6:
            if trend > 0.02:
                return "VOLATILE_UPTREND"
            elif trend < -0.02:
                return "VOLATILE_DOWNTREND" 
            else:
                return "VOLATILE_RANGE"
        elif fractal_dim > 1.4:
            if abs(trend) > 0.015:
                return "TRENDING_FRACTAL"
            else:
                return "RANGE_FRACTAL"
        else:
            return "NORMAL_MOVEMENT"
    
    def _calculate_confidence(self, prices: List[float], fractal_dim: float, 
                            volatilities: List[float]) -> float:
        """Calculate pattern confidence score"""
        # Multiple factors contribute to confidence
        
        # 1. Fractal dimension stability
        dim_score = 1.0 - abs(fractal_dim - 1.5) / 0.5
        dim_score = max(0, min(1, dim_score))
        
        # 2. Price range significance
        price_range = (max(prices) - min(prices)) / statistics.mean(prices)
        range_score = min(1.0, price_range * 20)
        
        # 3. Volume consistency (proxy via volatility)
        vol_consistency = 1.0 - (statistics.stdev(volatilities) / 
                               max(statistics.mean(volatilities), 0.001))
        vol_consistency = max(0, min(1, vol_consistency))
        
        # 4. Pattern length factor
        length_score = min(1.0, len(prices) / 50.0)
        
        # Weighted combination
        confidence = (dim_score * 0.3 + range_score * 0.3 + 
                     vol_consistency * 0.2 + length_score * 0.2)
        
        return max(0, min(1, confidence))
    
    def _calculate_pattern_strength(self, prices: List[float], 
                                  volatilities: List[float]) -> float:
        """Calculate overall pattern strength"""
        # Price momentum
        returns = [(prices[i] - prices[i-1]) / prices[i-1] 
                  for i in range(1, len(prices))]
        momentum = abs(sum(returns[-5:])) if len(returns) >= 5 else 0
        
        # Volatility strength
        vol_strength = statistics.mean(volatilities) / 0.02  # Normalize to 2% vol
        
        # Combine factors
        strength = (momentum * 10 + vol_strength) / 2
        return max(0, min(1, strength))
    
    def _generate_prediction_signal(self, pattern_type: str, fractal_dim: float, 
                                  strength: float) -> str:
        """Generate trading signal prediction"""
        if strength < 0.3:
            return "NEUTRAL"
        
        signal_map = {
            "VOLATILE_UPTREND": "BUY",
            "VOLATILE_BREAKOUT": "BUY" if fractal_dim > 1.7 else "NEUTRAL",
            "SMOOTH_TREND": "HOLD",
            "VOLATILE_DOWNTREND": "SELL",
            "TRENDING_FRACTAL": "HOLD",
            "VOLATILE_RANGE": "NEUTRAL",
            "CHOPPY": "AVOID"
        }
        
        base_signal = signal_map.get(pattern_type, "NEUTRAL")
        
        # Modify based on strength
        if strength > 0.8:
            if base_signal == "BUY":
                return "STRONG_BUY"
            elif base_signal == "SELL":
                return "STRONG_SELL"
        elif strength < 0.4:
            return "WEAK_" + base_signal if base_signal != "NEUTRAL" else "NEUTRAL"
        
        return base_signal
    
    def _calculate_risk_score(self, volatilities: List[float], 
                            fractal_dim: float) -> float:
        """Calculate risk score for anomaly detection"""
        avg_vol = statistics.mean(volatilities)
        vol_spike = max(volatilities) / max(avg_vol, 0.001)
        
        # Risk factors
        volatility_risk = min(2.0, avg_vol / 0.03)  # 3% vol = 1.0 risk
        fractal_risk = max(0, (fractal_dim - 1.5) * 2)  # Higher FD = more risk
        spike_risk = max(0, (vol_spike - 2) / 2)  # Volatility spikes
        
        return volatility_risk + fractal_risk + spike_risk

class HighPerformanceKafkaConsumer:
    """High-performance Kafka consumer optimized for real-time processing"""
    
    def __init__(self, bootstrap_servers: str = 'localhost:9092', 
                 topics: List[str] = None):
        self.bootstrap_servers = bootstrap_servers
        self.topics = topics or ['market-ticks']
        self.running = False
        self.consumer = None
        self.producer = None
        
        # Performance tracking
        self.messages_processed = 0
        self.start_time = time.time()
        self.last_throughput_update = time.time()
        
        # Pattern detector
        self.fractal_detector = RealTimeFractalDetector()
        
        # Processing queue for async handling
        self.processing_queue = queue.Queue(maxsize=10000)
        self.pattern_queue = queue.Queue(maxsize=1000)
        
    def start_consumer(self):
        """Start high-performance Kafka consumer"""
        logger.info("Starting high-performance fractal pattern consumer...")
        
        # Consumer configuration optimized for low latency
        self.consumer = KafkaConsumer(
            *self.topics,
            bootstrap_servers=self.bootstrap_servers,
            group_id='fractal-pattern-detector',
            enable_auto_commit=False,  # Manual commit for control
            auto_offset_reset='latest',
            consumer_timeout_ms=1000,
            fetch_min_bytes=1,  # Process immediately
            fetch_max_wait_ms=10,  # Low latency
            max_poll_records=1000,  # Batch processing
            value_deserializer=lambda m: json.loads(m.decode('utf-8')),
            key_deserializer=lambda k: k.decode('utf-8') if k else None
        )
        
        # Producer for pattern output
        self.producer = KafkaProducer(
            bootstrap_servers=self.bootstrap_servers,
            value_serializer=lambda v: json.dumps(v, separators=(',', ':')).encode('utf-8'),
            key_serializer=lambda k: k.encode('utf-8') if k else None,
            acks='all',
            retries=3,
            linger_ms=1,
            compression_type='lz4'
        )
        
        self.running = True
        
        # Start processing threads
        processing_thread = threading.Thread(target=self._process_messages)
        pattern_thread = threading.Thread(target=self._handle_patterns)
        throughput_thread = threading.Thread(target=self._update_throughput)
        
        processing_thread.start()
        pattern_thread.start()  
        throughput_thread.start()
        
        # Start metrics server
        start_http_server(8001)
        logger.info("Metrics server started on port 8001")
        
        # Main consumer loop
        try:
            for message in self.consumer:
                if not self.running:
                    break
                
                # Add to processing queue
                try:
                    self.processing_queue.put_nowait((message.timestamp, message.value))
                    self.messages_processed += 1
                except queue.Full:
                    logger.warning("Processing queue full, dropping message")
                
                # Manual commit for control
                if self.messages_processed % 100 == 0:
                    self.consumer.commit_async()
                    
        except KeyboardInterrupt:
            logger.info("Consumer interrupted by user")
        finally:
            self.stop_consumer()
        
        # Wait for threads to finish
        processing_thread.join()
        pattern_thread.join()
        throughput_thread.join()
    
    def _process_messages(self):
        """Process messages in separate thread for better performance"""
        while self.running:
            try:
                timestamp, message_data = self.processing_queue.get(timeout=1)
                
                # Parse market tick
                tick = MarketTick(**message_data)
                
                # Detect patterns
                patterns = self.fractal_detector.add_tick(tick)
                
                # Queue patterns for output
                for pattern in patterns:
                    try:
                        self.pattern_queue.put_nowait(pattern)
                    except queue.Full:
                        logger.warning("Pattern queue full, dropping pattern")
                
                self.processing_queue.task_done()
                
            except queue.Empty:
                continue
            except Exception as e:
                logger.error(f"Message processing error: {e}")
    
    def _handle_patterns(self):
        """Handle detected patterns in separate thread"""
        while self.running:
            try:
                pattern = self.pattern_queue.get(timeout=1)
                
                # Send pattern to output topic
                pattern_data = asdict(pattern)
                
                self.producer.send(
                    topic='fractal-patterns',
                    key=pattern.symbol,
                    value=pattern_data
                )
                
                # Log significant patterns
                if pattern.confidence > 0.8 or pattern.risk_score > 1.5:
                    logger.info(
                        f"Pattern detected: {pattern.symbol} - {pattern.pattern_type} "
                        f"(Conf: {pattern.confidence:.2f}, Risk: {pattern.risk_score:.2f})"
                    )
                
                self.pattern_queue.task_done()
                
            except queue.Empty:
                continue
            except Exception as e:
                logger.error(f"Pattern handling error: {e}")
    
    def _update_throughput(self):
        """Update throughput metrics periodically"""
        while self.running:
            time.sleep(5)  # Update every 5 seconds
            
            current_time = time.time()
            elapsed = current_time - self.last_throughput_update
            
            if elapsed > 0:
                throughput = self.messages_processed / (current_time - self.start_time)
                CURRENT_THROUGHPUT.set(throughput)
                
                logger.info(
                    f"Processing: {self.messages_processed:,} msgs, "
                    f"Throughput: {throughput:.1f} msg/s, "
                    f"Patterns: {self.fractal_detector.patterns_detected}, "
                    f"Avg latency: {statistics.mean(self.fractal_detector.processing_times[-100:]):.4f}s"
                )
            
            self.last_throughput_update = current_time
    
    def stop_consumer(self):
        """Stop consumer and cleanup"""
        logger.info("Stopping fractal pattern consumer...")
        self.running = False
        
        if self.consumer:
            self.consumer.close()
        if self.producer:
            self.producer.flush()
            self.producer.close()
        
        # Final statistics
        total_time = time.time() - self.start_time
        avg_throughput = self.messages_processed / max(total_time, 0.001)
        
        logger.info(f"Final Statistics:")
        logger.info(f"  Messages processed: {self.messages_processed:,}")
        logger.info(f"  Patterns detected: {self.fractal_detector.patterns_detected}")
        logger.info(f"  Average throughput: {avg_throughput:.1f} msg/s")
        logger.info(f"  Total runtime: {total_time:.1f}s")

def signal_handler(signum, frame):
    """Handle shutdown signals gracefully"""
    logger.info("Received shutdown signal")
    sys.exit(0)

def main():
    """Main entry point for real-time fractal pattern consumer"""
    parser = argparse.ArgumentParser(description='Real-Time Fractal Pattern Consumer')
    parser.add_argument('--kafka-servers', default='localhost:9092',
                       help='Kafka bootstrap servers')
    parser.add_argument('--topics', nargs='+', default=['market-ticks'],
                       help='Kafka topics to consume')
    parser.add_argument('--log-level', choices=['DEBUG', 'INFO', 'WARNING', 'ERROR'],
                       default='INFO', help='Logging level')
    
    args = parser.parse_args()
    
    # Set logging level
    logging.getLogger().setLevel(getattr(logging, args.log_level))
    
    # Setup signal handlers
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)
    
    # Create and start consumer
    consumer = HighPerformanceKafkaConsumer(
        bootstrap_servers=args.kafka_servers,
        topics=args.topics
    )
    
    try:
        consumer.start_consumer()
    except Exception as e:
        logger.error(f"Consumer failed: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())
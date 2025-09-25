#!/usr/bin/env python3
"""
HFT Trading Simulator with Fractal Analysis

This got way more complex than I originally planned. Started as a simple
order execution simulator and ended up building a full HFT engine with
real-time pattern recognition and portfolio management.

Warning: This is simulation code - don't use this with real money without
serious testing and risk management.
"""

import json
import time
import threading
import queue
import asyncio
from concurrent.futures import ThreadPoolExecutor, ProcessPoolExecutor
from collections import defaultdict, deque
from dataclasses import dataclass, asdict
from typing import Dict, List, Optional, Tuple, Any
from enum import Enum
import statistics
import math
import random
import numpy as np
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import KafkaError
import logging
import argparse
from datetime import datetime, timedelta
import signal
import sys
import psutil
import redis
from prometheus_client import Counter, Histogram, Gauge, start_http_server

# Performance Metrics
TRADES_EXECUTED = Counter('hft_trades_executed_total', 'Total HFT trades executed')
TRADE_LATENCY = Histogram('hft_trade_latency_microseconds', 'HFT trade execution latency')
PORTFOLIO_VALUE = Gauge('hft_portfolio_value_usd', 'Current portfolio value in USD')
RISK_EXPOSURE = Gauge('hft_risk_exposure', 'Current risk exposure level')
FRACTAL_SIGNALS = Counter('hft_fractal_signals_total', 'Total fractal trading signals')
EXECUTION_ERRORS = Counter('hft_execution_errors_total', 'Total execution errors')

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class OrderType(Enum):
    MARKET = "market"
    LIMIT = "limit"
    STOP_LOSS = "stop_loss"
    TAKE_PROFIT = "take_profit"

class OrderSide(Enum):
    BUY = "buy"
    SELL = "sell"

class ExecutionStatus(Enum):
    PENDING = "pending"
    FILLED = "filled"
    PARTIAL = "partial"
    CANCELLED = "cancelled"
    REJECTED = "rejected"

@dataclass
class Order:
    """High-frequency trading order"""
    order_id: str
    symbol: str
    side: OrderSide
    order_type: OrderType
    quantity: float
    price: Optional[float]
    timestamp_us: int
    client_id: str
    execution_status: ExecutionStatus = ExecutionStatus.PENDING
    filled_quantity: float = 0.0
    average_fill_price: float = 0.0
    commission: float = 0.0
    fractal_signal: Optional[str] = None
    risk_score: float = 0.0

@dataclass
class Position:
    """Trading position"""
    symbol: str
    quantity: float
    average_price: float
    market_value: float
    unrealized_pnl: float
    realized_pnl: float
    last_update_us: int

@dataclass
class TradeExecution:
    """Trade execution record"""
    trade_id: str
    order_id: str
    symbol: str
    side: OrderSide
    quantity: float
    price: float
    timestamp_us: int
    execution_latency_us: int
    commission: float
    fractal_pattern: Optional[str] = None

class FractalSignalProcessor:
    """Processes fractal patterns into trading signals"""
    
    def __init__(self):
        self.signal_history = defaultdict(deque)
        self.confidence_threshold = 0.7
        self.risk_threshold = 1.5
        
    def process_fractal_pattern(self, pattern_data: Dict[str, Any]) -> Optional[Dict[str, Any]]:
        """Convert fractal pattern into actionable trading signal"""
        try:
            symbol = pattern_data['symbol']
            pattern_type = pattern_data['pattern_type']
            confidence = pattern_data['confidence']
            risk_score = pattern_data['risk_score']
            prediction_signal = pattern_data['prediction_signal']
            
            # Filter low-confidence signals
            if confidence < self.confidence_threshold:
                return None
            
            # Risk management
            if risk_score > self.risk_threshold:
                logger.warning(f"High risk signal filtered: {symbol} - Risk: {risk_score:.2f}")
                return None
            
            # Signal strength calculation
            signal_strength = self._calculate_signal_strength(
                pattern_type, confidence, risk_score, prediction_signal
            )
            
            # Position sizing based on signal strength and risk
            position_size = self._calculate_position_size(signal_strength, risk_score)
            
            trading_signal = {
                'symbol': symbol,
                'action': prediction_signal,
                'signal_strength': signal_strength,
                'position_size': position_size,
                'confidence': confidence,
                'risk_score': risk_score,
                'pattern_type': pattern_type,
                'timestamp_us': int(time.time() * 1_000_000),
                'priority': 'HIGH' if signal_strength > 0.8 else 'MEDIUM'
            }
            
            # Store signal history
            self.signal_history[symbol].append(trading_signal)
            if len(self.signal_history[symbol]) > 100:
                self.signal_history[symbol].popleft()
            
            return trading_signal
            
        except Exception as e:
            logger.error(f"Signal processing error: {e}")
            return None
    
    def _calculate_signal_strength(self, pattern_type: str, confidence: float, 
                                 risk_score: float, prediction_signal: str) -> float:
        """Calculate trading signal strength"""
        # Base strength from confidence
        strength = confidence
        
        # Pattern type modifiers
        pattern_multipliers = {
            'VOLATILE_BREAKOUT': 1.2,
            'VOLATILE_UPTREND': 1.1,
            'VOLATILE_DOWNTREND': 1.1,
            'TRENDING_FRACTAL': 1.05,
            'SMOOTH_TREND': 0.9,
            'VOLATILE_RANGE': 0.8,
            'CHOPPY': 0.6
        }
        
        strength *= pattern_multipliers.get(pattern_type, 1.0)
        
        # Signal type modifiers
        signal_multipliers = {
            'STRONG_BUY': 1.3,
            'STRONG_SELL': 1.3,
            'BUY': 1.1,
            'SELL': 1.1,
            'HOLD': 0.7,
            'NEUTRAL': 0.5,
            'AVOID': 0.2
        }
        
        strength *= signal_multipliers.get(prediction_signal, 1.0)
        
        # Risk adjustment
        risk_penalty = min(0.3, risk_score * 0.15)
        strength -= risk_penalty
        
        return max(0.0, min(1.0, strength))
    
    def _calculate_position_size(self, signal_strength: float, risk_score: float) -> float:
        """Calculate position size based on signal and risk"""
        base_size = 1000.0  # Base position size
        
        # Strength multiplier
        strength_multiplier = 0.5 + (signal_strength * 1.5)
        
        # Risk adjustment
        risk_multiplier = max(0.2, 1.0 - (risk_score * 0.3))
        
        position_size = base_size * strength_multiplier * risk_multiplier
        
        # Position size limits
        return max(100.0, min(10000.0, position_size))

class PortfolioManager:
    """High-performance portfolio and risk management"""
    
    def __init__(self, initial_capital: float = 1_000_000.0):
        self.initial_capital = initial_capital
        self.current_capital = initial_capital
        self.positions = {}  # symbol -> Position
        self.orders = {}     # order_id -> Order
        self.trades = deque(maxlen=10000)  # Trade history
        
        # Risk management parameters
        self.max_position_size = 0.05  # 5% of portfolio per position
        self.max_total_exposure = 0.8  # 80% max exposure
        self.stop_loss_percentage = 0.02  # 2% stop loss
        self.take_profit_percentage = 0.04  # 4% take profit
        
        # Performance tracking
        self.total_trades = 0
        self.winning_trades = 0
        self.total_pnl = 0.0
        self.max_drawdown = 0.0
        self.peak_portfolio_value = initial_capital
        
        self.lock = threading.RLock()
    
    def calculate_portfolio_value(self, current_prices: Dict[str, float]) -> float:
        """Calculate current portfolio value"""
        with self.lock:
            total_value = self.current_capital
            
            for symbol, position in self.positions.items():
                if symbol in current_prices:
                    market_value = position.quantity * current_prices[symbol]
                    total_value += market_value
                    
                    # Update position
                    position.market_value = market_value
                    position.unrealized_pnl = market_value - (position.quantity * position.average_price)
            
            return total_value
    
    def check_risk_limits(self, order: Order, current_price: float) -> Tuple[bool, str]:
        """Check if order passes risk management rules"""
        with self.lock:
            # Position size check
            order_value = order.quantity * current_price
            portfolio_value = self.current_capital + sum(
                pos.market_value for pos in self.positions.values()
            )
            
            position_percentage = order_value / max(portfolio_value, 1.0)
            if position_percentage > self.max_position_size:
                return False, f"Position size {position_percentage:.2%} exceeds limit {self.max_position_size:.2%}"
            
            # Total exposure check
            current_exposure = sum(abs(pos.market_value) for pos in self.positions.values())
            total_exposure = (current_exposure + order_value) / max(portfolio_value, 1.0)
            
            if total_exposure > self.max_total_exposure:
                return False, f"Total exposure {total_exposure:.2%} exceeds limit {self.max_total_exposure:.2%}"
            
            # Capital adequacy check
            if order.side == OrderSide.BUY and order_value > self.current_capital * 0.95:
                return False, "Insufficient capital for purchase"
            
            return True, "Risk checks passed"
    
    def update_position(self, trade: TradeExecution):
        """Update portfolio positions after trade execution"""
        with self.lock:
            symbol = trade.symbol
            
            if symbol not in self.positions:
                self.positions[symbol] = Position(
                    symbol=symbol,
                    quantity=0.0,
                    average_price=0.0,
                    market_value=0.0,
                    unrealized_pnl=0.0,
                    realized_pnl=0.0,
                    last_update_us=trade.timestamp_us
                )
            
            position = self.positions[symbol]
            
            if trade.side == OrderSide.BUY:
                # Long position
                new_quantity = position.quantity + trade.quantity
                if new_quantity != 0:
                    position.average_price = (
                        (position.quantity * position.average_price + 
                         trade.quantity * trade.price) / new_quantity
                    )
                position.quantity = new_quantity
                self.current_capital -= (trade.quantity * trade.price + trade.commission)
                
            else:  # SELL
                # Realize PnL
                realized_pnl = (trade.price - position.average_price) * trade.quantity
                position.realized_pnl += realized_pnl
                self.total_pnl += realized_pnl
                
                position.quantity -= trade.quantity
                self.current_capital += (trade.quantity * trade.price - trade.commission)
                
                # Track winning trades
                if realized_pnl > 0:
                    self.winning_trades += 1
            
            position.last_update_us = trade.timestamp_us
            self.total_trades += 1
            
            # Update performance metrics
            current_portfolio_value = self.calculate_portfolio_value({symbol: trade.price})
            PORTFOLIO_VALUE.set(current_portfolio_value)
            
            # Track drawdown
            if current_portfolio_value > self.peak_portfolio_value:
                self.peak_portfolio_value = current_portfolio_value
            else:
                drawdown = (self.peak_portfolio_value - current_portfolio_value) / self.peak_portfolio_value
                self.max_drawdown = max(self.max_drawdown, drawdown)

class OrderExecutionEngine:
    """Microsecond-precision order execution engine"""
    
    def __init__(self, portfolio_manager: PortfolioManager):
        self.portfolio_manager = portfolio_manager
        self.execution_queue = queue.PriorityQueue()
        self.market_data = {}  # symbol -> current_price
        self.execution_latencies = deque(maxlen=10000)
        
        # Execution parameters
        self.slippage_model = 0.0001  # 1 basis point average slippage
        self.commission_rate = 0.0005  # 5 basis points commission
        
        # Performance optimization
        self.executor = ThreadPoolExecutor(max_workers=10)
        self.running = False
        
    def start_execution_engine(self):
        """Start the order execution engine"""
        self.running = True
        execution_thread = threading.Thread(target=self._process_execution_queue)
        execution_thread.start()
        logger.info("Order execution engine started")
        
    def stop_execution_engine(self):
        """Stop the order execution engine"""
        self.running = False
        
    def submit_order(self, order: Order, priority: int = 1):
        """Submit order for execution with priority"""
        try:
            # Add timestamp for latency tracking
            order.timestamp_us = int(time.time() * 1_000_000)
            self.execution_queue.put((priority, order))
        except Exception as e:
            logger.error(f"Order submission error: {e}")
            EXECUTION_ERRORS.inc()
    
    def update_market_data(self, symbol: str, price: float):
        """Update market data for execution"""
        self.market_data[symbol] = price
    
    def _process_execution_queue(self):
        """Process execution queue in separate thread"""
        while self.running:
            try:
                # Get order from queue with timeout
                priority, order = self.execution_queue.get(timeout=1)
                
                # Execute order asynchronously
                self.executor.submit(self._execute_order, order)
                
            except queue.Empty:
                continue
            except Exception as e:
                logger.error(f"Execution queue processing error: {e}")
                EXECUTION_ERRORS.inc()
    
    def _execute_order(self, order: Order):
        """Execute individual order with latency tracking"""
        start_time = time.time()
        start_time_us = int(start_time * 1_000_000)
        
        try:
            # Get current market price
            if order.symbol not in self.market_data:
                logger.error(f"No market data for {order.symbol}")
                order.execution_status = ExecutionStatus.REJECTED
                EXECUTION_ERRORS.inc()
                return
            
            current_price = self.market_data[order.symbol]
            
            # Risk management checks
            risk_passed, risk_message = self.portfolio_manager.check_risk_limits(order, current_price)
            if not risk_passed:
                logger.warning(f"Order rejected: {risk_message}")
                order.execution_status = ExecutionStatus.REJECTED
                return
            
            # Determine execution price
            execution_price = self._calculate_execution_price(order, current_price)
            
            # Calculate commission
            commission = order.quantity * execution_price * self.commission_rate
            
            # Create trade execution record
            execution_time_us = int(time.time() * 1_000_000)
            execution_latency_us = execution_time_us - start_time_us
            
            trade = TradeExecution(
                trade_id=f"T{execution_time_us}_{order.order_id}",
                order_id=order.order_id,
                symbol=order.symbol,
                side=order.side,
                quantity=order.quantity,
                price=execution_price,
                timestamp_us=execution_time_us,
                execution_latency_us=execution_latency_us,
                commission=commission,
                fractal_pattern=order.fractal_signal
            )
            
            # Update portfolio
            self.portfolio_manager.update_position(trade)
            
            # Update order status
            order.execution_status = ExecutionStatus.FILLED
            order.filled_quantity = order.quantity
            order.average_fill_price = execution_price
            order.commission = commission
            
            # Track metrics
            TRADES_EXECUTED.inc()
            TRADE_LATENCY.observe(execution_latency_us / 1_000_000)  # Convert to seconds
            self.execution_latencies.append(execution_latency_us)
            
            logger.info(
                f"Trade executed: {order.symbol} {order.side.value} {order.quantity} @ {execution_price:.4f} "
                f"(Latency: {execution_latency_us}μs)"
            )
            
        except Exception as e:
            logger.error(f"Order execution error: {e}")
            order.execution_status = ExecutionStatus.REJECTED
            EXECUTION_ERRORS.inc()
    
    def _calculate_execution_price(self, order: Order, current_price: float) -> float:
        """Calculate realistic execution price with slippage"""
        base_price = current_price
        
        if order.order_type == OrderType.MARKET:
            # Market orders experience slippage
            slippage_factor = random.gauss(0, self.slippage_model)
            
            if order.side == OrderSide.BUY:
                # Buy orders typically execute above mid
                slippage_factor = abs(slippage_factor)
            else:
                # Sell orders typically execute below mid  
                slippage_factor = -abs(slippage_factor)
                
            execution_price = base_price * (1 + slippage_factor)
            
        elif order.order_type == OrderType.LIMIT:
            # Limit orders execute at limit price or better
            if order.price is None:
                execution_price = base_price
            else:
                if order.side == OrderSide.BUY:
                    execution_price = min(order.price, base_price)
                else:
                    execution_price = max(order.price, base_price)
        else:
            execution_price = base_price
            
        return round(execution_price, 4)

class DistributedHFTEngine:
    """Main distributed high-frequency trading engine"""
    
    def __init__(self, kafka_servers: str = 'localhost:9092'):
        self.kafka_servers = kafka_servers
        self.running = False
        
        # Core components
        self.portfolio_manager = PortfolioManager()
        self.execution_engine = OrderExecutionEngine(self.portfolio_manager)
        self.signal_processor = FractalSignalProcessor()
        
        # Kafka connections
        self.market_consumer = None
        self.pattern_consumer = None
        self.order_producer = None
        
        # Performance tracking
        self.signals_processed = 0
        self.orders_generated = 0
        self.start_time = time.time()
        
        # Redis for low-latency cache (optional)
        try:
            self.redis_client = redis.Redis(host='localhost', port=6379, db=0, decode_responses=True)
            self.redis_available = True
        except:
            self.redis_available = False
            logger.warning("Redis not available - running without cache")
    
    def start_engine(self):
        """Start the complete HFT trading engine"""
        logger.info("Starting Distributed HFT Trading Engine...")
        
        # Start portfolio manager and execution engine
        self.execution_engine.start_execution_engine()
        
        # Initialize Kafka consumers
        self._initialize_kafka_connections()
        
        # Start processing threads
        self.running = True
        
        threads = [
            threading.Thread(target=self._consume_market_data, name="MarketDataConsumer"),
            threading.Thread(target=self._consume_fractal_patterns, name="PatternConsumer"),
            threading.Thread(target=self._performance_monitor, name="PerformanceMonitor"),
        ]
        
        for thread in threads:
            thread.start()
        
        # Start metrics server
        start_http_server(8002)
        logger.info("HFT Engine metrics server started on port 8002")
        
        try:
            # Main engine loop
            while self.running:
                time.sleep(1)  # Main loop heartbeat
                
        except KeyboardInterrupt:
            logger.info("HFT Engine interrupted by user")
        finally:
            self.stop_engine()
        
        # Wait for threads to complete
        for thread in threads:
            thread.join(timeout=5)
    
    def _initialize_kafka_connections(self):
        """Initialize Kafka producers and consumers"""
        # Market data consumer
        self.market_consumer = KafkaConsumer(
            'market-ticks',
            bootstrap_servers=self.kafka_servers,
            group_id='hft-market-data',
            enable_auto_commit=True,
            auto_offset_reset='latest',
            fetch_min_bytes=1,
            fetch_max_wait_ms=5,  # Very low latency
            value_deserializer=lambda m: json.loads(m.decode('utf-8'))
        )
        
        # Fractal pattern consumer
        self.pattern_consumer = KafkaConsumer(
            'fractal-patterns',
            bootstrap_servers=self.kafka_servers,
            group_id='hft-pattern-consumer',
            enable_auto_commit=True,
            auto_offset_reset='latest',
            fetch_min_bytes=1,
            fetch_max_wait_ms=5,
            value_deserializer=lambda m: json.loads(m.decode('utf-8'))
        )
        
        # Order producer
        self.order_producer = KafkaProducer(
            bootstrap_servers=self.kafka_servers,
            value_serializer=lambda v: json.dumps(v, separators=(',', ':')).encode('utf-8'),
            acks='all',
            retries=3,
            linger_ms=0,  # No batching for low latency
            compression_type='lz4'
        )
        
        logger.info("Kafka connections initialized")
    
    def _consume_market_data(self):
        """Consume real-time market data"""
        logger.info("Starting market data consumer...")
        
        try:
            for message in self.market_consumer:
                if not self.running:
                    break
                
                try:
                    tick_data = message.value
                    symbol = tick_data['symbol']
                    price = tick_data['price']
                    
                    # Update execution engine market data
                    self.execution_engine.update_market_data(symbol, price)
                    
                    # Cache in Redis for ultra-low latency
                    if self.redis_available:
                        self.redis_client.hset(f"price:{symbol}", mapping={
                            'price': price,
                            'timestamp': tick_data['timestamp_us']
                        })
                    
                except Exception as e:
                    logger.error(f"Market data processing error: {e}")
                    
        except Exception as e:
            logger.error(f"Market data consumer error: {e}")
    
    def _consume_fractal_patterns(self):
        """Consume fractal patterns and generate trading signals"""
        logger.info("Starting fractal pattern consumer...")
        
        try:
            for message in self.pattern_consumer:
                if not self.running:
                    break
                
                try:
                    pattern_data = message.value
                    
                    # Process pattern into trading signal
                    signal = self.signal_processor.process_fractal_pattern(pattern_data)
                    
                    if signal:
                        self.signals_processed += 1
                        FRACTAL_SIGNALS.inc()
                        
                        # Generate trading order
                        order = self._create_order_from_signal(signal)
                        
                        if order:
                            # Submit order for execution
                            priority = 0 if signal['priority'] == 'HIGH' else 1
                            self.execution_engine.submit_order(order, priority)
                            self.orders_generated += 1
                            
                            # Publish order to Kafka
                            self.order_producer.send(
                                topic='hft-orders',
                                key=signal['symbol'],
                                value=asdict(order)
                            )
                    
                except Exception as e:
                    logger.error(f"Pattern processing error: {e}")
                    
        except Exception as e:
            logger.error(f"Pattern consumer error: {e}")
    
    def _create_order_from_signal(self, signal: Dict[str, Any]) -> Optional[Order]:
        """Create trading order from fractal signal"""
        try:
            symbol = signal['symbol']
            action = signal['action']
            position_size = signal['position_size']
            
            # Determine order side
            if action in ['BUY', 'STRONG_BUY']:
                side = OrderSide.BUY
            elif action in ['SELL', 'STRONG_SELL']:
                side = OrderSide.SELL
            else:
                return None  # No actionable signal
            
            # Create order
            order_id = f"HFT_{int(time.time() * 1_000_000)}_{symbol}"
            
            order = Order(
                order_id=order_id,
                symbol=symbol,
                side=side,
                order_type=OrderType.MARKET,  # Market orders for HFT
                quantity=position_size,
                price=None,
                timestamp_us=signal['timestamp_us'],
                client_id='HFT_ENGINE',
                fractal_signal=signal['pattern_type'],
                risk_score=signal['risk_score']
            )
            
            return order
            
        except Exception as e:
            logger.error(f"Order creation error: {e}")
            return None
    
    def _performance_monitor(self):
        """Monitor and report performance metrics"""
        while self.running:
            try:
                time.sleep(10)  # Report every 10 seconds
                
                # Calculate performance metrics
                runtime = time.time() - self.start_time
                signal_rate = self.signals_processed / max(runtime, 1)
                order_rate = self.orders_generated / max(runtime, 1)
                
                # Portfolio metrics
                portfolio_value = self.portfolio_manager.calculate_portfolio_value(
                    self.execution_engine.market_data
                )
                
                total_return = (portfolio_value - self.portfolio_manager.initial_capital) / self.portfolio_manager.initial_capital
                win_rate = (self.portfolio_manager.winning_trades / 
                           max(self.portfolio_manager.total_trades, 1)) * 100
                
                # Execution latency stats
                if self.execution_engine.execution_latencies:
                    avg_latency_us = statistics.mean(self.execution_engine.execution_latencies)
                    p95_latency_us = np.percentile(self.execution_engine.execution_latencies, 95)
                else:
                    avg_latency_us = p95_latency_us = 0
                
                # Update Prometheus metrics
                PORTFOLIO_VALUE.set(portfolio_value)
                current_exposure = sum(
                    abs(pos.market_value) for pos in self.portfolio_manager.positions.values()
                ) / max(portfolio_value, 1.0)
                RISK_EXPOSURE.set(current_exposure)
                
                # Log performance summary
                logger.info(
                    f"HFT Performance: Portfolio: ${portfolio_value:,.0f} ({total_return:+.2%}), "
                    f"Trades: {self.portfolio_manager.total_trades} (Win: {win_rate:.1f}%), "
                    f"Signals: {self.signals_processed} ({signal_rate:.1f}/s), "
                    f"Orders: {self.orders_generated} ({order_rate:.1f}/s), "
                    f"Latency: {avg_latency_us:.0f}μs avg, {p95_latency_us:.0f}μs p95"
                )
                
            except Exception as e:
                logger.error(f"Performance monitoring error: {e}")
    
    def stop_engine(self):
        """Stop the HFT trading engine"""
        logger.info("Stopping HFT Trading Engine...")
        self.running = False
        
        # Stop execution engine
        self.execution_engine.stop_execution_engine()
        
        # Close Kafka connections
        if self.market_consumer:
            self.market_consumer.close()
        if self.pattern_consumer:
            self.pattern_consumer.close()
        if self.order_producer:
            self.order_producer.flush()
            self.order_producer.close()
        
        # Final performance report
        runtime = time.time() - self.start_time
        final_portfolio_value = self.portfolio_manager.calculate_portfolio_value(
            self.execution_engine.market_data
        )
        total_return = (final_portfolio_value - self.portfolio_manager.initial_capital) / self.portfolio_manager.initial_capital
        
        logger.info("=== HFT ENGINE FINAL REPORT ===")
        logger.info(f"Runtime: {runtime:.1f}s")
        logger.info(f"Initial Capital: ${self.portfolio_manager.initial_capital:,.0f}")
        logger.info(f"Final Portfolio Value: ${final_portfolio_value:,.0f}")
        logger.info(f"Total Return: {total_return:+.2%}")
        logger.info(f"Total Trades: {self.portfolio_manager.total_trades}")
        logger.info(f"Win Rate: {(self.portfolio_manager.winning_trades/max(self.portfolio_manager.total_trades,1))*100:.1f}%")
        logger.info(f"Max Drawdown: {self.portfolio_manager.max_drawdown:.2%}")
        logger.info(f"Signals Processed: {self.signals_processed}")
        logger.info(f"Orders Generated: {self.orders_generated}")

def signal_handler(signum, frame):
    """Handle shutdown signals gracefully"""
    logger.info("Received shutdown signal")
    sys.exit(0)

def main():
    """Main entry point for HFT simulation engine"""
    parser = argparse.ArgumentParser(description='Distributed HFT Simulation Engine')
    parser.add_argument('--kafka-servers', default='localhost:9092',
                       help='Kafka bootstrap servers')
    parser.add_argument('--initial-capital', type=float, default=1_000_000,
                       help='Initial trading capital')
    parser.add_argument('--log-level', choices=['DEBUG', 'INFO', 'WARNING', 'ERROR'],
                       default='INFO', help='Logging level')
    
    args = parser.parse_args()
    
    # Set logging level
    logging.getLogger().setLevel(getattr(logging, args.log_level))
    
    # Setup signal handlers
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)
    
    # Create and start HFT engine
    engine = DistributedHFTEngine(kafka_servers=args.kafka_servers)
    engine.portfolio_manager.initial_capital = args.initial_capital
    engine.portfolio_manager.current_capital = args.initial_capital
    
    try:
        engine.start_engine()
    except Exception as e:
        logger.error(f"HFT Engine failed: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())
#!/usr/bin/env python3
"""
Distributed Fractal Analysis with Apache Spark
Processes massive datasets with distributed box-counting algorithms

Enhanced system scalability enabling large-scale HFT analysis
"""

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import *
from pyspark.sql.types import *
from pyspark.sql.window import Window
import pyspark.sql.functions as F
from pyspark.ml.feature import VectorAssembler
from pyspark.ml.clustering import KMeans
import numpy as np
import math
from typing import List, Tuple, Optional
import argparse
import logging
import time
from concurrent.futures import ThreadPoolExecutor

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Custom UDFs for fractal analysis
def box_counting_fractal_dimension(prices: List[float], box_sizes: List[int] = None) -> float:
    """
    Distributed box-counting fractal dimension calculation
    Optimized for Spark DataFrame operations
    """
    if not prices or len(prices) < 10:
        return 1.0
    
    if box_sizes is None:
        box_sizes = [1, 2, 3, 4, 5, 8, 10, 16, 20, 25, 32, 50]
    
    # Normalize prices to [0, 1] range
    min_price = min(prices)
    max_price = max(prices)
    price_range = max_price - min_price
    
    if price_range == 0:
        return 1.0
    
    normalized_prices = [(p - min_price) / price_range for p in prices]
    
    log_inv_sizes = []
    log_box_counts = []
    
    for box_size in box_sizes:
        if box_size >= len(prices) // 3:
            break
            
        # Count unique boxes covering the price curve
        boxes = set()
        for i in range(len(normalized_prices) - 1):
            x = i // box_size
            y = int(normalized_prices[i] * box_size)
            boxes.add((x, y))
        
        if len(boxes) > 0:
            log_inv_sizes.append(math.log(1.0 / box_size))
            log_box_counts.append(math.log(len(boxes)))
    
    if len(log_inv_sizes) < 3:
        return 1.0
    
    # Linear regression to find slope (fractal dimension)
    n = len(log_inv_sizes)
    sum_x = sum(log_inv_sizes)
    sum_y = sum(log_box_counts)
    sum_xy = sum(x * y for x, y in zip(log_inv_sizes, log_box_counts))
    sum_xx = sum(x * x for x in log_inv_sizes)
    
    denominator = n * sum_xx - sum_x * sum_x
    if abs(denominator) < 1e-10:
        return 1.0
    
    slope = (n * sum_xy - sum_x * sum_y) / denominator
    return max(1.0, min(2.0, slope))  # Clamp to valid fractal dimension range

def volatility_fractal_correlation(volatilities: List[float], window: int = 20) -> float:
    """Calculate correlation between volatility and fractal behavior"""
    if len(volatilities) < window * 2:
        return 0.0
    
    # Calculate autocorrelation at different lags
    correlations = []
    for lag in [1, 5, 10]:
        if lag >= len(volatilities):
            continue
        
        x = volatilities[:-lag] if lag > 0 else volatilities
        y = volatilities[lag:]
        
        if len(x) != len(y) or len(x) == 0:
            continue
        
        mean_x = sum(x) / len(x)
        mean_y = sum(y) / len(y)
        
        numerator = sum((a - mean_x) * (b - mean_y) for a, b in zip(x, y))
        denom_x = sum((a - mean_x) ** 2 for a in x)
        denom_y = sum((b - mean_y) ** 2 for b in y)
        
        if denom_x > 0 and denom_y > 0:
            correlation = numerator / math.sqrt(denom_x * denom_y)
            correlations.append(abs(correlation))
    
    return sum(correlations) / max(len(correlations), 1)

def detect_fractal_patterns(prices: List[float], fd_threshold: float = 1.5) -> str:
    """Detect specific fractal patterns in price series"""
    if len(prices) < 50:
        return "INSUFFICIENT_DATA"
    
    fd = box_counting_fractal_dimension(prices)
    
    # Analyze price movement characteristics
    returns = [(prices[i] - prices[i-1]) / prices[i-1] for i in range(1, len(prices))]
    volatility = math.sqrt(sum(r*r for r in returns) / len(returns))
    
    # Pattern classification based on fractal properties
    if fd < 1.2:
        return "TRENDING" if abs(returns[-10:]) > volatility else "SMOOTH"
    elif fd > 1.7:
        return "HIGHLY_VOLATILE" 
    elif volatility > 0.02:
        trend_direction = sum(returns[-20:])
        return "VOLATILE_UPTREND" if trend_direction > 0 else "VOLATILE_DOWNTREND"
    else:
        return "NORMAL_FRACTAL"

# Register UDFs
from pyspark.sql.functions import udf
box_counting_udf = udf(box_counting_fractal_dimension, FloatType())
volatility_correlation_udf = udf(volatility_fractal_correlation, FloatType()) 
pattern_detection_udf = udf(detect_fractal_patterns, StringType())

class DistributedFractalAnalyzer:
    """Distributed fractal analysis engine using Apache Spark"""
    
    def __init__(self, app_name: str = "FractalAnalysis", master: str = "local[*]"):
        self.spark = SparkSession.builder \
            .appName(app_name) \
            .master(master) \
            .config("spark.sql.adaptive.enabled", "true") \
            .config("spark.sql.adaptive.coalescePartitions.enabled", "true") \
            .config("spark.sql.adaptive.skewJoin.enabled", "true") \
            .config("spark.serializer", "org.apache.spark.serializer.KryoSerializer") \
            .config("spark.sql.streaming.checkpointLocation", "/tmp/spark-checkpoint") \
            .getOrCreate()
        
        self.spark.sparkContext.setLogLevel("WARN")
        logger.info(f"Spark session created with {self.spark.sparkContext.defaultParallelism} cores")
    
    def load_kafka_stream(self, kafka_servers: str = "localhost:9092", 
                         topic: str = "market-ticks") -> DataFrame:
        """Load real-time streaming data from Kafka"""
        return self.spark \
            .readStream \
            .format("kafka") \
            .option("kafka.bootstrap.servers", kafka_servers) \
            .option("subscribe", topic) \
            .option("startingOffsets", "latest") \
            .option("failOnDataLoss", "false") \
            .load()
    
    def parse_market_data(self, kafka_df: DataFrame) -> DataFrame:
        """Parse JSON market data from Kafka stream"""
        # Define schema for market tick data
        schema = StructType([
            StructField("timestamp_us", LongType(), True),
            StructField("symbol", StringType(), True),
            StructField("price", FloatType(), True),
            StructField("volume", IntegerType(), True),
            StructField("bid", FloatType(), True),
            StructField("ask", FloatType(), True),
            StructField("spread", FloatType(), True),
            StructField("volatility", FloatType(), True),
            StructField("fractal_dimension", FloatType(), True),
            StructField("sequence_id", LongType(), True),
            StructField("exchange", StringType(), True)
        ])
        
        return kafka_df.select(
            from_json(col("value").cast("string"), schema).alias("data"),
            col("timestamp").alias("kafka_timestamp")
        ).select("data.*", "kafka_timestamp")
    
    def create_sliding_windows(self, df: DataFrame, window_sizes: List[int]) -> DataFrame:
        """Create sliding windows for fractal analysis"""
        # Convert microsecond timestamp to Spark timestamp
        df_with_ts = df.withColumn(
            "timestamp", 
            to_timestamp(col("timestamp_us") / 1000000)
        )
        
        results = []
        for window_size in window_sizes:
            window_spec = Window \
                .partitionBy("symbol") \
                .orderBy("timestamp") \
                .rowsBetween(-window_size, -1)
            
            windowed_df = df_with_ts \
                .withColumn(f"prices_{window_size}", 
                           collect_list("price").over(window_spec)) \
                .withColumn(f"volatilities_{window_size}", 
                           collect_list("volatility").over(window_spec)) \
                .filter(size(col(f"prices_{window_size}")) >= window_size)
            
            results.append(windowed_df)
        
        # Join all window sizes
        final_df = results[0]
        for df in results[1:]:
            final_df = final_df.join(df, ["symbol", "timestamp"], "outer")
        
        return final_df
    
    def compute_distributed_fractal_analysis(self, df: DataFrame) -> DataFrame:
        """Compute fractal analysis metrics across distributed partitions"""
        return df \
            .withColumn("fractal_dim_50", box_counting_udf(col("prices_50"))) \
            .withColumn("fractal_dim_100", box_counting_udf(col("prices_100"))) \
            .withColumn("fractal_dim_200", box_counting_udf(col("prices_200"))) \
            .withColumn("vol_correlation_50", volatility_correlation_udf(col("volatilities_50"))) \
            .withColumn("vol_correlation_100", volatility_correlation_udf(col("volatilities_100"))) \
            .withColumn("pattern_50", pattern_detection_udf(col("prices_50"))) \
            .withColumn("pattern_100", pattern_detection_udf(col("prices_100"))) \
            .withColumn("pattern_200", pattern_detection_udf(col("prices_200")))
    
    def aggregate_fractal_metrics(self, df: DataFrame) -> DataFrame:
        """Aggregate fractal metrics by symbol and time windows"""
        return df.groupBy(
            "symbol",
            window(col("timestamp"), "1 minute").alias("time_window")
        ).agg(
            avg("fractal_dim_50").alias("avg_fractal_dim_50"),
            avg("fractal_dim_100").alias("avg_fractal_dim_100"),
            avg("fractal_dim_200").alias("avg_fractal_dim_200"),
            avg("vol_correlation_50").alias("avg_vol_correlation_50"),
            avg("vol_correlation_100").alias("avg_vol_correlation_100"),
            count("*").alias("tick_count"),
            min("price").alias("min_price"),
            max("price").alias("max_price"),
            first("pattern_50").alias("dominant_pattern_50"),
            first("pattern_100").alias("dominant_pattern_100"),
            stddev("price").alias("price_volatility"),
            collect_list("fractal_dimension").alias("fractal_dimensions")
        )
    
    def detect_fractal_anomalies(self, df: DataFrame) -> DataFrame:
        """Detect fractal anomalies using statistical methods"""
        # Calculate z-scores for fractal dimensions
        stats_df = df.select(
            avg("avg_fractal_dim_100").alias("mean_fd"),
            stddev("avg_fractal_dim_100").alias("std_fd")
        ).collect()[0]
        
        mean_fd = stats_df["mean_fd"] or 1.5
        std_fd = stats_df["std_fd"] or 0.1
        
        return df.withColumn(
            "fractal_anomaly_score",
            abs(col("avg_fractal_dim_100") - mean_fd) / std_fd
        ).withColumn(
            "is_anomaly",
            col("fractal_anomaly_score") > 2.0
        )
    
    def run_batch_analysis(self, input_path: str, output_path: str):
        """Run batch fractal analysis on historical data"""
        logger.info(f"Starting batch fractal analysis: {input_path} -> {output_path}")
        
        # Load historical market data
        df = self.spark.read \
            .option("header", "true") \
            .option("inferSchema", "true") \
            .csv(input_path)
        
        # Create sliding windows
        windowed_df = self.create_sliding_windows(df, [50, 100, 200])
        
        # Compute fractal analysis
        analyzed_df = self.compute_distributed_fractal_analysis(windowed_df)
        
        # Aggregate metrics
        aggregated_df = self.aggregate_fractal_metrics(analyzed_df)
        
        # Detect anomalies
        final_df = self.detect_fractal_anomalies(aggregated_df)
        
        # Save results
        final_df.coalesce(10).write \
            .mode("overwrite") \
            .option("header", "true") \
            .csv(output_path)
        
        # Show sample results
        logger.info("Sample fractal analysis results:")
        final_df.select(
            "symbol", "time_window", 
            "avg_fractal_dim_100", "price_volatility", 
            "dominant_pattern_100", "is_anomaly"
        ).show(20, truncate=False)
        
        # Performance statistics
        total_rows = final_df.count()
        anomalies = final_df.filter(col("is_anomaly") == True).count()
        
        logger.info(f"Analysis complete:")
        logger.info(f"  Total windows analyzed: {total_rows:,}")
        logger.info(f"  Fractal anomalies detected: {anomalies} ({100*anomalies/max(total_rows,1):.1f}%)")
    
    def run_streaming_analysis(self, kafka_servers: str = "localhost:9092"):
        """Run real-time streaming fractal analysis"""
        logger.info("Starting streaming fractal analysis...")
        
        # Load streaming data
        kafka_df = self.load_kafka_stream(kafka_servers)
        parsed_df = self.parse_market_data(kafka_df)
        
        # Define streaming query for real-time analysis
        query = parsed_df \
            .writeStream \
            .outputMode("append") \
            .format("console") \
            .option("truncate", "false") \
            .option("numRows", 10) \
            .trigger(processingTime='10 seconds') \
            .start()
        
        # Also write to Kafka for downstream processing
        kafka_output_query = parsed_df \
            .selectExpr(
                "symbol as key",
                "to_json(struct(*)) as value"
            ) \
            .writeStream \
            .format("kafka") \
            .option("kafka.bootstrap.servers", kafka_servers) \
            .option("topic", "fractal-analysis-results") \
            .option("checkpointLocation", "/tmp/kafka-checkpoint") \
            .start()
        
        # Wait for termination
        try:
            query.awaitTermination()
            kafka_output_query.awaitTermination()
        except KeyboardInterrupt:
            logger.info("Streaming analysis interrupted")
            query.stop()
            kafka_output_query.stop()
    
    def run_ml_pattern_classification(self, df: DataFrame) -> DataFrame:
        """Apply ML clustering to identify fractal pattern families"""
        # Prepare features for ML
        feature_cols = [
            "avg_fractal_dim_50", "avg_fractal_dim_100", "avg_fractal_dim_200",
            "avg_vol_correlation_50", "avg_vol_correlation_100", "price_volatility"
        ]
        
        assembler = VectorAssembler(
            inputCols=feature_cols,
            outputCol="features",
            handleInvalid="skip"
        )
        
        feature_df = assembler.transform(df.na.fill(1.5))
        
        # K-means clustering for pattern families
        kmeans = KMeans(k=5, seed=42, featuresCol="features", predictionCol="pattern_cluster")
        model = kmeans.fit(feature_df)
        
        clustered_df = model.transform(feature_df)
        
        # Add cluster interpretation
        cluster_interpretation = {
            0: "STABLE_LOW_FRACTAL",
            1: "VOLATILE_HIGH_FRACTAL", 
            2: "TRENDING_MEDIUM_FRACTAL",
            3: "REVERSAL_PATTERN",
            4: "ANOMALOUS_BEHAVIOR"
        }
        
        # Create mapping UDF
        interpret_cluster_udf = udf(
            lambda x: cluster_interpretation.get(x, "UNKNOWN"), 
            StringType()
        )
        
        return clustered_df.withColumn(
            "pattern_interpretation",
            interpret_cluster_udf(col("pattern_cluster"))
        )
    
    def generate_fractal_insights(self, df: DataFrame) -> DataFrame:
        """Generate actionable insights from fractal analysis"""
        return df.withColumn(
            "trading_signal",
            when(col("fractal_anomaly_score") > 3.0, "STRONG_SIGNAL")
            .when(col("fractal_anomaly_score") > 2.0, "MODERATE_SIGNAL")
            .when(col("avg_fractal_dim_100") > 1.7, "HIGH_VOLATILITY_EXPECTED")
            .when(col("avg_fractal_dim_100") < 1.3, "TRENDING_CONTINUATION")
            .otherwise("NEUTRAL")
        ).withColumn(
            "confidence_score",
            greatest(
                col("avg_vol_correlation_50"),
                col("avg_vol_correlation_100")
            ) * 100
        )
    
    def close(self):
        """Close Spark session"""
        self.spark.stop()

def main():
    """Main entry point for distributed fractal analysis"""
    parser = argparse.ArgumentParser(description='Distributed Fractal Analysis with Spark')
    parser.add_argument('--mode', choices=['batch', 'streaming'], default='batch',
                       help='Analysis mode')
    parser.add_argument('--input', default='../csharp/out-csharp/market_data.csv',
                       help='Input data path for batch mode')
    parser.add_argument('--output', default='spark-results/',
                       help='Output path for results')
    parser.add_argument('--kafka-servers', default='localhost:9092',
                       help='Kafka bootstrap servers')
    parser.add_argument('--spark-master', default='local[*]',
                       help='Spark master URL')
    
    args = parser.parse_args()
    
    analyzer = DistributedFractalAnalyzer(
        app_name="FractalHFTAnalysis",
        master=args.spark_master
    )
    
    try:
        if args.mode == 'batch':
            analyzer.run_batch_analysis(args.input, args.output)
        else:
            analyzer.run_streaming_analysis(args.kafka_servers)
            
    except Exception as e:
        logger.error(f"Analysis failed: {e}")
        return 1
    finally:
        analyzer.close()
    
    return 0

if __name__ == "__main__":
    exit(main())
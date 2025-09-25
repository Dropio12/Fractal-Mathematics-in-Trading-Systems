using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalMarketDemo
{
    /// <summary>
    /// Represents market behavior analysis results
    /// </summary>
    public class MarketBehaviorAnalysis
    {
        public double OverallTrendStrength { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
        public double VolatilityClustering { get; set; }
        public double MeanReversion { get; set; }
        public double Momentum { get; set; }
        public List<TrendPeriod> TrendPeriods { get; set; } = new();
        public List<VolatilityCluster> VolatilityClusters { get; set; } = new();
    }

    /// <summary>
    /// Represents a detected trend period
    /// </summary>
    public class TrendPeriod
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Direction { get; set; } = string.Empty;
        public double Strength { get; set; }
        public double Persistence { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public int Duration => EndIndex - StartIndex + 1;
    }

    /// <summary>
    /// Represents a volatility cluster
    /// </summary>
    public class VolatilityCluster
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public double IntensityMultiplier { get; set; }
        public double AverageVolatility { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public int Duration => EndIndex - StartIndex + 1;
    }

    /// <summary>
    /// Prediction result from the ensemble model
    /// </summary>
    public class PredictionResult
    {
        public string Method { get; set; } = string.Empty;
        public double PredictedDirection { get; set; } // -1 to 1, where 1 is strongly up
        public double Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    /// <summary>
    /// Market behavior analyzer with trend analysis, volatility clustering, and predictions
    /// </summary>
    public class MarketBehaviorAnalyzer
    {
        private readonly int _trendWindow;
        private readonly int _volatilityWindow;
        
        public MarketBehaviorAnalyzer(int trendWindow = 50, int volatilityWindow = 20)
        {
            _trendWindow = trendWindow;
            _volatilityWindow = volatilityWindow;
        }

        /// <summary>
        /// Performs comprehensive market behavior analysis
        /// </summary>
        public MarketBehaviorAnalysis AnalyzeMarketBehavior(List<MarketDataPoint> data)
        {
            Console.WriteLine("📊 Analyzing market behavior patterns...");
            
            var analysis = new MarketBehaviorAnalysis();
            
            // 1. Trend Analysis
            analysis.TrendPeriods = DetectTrendPeriods(data);
            (analysis.OverallTrendStrength, analysis.TrendDirection) = CalculateOverallTrend(data);
            
            // 2. Volatility Clustering Analysis
            analysis.VolatilityClusters = DetectVolatilityClusters(data);
            analysis.VolatilityClustering = CalculateVolatilityClusteringIndex(data);
            
            // 3. Market Dynamics
            analysis.MeanReversion = CalculateMeanReversionTendency(data);
            analysis.Momentum = CalculateMomentum(data);
            
            Console.WriteLine($"✅ Detected {analysis.TrendPeriods.Count} trend periods and {analysis.VolatilityClusters.Count} volatility clusters");
            
            return analysis;
        }

        /// <summary>
        /// Detects distinct trend periods in the market data
        /// </summary>
        private List<TrendPeriod> DetectTrendPeriods(List<MarketDataPoint> data)
        {
            var trends = new List<TrendPeriod>();
            if (data.Count < _trendWindow) return trends;

            var movingAverages = CalculateMovingAverages(data, _trendWindow);
            var currentTrend = new TrendPeriod();
            var isInTrend = false;

            for (int i = _trendWindow; i < data.Count - 1; i++)
            {
                var slope = (movingAverages[i] - movingAverages[i - 5]) / 5; // 5-period slope
                var direction = slope > 0.001 ? "Upward" : slope < -0.001 ? "Downward" : "Sideways";
                var strength = Math.Abs(slope) * 1000; // Scale for readability

                if (!isInTrend && strength > 0.5) // Start new trend
                {
                    currentTrend = new TrendPeriod
                    {
                        StartIndex = i,
                        Direction = direction,
                        Strength = strength,
                        StartTime = data[i].Timestamp
                    };
                    isInTrend = true;
                }
                else if (isInTrend)
                {
                    // Check if trend continues
                    if (direction == currentTrend.Direction && strength > 0.3)
                    {
                        currentTrend.Strength = (currentTrend.Strength + strength) / 2; // Running average
                    }
                    else // Trend ended
                    {
                        currentTrend.EndIndex = i - 1;
                        currentTrend.EndTime = data[i - 1].Timestamp;
                        currentTrend.Persistence = CalculateTrendPersistence(data, currentTrend);
                        
                        if (currentTrend.Duration > 10) // Only keep significant trends
                        {
                            trends.Add(currentTrend);
                        }
                        isInTrend = false;
                    }
                }
            }

            // Close final trend if still active
            if (isInTrend)
            {
                currentTrend.EndIndex = data.Count - 1;
                currentTrend.EndTime = data[^1].Timestamp;
                currentTrend.Persistence = CalculateTrendPersistence(data, currentTrend);
                if (currentTrend.Duration > 10)
                {
                    trends.Add(currentTrend);
                }
            }

            return trends;
        }

        /// <summary>
        /// Detects volatility clustering in market data
        /// </summary>
        private List<VolatilityCluster> DetectVolatilityClusters(List<MarketDataPoint> data)
        {
            var clusters = new List<VolatilityCluster>();
            if (data.Count < _volatilityWindow) return clusters;

            // Calculate rolling volatility if not already done
            var avgVolatility = data.Skip(_volatilityWindow).Select(d => d.Volatility).Average();
            var threshold = avgVolatility * 1.5; // 1.5x average volatility

            var currentCluster = new VolatilityCluster();
            var isInCluster = false;

            for (int i = _volatilityWindow; i < data.Count; i++)
            {
                var currentVol = data[i].Volatility;

                if (!isInCluster && currentVol > threshold)
                {
                    currentCluster = new VolatilityCluster
                    {
                        StartIndex = i,
                        StartTime = data[i].Timestamp,
                        IntensityMultiplier = currentVol / avgVolatility
                    };
                    isInCluster = true;
                }
                else if (isInCluster)
                {
                    if (currentVol > threshold * 0.8) // Continue cluster
                    {
                        currentCluster.IntensityMultiplier = Math.Max(currentCluster.IntensityMultiplier, currentVol / avgVolatility);
                    }
                    else // End cluster
                    {
                        currentCluster.EndIndex = i - 1;
                        currentCluster.EndTime = data[i - 1].Timestamp;
                        currentCluster.AverageVolatility = CalculateAverageVolatility(data, currentCluster);
                        
                        if (currentCluster.Duration > 3) // Only keep significant clusters
                        {
                            clusters.Add(currentCluster);
                        }
                        isInCluster = false;
                    }
                }
            }

            return clusters;
        }

        /// <summary>
        /// Calculates moving averages for trend detection
        /// </summary>
        private double[] CalculateMovingAverages(List<MarketDataPoint> data, int window)
        {
            var movingAvg = new double[data.Count];
            
            for (int i = 0; i < data.Count; i++)
            {
                if (i < window)
                {
                    movingAvg[i] = data[i].Price;
                    continue;
                }

                double sum = 0;
                for (int j = i - window; j < i; j++)
                {
                    sum += data[j].Price;
                }
                movingAvg[i] = sum / window;
            }

            return movingAvg;
        }

        /// <summary>
        /// Calculates overall trend direction and strength
        /// </summary>
        private (double strength, string direction) CalculateOverallTrend(List<MarketDataPoint> data)
        {
            if (data.Count < 20) return (0, "Insufficient Data");

            var startPrice = data.Take(10).Select(d => d.Price).Average();
            var endPrice = data.TakeLast(10).Select(d => d.Price).Average();
            
            var totalReturn = (endPrice - startPrice) / startPrice;
            var strength = Math.Abs(totalReturn) * 100;
            
            var direction = totalReturn > 0.02 ? "Bullish" : 
                           totalReturn < -0.02 ? "Bearish" : "Sideways";
            
            return (strength, direction);
        }

        /// <summary>
        /// Calculates trend persistence (how consistently the trend moves in one direction)
        /// </summary>
        private double CalculateTrendPersistence(List<MarketDataPoint> data, TrendPeriod trend)
        {
            if (trend.StartIndex >= trend.EndIndex) return 0;

            var trendData = data.Skip(trend.StartIndex).Take(trend.Duration).ToList();
            var consistentMoves = 0;
            var totalMoves = 0;

            for (int i = 1; i < trendData.Count; i++)
            {
                var currentMove = trendData[i].Price - trendData[i - 1].Price;
                var expectedDirection = trend.Direction == "Upward" ? 1 : -1;
                
                if (Math.Sign(currentMove) == expectedDirection || Math.Abs(currentMove) < 0.001)
                {
                    consistentMoves++;
                }
                totalMoves++;
            }

            return totalMoves > 0 ? (double)consistentMoves / totalMoves : 0;
        }

        /// <summary>
        /// Calculates average volatility within a cluster
        /// </summary>
        private double CalculateAverageVolatility(List<MarketDataPoint> data, VolatilityCluster cluster)
        {
            return data.Skip(cluster.StartIndex)
                      .Take(cluster.Duration)
                      .Select(d => d.Volatility)
                      .Average();
        }

        /// <summary>
        /// Calculates volatility clustering index
        /// </summary>
        private double CalculateVolatilityClusteringIndex(List<MarketDataPoint> data)
        {
            if (data.Count < 40) return 0;

            // Compare volatility autocorrelation
            var volatilities = data.Select(d => d.Volatility).ToArray();
            var lag1Correlation = CalculateAutocorrelation(volatilities, 1);
            var lag5Correlation = CalculateAutocorrelation(volatilities, 5);
            
            return (lag1Correlation + lag5Correlation) / 2;
        }

        /// <summary>
        /// Calculates mean reversion tendency
        /// </summary>
        private double CalculateMeanReversionTendency(List<MarketDataPoint> data)
        {
            if (data.Count < 50) return 0;

            var prices = data.Select(d => d.Price).ToArray();
            var movingAvg = CalculateMovingAverages(data, 20);
            
            var reversionCount = 0;
            var totalDeviations = 0;

            for (int i = 21; i < data.Count - 1; i++)
            {
                var deviation = prices[i] - movingAvg[i];
                var nextDeviation = prices[i + 1] - movingAvg[i + 1];
                
                // Check if price moved back toward mean
                if (Math.Sign(deviation) != Math.Sign(nextDeviation) && Math.Abs(deviation) > 0.001)
                {
                    reversionCount++;
                }
                totalDeviations++;
            }

            return totalDeviations > 0 ? (double)reversionCount / totalDeviations : 0;
        }

        /// <summary>
        /// Calculates momentum indicator
        /// </summary>
        private double CalculateMomentum(List<MarketDataPoint> data)
        {
            if (data.Count < 30) return 0;

            var recentReturns = data.TakeLast(10).Select(d => d.Returns).ToArray();
            var momentum = recentReturns.Sum();
            
            return Math.Max(-1, Math.Min(1, momentum * 10)); // Normalize to [-1, 1]
        }

        /// <summary>
        /// Calculates autocorrelation for volatility clustering analysis
        /// </summary>
        private double CalculateAutocorrelation(double[] series, int lag)
        {
            if (series.Length < lag + 10) return 0;

            var n = series.Length - lag;
            var mean = series.Average();
            
            var numerator = 0.0;
            var denominator = 0.0;

            for (int i = 0; i < n; i++)
            {
                numerator += (series[i] - mean) * (series[i + lag] - mean);
            }

            for (int i = 0; i < series.Length; i++)
            {
                denominator += (series[i] - mean) * (series[i] - mean);
            }

            return denominator > 0 ? numerator / denominator : 0;
        }

        /// <summary>
        /// Generates ensemble predictions using multiple methods
        /// </summary>
        public List<PredictionResult> GenerateEnsemblePredictions(List<MarketDataPoint> data, List<FractalPattern> patterns)
        {
            Console.WriteLine("🎯 Generating ensemble predictions...");
            
            var predictions = new List<PredictionResult>();
            
            // Method 1: Trend-based prediction
            predictions.Add(GenerateTrendBasedPrediction(data));
            
            // Method 2: Mean reversion prediction  
            predictions.Add(GenerateMeanReversionPrediction(data));
            
            // Method 3: Fractal pattern prediction
            predictions.Add(GenerateFractalPatternPrediction(data, patterns));
            
            // Method 4: Volatility-based prediction
            predictions.Add(GenerateVolatilityPrediction(data));
            
            // Method 5: Simple momentum prediction
            predictions.Add(GenerateMomentumPrediction(data));
            
            return predictions;
        }

        private PredictionResult GenerateTrendBasedPrediction(List<MarketDataPoint> data)
        {
            var recentSlope = (data[^1].Price - data[^10].Price) / 10;
            var direction = Math.Sign(recentSlope);
            var confidence = Math.Min(0.9, Math.Abs(recentSlope) * 100);
            
            return new PredictionResult
            {
                Method = "Trend Following",
                PredictedDirection = direction,
                Confidence = confidence,
                Reasoning = $"Recent 10-period slope: {recentSlope:F4}"
            };
        }

        private PredictionResult GenerateMeanReversionPrediction(List<MarketDataPoint> data)
        {
            var movingAvg = data.TakeLast(20).Select(d => d.Price).Average();
            var currentPrice = data[^1].Price;
            var deviation = (currentPrice - movingAvg) / movingAvg;
            
            var direction = -Math.Sign(deviation); // Opposite to current deviation
            var confidence = Math.Min(0.8, Math.Abs(deviation) * 5);
            
            return new PredictionResult
            {
                Method = "Mean Reversion",
                PredictedDirection = direction,
                Confidence = confidence,
                Reasoning = $"Price deviation from MA: {deviation:P2}"
            };
        }

        private PredictionResult GenerateFractalPatternPrediction(List<MarketDataPoint> data, List<FractalPattern> patterns)
        {
            if (patterns.Count == 0)
            {
                return new PredictionResult
                {
                    Method = "Fractal Pattern",
                    PredictedDirection = 0,
                    Confidence = 0,
                    Reasoning = "No patterns detected"
                };
            }

            var recentPattern = patterns.OrderByDescending(p => p.EndIndex).First();
            var direction = recentPattern.PatternType.Contains("Upward") ? 1 : 
                           recentPattern.PatternType.Contains("Downward") ? -1 : 0;
            
            return new PredictionResult
            {
                Method = "Fractal Pattern",
                PredictedDirection = direction,
                Confidence = recentPattern.Confidence,
                Reasoning = $"Recent pattern: {recentPattern.PatternType}"
            };
        }

        private PredictionResult GenerateVolatilityPrediction(List<MarketDataPoint> data)
        {
            var recentVol = data.TakeLast(5).Select(d => d.Volatility).Average();
            var avgVol = data.Select(d => d.Volatility).Average();
            
            // High volatility often precedes reversals
            var volRatio = recentVol / avgVol;
            var direction = volRatio > 1.5 ? -Math.Sign(data[^1].Returns) : 0;
            var confidence = Math.Min(0.7, Math.Abs(volRatio - 1));
            
            return new PredictionResult
            {
                Method = "Volatility Analysis",
                PredictedDirection = direction,
                Confidence = confidence,
                Reasoning = $"Vol ratio: {volRatio:F2}"
            };
        }

        private PredictionResult GenerateMomentumPrediction(List<MarketDataPoint> data)
        {
            var momentum = data.TakeLast(5).Select(d => d.Returns).Sum();
            var direction = Math.Sign(momentum);
            var confidence = Math.Min(0.8, Math.Abs(momentum) * 10);
            
            return new PredictionResult
            {
                Method = "Momentum",
                PredictedDirection = direction,
                Confidence = confidence,
                Reasoning = $"5-period momentum: {momentum:F4}"
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FractalMarketDemo
{
    /// <summary>
    /// Handles data export and console visualization for the fractal market analysis
    /// </summary>
    public class DataExportAndVisualization
    {
        private readonly string _outputDirectory;
        
        public DataExportAndVisualization(string outputDirectory = "FractalMarketResults")
        {
            _outputDirectory = outputDirectory;
            Directory.CreateDirectory(_outputDirectory);
        }

        /// <summary>
        /// Displays a comprehensive summary of all analysis results
        /// </summary>
        public void DisplayComprehensiveSummary(
            List<MarketDataPoint> marketData, 
            List<FractalPattern> patterns, 
            MarketBehaviorAnalysis behavior,
            List<PredictionResult> predictions)
        {
            Console.Clear();
            DisplayHeader();
            DisplayMarketSummary(marketData);
            DisplayFractalPatterns(patterns);
            DisplayMarketBehavior(behavior);
            DisplayPredictions(predictions);
            DisplayFooter();
        }

        /// <summary>
        /// Displays the application header
        /// </summary>
        private void DisplayHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    🔢 FRACTAL MARKET DEMO 🔢                   ║");
            Console.WriteLine("║              Fractal Mathematics in Trading Systems           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Displays market data summary
        /// </summary>
        private void DisplayMarketSummary(List<MarketDataPoint> data)
        {
            if (data.Count == 0) return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("📈 MARKET DATA SUMMARY");
            Console.WriteLine("─────────────────────────");
            Console.ResetColor();

            var startPrice = data.First().Price;
            var endPrice = data.Last().Price;
            var totalReturn = (endPrice - startPrice) / startPrice;
            var avgVolatility = data.Select(d => d.Volatility).Average();
            var maxPrice = data.Max(d => d.Price);
            var minPrice = data.Min(d => d.Price);

            Console.WriteLine($"📊 Data Points:        {data.Count:N0}");
            Console.WriteLine($"📅 Time Period:        {data.First().Timestamp:yyyy-MM-dd HH:mm} to {data.Last().Timestamp:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"💰 Starting Price:     ${startPrice:F2}");
            Console.WriteLine($"💰 Ending Price:       ${endPrice:F2}");
            Console.WriteLine($"📈 Total Return:       {totalReturn:P2} ({(totalReturn > 0 ? "▲" : "▼")})");
            Console.WriteLine($"🎯 Price Range:        ${minPrice:F2} - ${maxPrice:F2}");
            Console.WriteLine($"📊 Average Volatility: {avgVolatility:P2}");
            
            // Simple price chart
            DisplaySimplePriceChart(data.TakeLast(50).ToList());
            Console.WriteLine();
        }

        /// <summary>
        /// Displays a simple ASCII price chart
        /// </summary>
        private void DisplaySimplePriceChart(List<MarketDataPoint> data)
        {
            if (data.Count < 10) return;

            Console.WriteLine("\n📉 Recent Price Movement (Last 50 Points):");
            var prices = data.Select(d => d.Price).ToArray();
            var min = prices.Min();
            var max = prices.Max();
            var range = max - min;
            
            if (range == 0)
            {
                Console.WriteLine("   Price remained constant");
                return;
            }

            var height = 8;
            for (int row = height - 1; row >= 0; row--)
            {
                var threshold = min + (range * row / (height - 1));
                Console.Write($"{threshold,6:F1} ");
                
                for (int i = 0; i < Math.Min(prices.Length, 50); i++)
                {
                    if (prices[i] >= threshold)
                        Console.Write("█");
                    else
                        Console.Write(" ");
                }
                Console.WriteLine();
            }
            
            Console.Write("       ");
            for (int i = 0; i < Math.Min(prices.Length, 50); i += 10)
            {
                Console.Write($"{i,10}");
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// Displays detected fractal patterns
        /// </summary>
        private void DisplayFractalPatterns(List<FractalPattern> patterns)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("🔍 FRACTAL PATTERN ANALYSIS");
            Console.WriteLine("──────────────────────────────");
            Console.ResetColor();

            if (patterns.Count == 0)
            {
                Console.WriteLine("❌ No significant fractal patterns detected");
                Console.WriteLine();
                return;
            }

            Console.WriteLine($"✅ Found {patterns.Count} fractal patterns:\n");

            for (int i = 0; i < Math.Min(patterns.Count, 5); i++)
            {
                var pattern = patterns[i];
                var confidenceColor = pattern.Confidence > 0.8 ? ConsoleColor.Green :
                                    pattern.Confidence > 0.6 ? ConsoleColor.Yellow :
                                    ConsoleColor.Red;

                Console.Write($"  {i + 1}. ");
                Console.ForegroundColor = confidenceColor;
                Console.Write($"{pattern.PatternType}");
                Console.ResetColor();
                Console.WriteLine($" (Confidence: {pattern.Confidence:P0})");

                Console.WriteLine($"     🕐 Duration: {pattern.Length} periods ({pattern.StartTime:HH:mm} - {pattern.EndTime:HH:mm})");
                Console.WriteLine($"     📐 Fractal Dimension: {pattern.FractalDimension:F3}");
                Console.WriteLine($"     🔄 Self-Similarity: {pattern.SelfSimilarityIndex:F3}");
                
                // Mini pattern visualization
                DisplayPatternMiniChart(pattern.NormalizedPrices);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Displays a mini chart for a fractal pattern
        /// </summary>
        private void DisplayPatternMiniChart(double[] normalizedPrices)
        {
            if (normalizedPrices.Length < 5) return;

            Console.Write("     📈 Shape: ");
            var height = 3;
            
            for (int i = 0; i < Math.Min(normalizedPrices.Length, 30); i += Math.Max(1, normalizedPrices.Length / 30))
            {
                var level = (int)(normalizedPrices[i] * height);
                var symbol = level switch
                {
                    0 => "_",
                    1 => "-",
                    2 => "=",
                    _ => "▄"
                };
                Console.Write(symbol);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Displays market behavior analysis results
        /// </summary>
        private void DisplayMarketBehavior(MarketBehaviorAnalysis behavior)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📊 MARKET BEHAVIOR ANALYSIS");
            Console.WriteLine("───────────────────────────────");
            Console.ResetColor();

            // Overall trend
            var trendColor = behavior.TrendDirection switch
            {
                "Bullish" => ConsoleColor.Green,
                "Bearish" => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            Console.Write("🎯 Overall Trend: ");
            Console.ForegroundColor = trendColor;
            Console.Write($"{behavior.TrendDirection}");
            Console.ResetColor();
            Console.WriteLine($" (Strength: {behavior.OverallTrendStrength:F1}%)");

            Console.WriteLine($"📈 Momentum:           {GetMomentumIndicator(behavior.Momentum)} ({behavior.Momentum:F3})");
            Console.WriteLine($"🔄 Mean Reversion:     {GetIndicatorBar(behavior.MeanReversion)} ({behavior.MeanReversion:P0})");
            Console.WriteLine($"📊 Vol Clustering:     {GetIndicatorBar(behavior.VolatilityClustering)} ({behavior.VolatilityClustering:F3})");

            // Trend periods
            if (behavior.TrendPeriods.Count > 0)
            {
                Console.WriteLine($"\n🔍 Detected {behavior.TrendPeriods.Count} trend periods:");
                foreach (var trend in behavior.TrendPeriods.Take(3))
                {
                    var arrow = trend.Direction switch
                    {
                        "Upward" => "📈",
                        "Downward" => "📉",
                        _ => "➡️"
                    };
                    Console.WriteLine($"   {arrow} {trend.Direction} trend: {trend.Duration} periods (Persistence: {trend.Persistence:P0})");
                }
            }

            // Volatility clusters
            if (behavior.VolatilityClusters.Count > 0)
            {
                Console.WriteLine($"\n⚡ Found {behavior.VolatilityClusters.Count} volatility clusters:");
                foreach (var cluster in behavior.VolatilityClusters.Take(3))
                {
                    Console.WriteLine($"   🔥 {cluster.Duration} periods, {cluster.IntensityMultiplier:F1}x intensity");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Displays prediction results
        /// </summary>
        private void DisplayPredictions(List<PredictionResult> predictions)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("🎯 ENSEMBLE PREDICTIONS");
            Console.WriteLine("─────────────────────────");
            Console.ResetColor();

            if (predictions.Count == 0)
            {
                Console.WriteLine("❌ No predictions generated");
                Console.WriteLine();
                return;
            }

            // Calculate consensus
            var weightedDirection = predictions.Sum(p => p.PredictedDirection * p.Confidence);
            var totalWeight = predictions.Sum(p => p.Confidence);
            var consensus = totalWeight > 0 ? weightedDirection / totalWeight : 0;
            
            // Display consensus
            var consensusColor = Math.Abs(consensus) > 0.3 ? 
                               (consensus > 0 ? ConsoleColor.Green : ConsoleColor.Red) : 
                               ConsoleColor.Gray;
            
            Console.Write("🎲 Ensemble Consensus: ");
            Console.ForegroundColor = consensusColor;
            var direction = consensus > 0.1 ? "BULLISH ▲" : consensus < -0.1 ? "BEARISH ▼" : "NEUTRAL ➡️";
            Console.Write($"{direction}");
            Console.ResetColor();
            Console.WriteLine($" (Score: {consensus:F2})");
            Console.WriteLine();

            // Individual predictions
            Console.WriteLine("📋 Individual Method Results:");
            foreach (var prediction in predictions.OrderByDescending(p => p.Confidence))
            {
                var predictionColor = prediction.PredictedDirection > 0.1 ? ConsoleColor.Green :
                                    prediction.PredictedDirection < -0.1 ? ConsoleColor.Red :
                                    ConsoleColor.Gray;

                var arrow = prediction.PredictedDirection > 0.1 ? "▲" :
                          prediction.PredictedDirection < -0.1 ? "▼" : "➡️";

                Console.Write($"  • {prediction.Method,-18}: ");
                Console.ForegroundColor = predictionColor;
                Console.Write($"{arrow}");
                Console.ResetColor();
                Console.WriteLine($" (Conf: {prediction.Confidence:P0}) - {prediction.Reasoning}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Gets a momentum indicator string
        /// </summary>
        private string GetMomentumIndicator(double momentum)
        {
            return momentum switch
            {
                > 0.3 => "🚀 Strong Up",
                > 0.1 => "📈 Moderate Up", 
                > -0.1 => "➡️ Neutral",
                > -0.3 => "📉 Moderate Down",
                _ => "⬇️ Strong Down"
            };
        }

        /// <summary>
        /// Gets a visual indicator bar for metrics
        /// </summary>
        private string GetIndicatorBar(double value, int length = 10)
        {
            var filled = (int)(Math.Abs(value) * length);
            var bar = new string('█', Math.Min(filled, length)) + new string('░', Math.Max(0, length - filled));
            return $"[{bar}]";
        }

        /// <summary>
        /// Displays footer information
        /// </summary>
        private void DisplayFooter()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine("💾 Analysis complete. Data exported to CSV files.");
            Console.WriteLine($"📁 Output directory: .\\{_outputDirectory}\\");
            Console.WriteLine("🔬 Fractal Mathematics in Trading Systems - Demo Version");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Exports all analysis results to CSV files
        /// </summary>
        public void ExportAllResults(
            List<MarketDataPoint> marketData,
            List<FractalPattern> patterns,
            MarketBehaviorAnalysis behavior,
            List<PredictionResult> predictions)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            Console.WriteLine("💾 Exporting analysis results to CSV...");

            // Export market data
            ExportMarketData(marketData, $"market_data_{timestamp}.csv");
            
            // Export fractal patterns
            ExportFractalPatterns(patterns, $"fractal_patterns_{timestamp}.csv");
            
            // Export market behavior analysis
            ExportMarketBehavior(behavior, $"market_behavior_{timestamp}.csv");
            
            // Export predictions
            ExportPredictions(predictions, $"predictions_{timestamp}.csv");
            
            // Export session summary
            ExportSessionSummary(marketData, patterns, behavior, predictions, $"session_summary_{timestamp}.csv");

            Console.WriteLine($"✅ All results exported to .\\{_outputDirectory}\\ directory");
        }

        /// <summary>
        /// Exports market data to CSV
        /// </summary>
        private void ExportMarketData(List<MarketDataPoint> data, string filename)
        {
            var filePath = Path.Combine(_outputDirectory, filename);
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            
            writer.WriteLine("Timestamp,Price,Volume,Returns,Volatility");
            
            foreach (var point in data)
            {
                writer.WriteLine($"{point.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                               $"{point.Price:F4}," +
                               $"{point.Volume:F2}," +
                               $"{point.Returns:F6}," +
                               $"{point.Volatility:F6}");
            }
        }

        /// <summary>
        /// Exports fractal patterns to CSV
        /// </summary>
        private void ExportFractalPatterns(List<FractalPattern> patterns, string filename)
        {
            var filePath = Path.Combine(_outputDirectory, filename);
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            
            writer.WriteLine("StartIndex,EndIndex,Duration,StartTime,EndTime,PatternType,FractalDimension,SelfSimilarityIndex,Confidence");
            
            foreach (var pattern in patterns)
            {
                writer.WriteLine($"{pattern.StartIndex}," +
                               $"{pattern.EndIndex}," +
                               $"{pattern.Length}," +
                               $"{pattern.StartTime:yyyy-MM-dd HH:mm:ss}," +
                               $"{pattern.EndTime:yyyy-MM-dd HH:mm:ss}," +
                               $"{pattern.PatternType}," +
                               $"{pattern.FractalDimension:F4}," +
                               $"{pattern.SelfSimilarityIndex:F4}," +
                               $"{pattern.Confidence:F4}");
            }
        }

        /// <summary>
        /// Exports market behavior analysis to CSV
        /// </summary>
        private void ExportMarketBehavior(MarketBehaviorAnalysis behavior, string filename)
        {
            var filePath = Path.Combine(_outputDirectory, filename);
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            
            // Overall metrics
            writer.WriteLine("Metric,Value");
            writer.WriteLine($"OverallTrendStrength,{behavior.OverallTrendStrength:F4}");
            writer.WriteLine($"TrendDirection,{behavior.TrendDirection}");
            writer.WriteLine($"VolatilityClustering,{behavior.VolatilityClustering:F4}");
            writer.WriteLine($"MeanReversion,{behavior.MeanReversion:F4}");
            writer.WriteLine($"Momentum,{behavior.Momentum:F4}");
            writer.WriteLine($"TrendPeriodCount,{behavior.TrendPeriods.Count}");
            writer.WriteLine($"VolatilityClusterCount,{behavior.VolatilityClusters.Count}");
            
            writer.WriteLine();
            writer.WriteLine("TrendPeriods");
            writer.WriteLine("StartIndex,EndIndex,Duration,Direction,Strength,Persistence,StartTime,EndTime");
            
            foreach (var trend in behavior.TrendPeriods)
            {
                writer.WriteLine($"{trend.StartIndex}," +
                               $"{trend.EndIndex}," +
                               $"{trend.Duration}," +
                               $"{trend.Direction}," +
                               $"{trend.Strength:F4}," +
                               $"{trend.Persistence:F4}," +
                               $"{trend.StartTime:yyyy-MM-dd HH:mm:ss}," +
                               $"{trend.EndTime:yyyy-MM-dd HH:mm:ss}");
            }
        }

        /// <summary>
        /// Exports predictions to CSV
        /// </summary>
        private void ExportPredictions(List<PredictionResult> predictions, string filename)
        {
            var filePath = Path.Combine(_outputDirectory, filename);
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            
            writer.WriteLine("Method,PredictedDirection,Confidence,Reasoning");
            
            foreach (var prediction in predictions)
            {
                writer.WriteLine($"{prediction.Method}," +
                               $"{prediction.PredictedDirection:F4}," +
                               $"{prediction.Confidence:F4}," +
                               $"\"{prediction.Reasoning}\"");
            }
        }

        /// <summary>
        /// Exports session summary
        /// </summary>
        private void ExportSessionSummary(
            List<MarketDataPoint> marketData,
            List<FractalPattern> patterns,
            MarketBehaviorAnalysis behavior,
            List<PredictionResult> predictions,
            string filename)
        {
            var filePath = Path.Combine(_outputDirectory, filename);
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            
            var startPrice = marketData.First().Price;
            var endPrice = marketData.Last().Price;
            var totalReturn = (endPrice - startPrice) / startPrice;
            
            writer.WriteLine("FRACTAL MARKET ANALYSIS SESSION SUMMARY");
            writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine();
            writer.WriteLine("MARKET DATA SUMMARY");
            writer.WriteLine($"DataPoints,{marketData.Count}");
            writer.WriteLine($"StartPrice,{startPrice:F4}");
            writer.WriteLine($"EndPrice,{endPrice:F4}");
            writer.WriteLine($"TotalReturn,{totalReturn:F6}");
            writer.WriteLine($"StartTime,{marketData.First().Timestamp:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine($"EndTime,{marketData.Last().Timestamp:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine();
            writer.WriteLine("ANALYSIS RESULTS");
            writer.WriteLine($"FractalPatternsFound,{patterns.Count}");
            writer.WriteLine($"TrendPeriodsDetected,{behavior.TrendPeriods.Count}");
            writer.WriteLine($"VolatilityClustersFound,{behavior.VolatilityClusters.Count}");
            writer.WriteLine($"PredictionMethodsUsed,{predictions.Count}");
            writer.WriteLine();
            writer.WriteLine("ENSEMBLE PREDICTION CONSENSUS");
            var consensus = predictions.Sum(p => p.PredictedDirection * p.Confidence) / Math.Max(predictions.Sum(p => p.Confidence), 1);
            writer.WriteLine($"WeightedConsensus,{consensus:F4}");
            writer.WriteLine($"ConsensusDirection,{(consensus > 0.1 ? "BULLISH" : consensus < -0.1 ? "BEARISH" : "NEUTRAL")}");
        }
    }
}
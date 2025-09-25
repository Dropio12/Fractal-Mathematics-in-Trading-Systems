using System;
using System.Collections.Generic;
using System.Threading;

namespace FractalMarketDemo
{
    /// <summary>
    /// Main program demonstrating fractal pattern recognition in simulated market data
    /// This application showcases the core concepts from the FMTS (Fractal Mathematics in Trading Systems)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var demo = new FractalMarketDemoApplication();
                demo.RunCompleteDemo();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error occurred: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// Main application class that orchestrates the fractal market demonstration
    /// </summary>
    public class FractalMarketDemoApplication
    {
        private readonly MarketDataGenerator _dataGenerator;
        private readonly MarketEventSimulator _eventSimulator;
        private readonly FractalPatternRecognizer _patternRecognizer;
        private readonly MarketBehaviorAnalyzer _behaviorAnalyzer;
        private readonly DataExportAndVisualization _visualizer;

        public FractalMarketDemoApplication()
        {
            // Initialize all components with reasonable defaults
            _dataGenerator = new MarketDataGenerator(
                drift: 0.0002,           // Small positive drift (bullish bias)
                volatility: 0.025,       // 2.5% volatility 
                fractalDimension: 1.4    // Fractal dimension between 1 and 2
            );
            
            _eventSimulator = new MarketEventSimulator();
            _patternRecognizer = new FractalPatternRecognizer();
            _behaviorAnalyzer = new MarketBehaviorAnalyzer();
            _visualizer = new DataExportAndVisualization();
        }

        /// <summary>
        /// Runs the complete fractal market analysis demonstration
        /// </summary>
        public void RunCompleteDemo()
        {
            ShowWelcomeScreen();
            
            // Step 1: Generate Market Data
            var marketData = GenerateMarketData();
            
            // Step 2: Add Market Events
            AddMarketEvents(marketData);
            
            // Step 3: Detect Fractal Patterns
            var fractalPatterns = DetectFractalPatterns(marketData);
            
            // Step 4: Analyze Market Behavior
            var behaviorAnalysis = AnalyzeMarketBehavior(marketData);
            
            // Step 5: Generate Predictions
            var predictions = GeneratePredictions(marketData, fractalPatterns);
            
            // Step 6: Display Results and Export Data
            DisplayResults(marketData, fractalPatterns, behaviorAnalysis, predictions);
            
            // Step 7: Export to CSV
            ExportResults(marketData, fractalPatterns, behaviorAnalysis, predictions);
            
            ShowCompletionScreen();
        }

        /// <summary>
        /// Shows the welcome screen with demo information
        /// </summary>
        private void ShowWelcomeScreen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  🔢 FRACTAL MARKET DEMO 🔢                      ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("║         Fractal Mathematics in Trading Systems                ║");
            Console.WriteLine("║                     Demo Application                          ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("║  This demonstration will showcase:                            ║");
            Console.WriteLine("║  • Market data simulation with fractal noise                  ║");
            Console.WriteLine("║  • Fractal pattern recognition using box-counting             ║");
            Console.WriteLine("║  • Market behavior analysis and trend detection               ║");
            Console.WriteLine("║  • Ensemble prediction methods                                ║");
            Console.WriteLine("║  • Data visualization and CSV export                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("Press any key to start the demonstration...");
            Console.ReadKey();
        }

        /// <summary>
        /// Generates realistic market data using fractal-enhanced Brownian motion
        /// </summary>
        private List<MarketDataPoint> GenerateMarketData()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🎲 STEP 1: GENERATING MARKET DATA");
            Console.WriteLine("═══════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("📊 Generating realistic market data using:");
            Console.WriteLine("   • Geometric Brownian motion");
            Console.WriteLine("   • Fractal noise components");
            Console.WriteLine("   • Golden ratio scaling");
            Console.WriteLine("   • Volume correlation");
            
            // Generate data with progress indicator
            const int dataPoints = 500; // 500 hourly data points (~3 weeks)
            const double initialPrice = 100.0;
            
            Console.Write("\n🔄 Progress: ");
            var data = _dataGenerator.GenerateMarketData(dataPoints, initialPrice);
            
            // Simple progress animation
            for (int i = 0; i <= 20; i++)
            {
                Thread.Sleep(50);
                Console.Write("█");
            }
            
            Console.WriteLine($"\n\n✅ Generated {data.Count} market data points");
            Console.WriteLine($"   Starting Price: ${data[0].Price:F2}");
            Console.WriteLine($"   Ending Price: ${data[^1].Price:F2}");
            Console.WriteLine($"   Total Return: {((data[^1].Price - data[0].Price) / data[0].Price):P2}");
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            
            return data;
        }

        /// <summary>
        /// Adds realistic market events like volatility clustering
        /// </summary>
        private void AddMarketEvents(List<MarketDataPoint> data)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚡ STEP 2: ADDING MARKET EVENTS");
            Console.WriteLine("═══════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("🔥 Adding realistic market events:");
            Console.WriteLine("   • Volatility clustering");
            Console.WriteLine("   • Market shock simulation");
            Console.WriteLine("   • Volume spike generation");
            
            Console.Write("\n🔄 Processing market events... ");
            _eventSimulator.AddVolatilityClustering(data);
            
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(100);
                Console.Write("▓");
            }
            
            Console.WriteLine("\n\n✅ Market events added successfully");
            Console.WriteLine("   Events now embedded in price and volume data");
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Detects fractal patterns using box-counting method
        /// </summary>
        private List<FractalPattern> DetectFractalPatterns(List<MarketDataPoint> data)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("🔍 STEP 3: FRACTAL PATTERN RECOGNITION");
            Console.WriteLine("═══════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("📐 Analyzing fractal patterns using:");
            Console.WriteLine("   • Box-counting method");
            Console.WriteLine("   • Self-similarity detection");
            Console.WriteLine("   • Dynamic Time Warping");
            Console.WriteLine("   • Pattern classification");
            
            Console.WriteLine();
            var patterns = _patternRecognizer.DetectPatterns(data);
            
            if (patterns.Count > 0)
            {
                Console.WriteLine($"\n✅ Pattern recognition complete!");
                Console.WriteLine($"   Found {patterns.Count} significant patterns");
                Console.WriteLine($"   Average fractal dimension: {patterns.Average(p => p.FractalDimension):F3}");
                Console.WriteLine($"   Highest confidence: {patterns.Max(p => p.Confidence):P0}");
            }
            else
            {
                Console.WriteLine("\n⚠️ No significant patterns detected");
                Console.WriteLine("   This is normal for highly random data");
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            
            return patterns;
        }

        /// <summary>
        /// Analyzes market behavior including trends and volatility clustering
        /// </summary>
        private MarketBehaviorAnalysis AnalyzeMarketBehavior(List<MarketDataPoint> data)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("📊 STEP 4: MARKET BEHAVIOR ANALYSIS");
            Console.WriteLine("═══════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("🎯 Analyzing market behavior:");
            Console.WriteLine("   • Trend detection and persistence");
            Console.WriteLine("   • Volatility clustering analysis");
            Console.WriteLine("   • Mean reversion calculation");
            Console.WriteLine("   • Momentum indicators");
            
            Console.WriteLine();
            var analysis = _behaviorAnalyzer.AnalyzeMarketBehavior(data);
            
            Console.WriteLine($"\n✅ Market analysis complete!");
            Console.WriteLine($"   Overall trend: {analysis.TrendDirection} ({analysis.OverallTrendStrength:F1}% strength)");
            Console.WriteLine($"   Trend periods detected: {analysis.TrendPeriods.Count}");
            Console.WriteLine($"   Volatility clusters: {analysis.VolatilityClusters.Count}");
            Console.WriteLine($"   Mean reversion tendency: {analysis.MeanReversion:P1}");
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            
            return analysis;
        }

        /// <summary>
        /// Generates ensemble predictions using multiple methods
        /// </summary>
        private List<PredictionResult> GeneratePredictions(List<MarketDataPoint> data, List<FractalPattern> patterns)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🎯 STEP 5: ENSEMBLE PREDICTION ENGINE");
            Console.WriteLine("═════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("🧠 Generating predictions using:");
            Console.WriteLine("   • Trend following analysis");
            Console.WriteLine("   • Mean reversion signals");
            Console.WriteLine("   • Fractal pattern recognition");
            Console.WriteLine("   • Volatility analysis");
            Console.WriteLine("   • Momentum indicators");
            
            Console.WriteLine();
            var predictions = _behaviorAnalyzer.GenerateEnsemblePredictions(data, patterns);
            
            // Calculate consensus
            var weightedConsensus = predictions.Sum(p => p.PredictedDirection * p.Confidence);
            var totalWeight = predictions.Sum(p => p.Confidence);
            var consensus = totalWeight > 0 ? weightedConsensus / totalWeight : 0;
            
            Console.WriteLine($"\n✅ Ensemble predictions generated!");
            Console.WriteLine($"   Methods used: {predictions.Count}");
            Console.WriteLine($"   Average confidence: {predictions.Average(p => p.Confidence):P0}");
            
            var consensusDirection = consensus > 0.1 ? "BULLISH ▲" : consensus < -0.1 ? "BEARISH ▼" : "NEUTRAL ➡️";
            Console.WriteLine($"   Consensus direction: {consensusDirection}");
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            
            return predictions;
        }

        /// <summary>
        /// Displays comprehensive analysis results
        /// </summary>
        private void DisplayResults(
            List<MarketDataPoint> marketData, 
            List<FractalPattern> patterns, 
            MarketBehaviorAnalysis behavior,
            List<PredictionResult> predictions)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("📋 STEP 6: COMPREHENSIVE RESULTS");
            Console.WriteLine("═══════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            
            _visualizer.DisplayComprehensiveSummary(marketData, patterns, behavior, predictions);
            
            Console.WriteLine("Press any key to export results to CSV...");
            Console.ReadKey();
        }

        /// <summary>
        /// Exports all results to CSV files
        /// </summary>
        private void ExportResults(
            List<MarketDataPoint> marketData,
            List<FractalPattern> patterns,
            MarketBehaviorAnalysis behavior,
            List<PredictionResult> predictions)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("💾 STEP 7: DATA EXPORT");
            Console.WriteLine("══════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            
            _visualizer.ExportAllResults(marketData, patterns, behavior, predictions);
            
            Console.WriteLine("\n📁 Exported files:");
            Console.WriteLine("   • market_data_[timestamp].csv - Raw market data");
            Console.WriteLine("   • fractal_patterns_[timestamp].csv - Detected patterns");
            Console.WriteLine("   • market_behavior_[timestamp].csv - Behavior analysis");
            Console.WriteLine("   • predictions_[timestamp].csv - Ensemble predictions");
            Console.WriteLine("   • session_summary_[timestamp].csv - Session overview");
        }

        /// <summary>
        /// Shows the completion screen
        /// </summary>
        private void ShowCompletionScreen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    🎉 DEMO COMPLETE! 🎉                        ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("║              Fractal Market Analysis Finished                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine("📊 What was demonstrated:");
            Console.WriteLine("   ✅ Market data simulation with fractal mathematics");
            Console.WriteLine("   ✅ Box-counting fractal dimension calculations");
            Console.WriteLine("   ✅ Self-similarity pattern detection");
            Console.WriteLine("   ✅ Dynamic time warping for pattern matching");
            Console.WriteLine("   ✅ Trend analysis and volatility clustering");
            Console.WriteLine("   ✅ Ensemble prediction methods");
            Console.WriteLine("   ✅ Comprehensive data visualization");
            Console.WriteLine("   ✅ CSV export for further analysis");
            
            Console.WriteLine();
            Console.WriteLine("🔬 Key Concepts Explored:");
            Console.WriteLine("   • Fractal dimension analysis (Box-counting method)");
            Console.WriteLine("   • Geometric Brownian motion with fractal noise");
            Console.WriteLine("   • Volatility clustering and mean reversion");
            Console.WriteLine("   • Multi-scale pattern recognition");
            Console.WriteLine("   • Ensemble machine learning approaches");
            
            Console.WriteLine();
            Console.WriteLine("📚 Educational Value:");
            Console.WriteLine("   This demo illustrates how fractal mathematics can be applied");
            Console.WriteLine("   to financial market analysis, combining rigorous mathematical");
            Console.WriteLine("   methods with practical trading system concepts.");
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💡 Next Steps:");
            Console.ResetColor();
            Console.WriteLine("   • Examine the exported CSV files for detailed analysis");
            Console.WriteLine("   • Experiment with different fractal dimensions");
            Console.WriteLine("   • Try varying the market volatility parameters");
            Console.WriteLine("   • Explore the pattern recognition algorithms");
            
            Console.WriteLine();
            Console.WriteLine("Thank you for exploring Fractal Mathematics in Trading Systems!");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
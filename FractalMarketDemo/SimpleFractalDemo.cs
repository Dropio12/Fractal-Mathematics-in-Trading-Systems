using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleFractalDemo
{
    public class MarketDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public double Returns { get; set; }
        public double Volatility { get; set; }
        
        public MarketDataPoint(DateTime timestamp, double price, double volume = 1000)
        {
            Timestamp = timestamp;
            Price = price;
            Volume = volume;
        }
    }

    public class FractalPattern
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public double FractalDimension { get; set; }
        public double Confidence { get; set; }
        public string PatternType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public int Length { get { return EndIndex - StartIndex + 1; } }
    }

    public class SimpleFractalMarketDemo
    {
        private static Random random = new Random(42);
        
        public static void Main(string[] args)
        {
            try
            {
                Console.Clear();
                ShowHeader();
                
                // Generate market data
                Console.WriteLine("🎲 Generating market data with fractal noise...");
                var marketData = GenerateMarketData(200, 100.0);
                Console.WriteLine("✅ Generated {0} data points", marketData.Count);
                
                // Detect patterns
                Console.WriteLine("\n🔍 Detecting fractal patterns...");
                var patterns = DetectFractalPatterns(marketData);
                Console.WriteLine("✅ Found {0} patterns", patterns.Count);
                
                // Display results
                Console.WriteLine("\n📊 MARKET DATA SUMMARY");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━");
                DisplayMarketSummary(marketData);
                
                Console.WriteLine("\n🔍 FRACTAL PATTERNS DETECTED");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                DisplayPatterns(patterns);
                
                Console.WriteLine("\n💾 Exporting to CSV...");
                ExportToCsv(marketData, patterns);
                
                Console.WriteLine("\n🎉 Demo completed successfully!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: {0}", ex.Message);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        
        private static void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    🔢 FRACTAL MARKET DEMO 🔢                   ║");
            Console.WriteLine("║              Fractal Mathematics in Trading Systems           ║");
            Console.WriteLine("║                     Simplified Version                        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
        
        private static List<MarketDataPoint> GenerateMarketData(int dataPoints, double initialPrice)
        {
            var data = new List<MarketDataPoint>();
            var currentPrice = initialPrice;
            var startTime = DateTime.Now.AddDays(-dataPoints / 24.0);
            
            for (int i = 0; i < dataPoints; i++)
            {
                var timestamp = startTime.AddHours(i);
                
                // Generate fractal noise
                var fractalNoise = GenerateFractalNoise(i);
                
                // Geometric Brownian motion
                var randomComponent = GenerateGaussianRandom();
                var drift = 0.0002;
                var volatility = 0.025;
                var priceChange = drift + (volatility * (randomComponent + fractalNoise * 0.3));
                
                currentPrice *= (1 + priceChange);
                
                var volume = 1000 + Math.Abs(randomComponent) * 500;
                var dataPoint = new MarketDataPoint(timestamp, currentPrice, volume);
                
                if (i > 0)
                {
                    dataPoint.Returns = (currentPrice - data[i - 1].Price) / data[i - 1].Price;
                }
                
                data.Add(dataPoint);
            }
            
            // Calculate volatility
            for (int i = 20; i < data.Count; i++)
            {
                var returns = new List<double>();
                for (int j = i - 20; j < i; j++)
                {
                    returns.Add(data[j].Returns);
                }
                var mean = returns.Average();
                var variance = returns.Select(r => Math.Pow(r - mean, 2)).Average();
                data[i].Volatility = Math.Sqrt(variance);
            }
            
            return data;
        }
        
        private static double GenerateFractalNoise(int timeIndex)
        {
            double noise = 0.0;
            double amplitude = 1.0;
            double frequency = 1.0;
            
            for (int octave = 0; octave < 4; octave++)
            {
                var phase = (timeIndex * frequency * 0.1) % (2 * Math.PI);
                var sineWave = Math.Sin(phase) + Math.Sin(phase * 1.618) * 0.5;
                
                noise += amplitude * sineWave * GenerateGaussianRandom() * 0.1;
                amplitude *= 0.5;
                frequency *= 2.0;
            }
            
            return noise;
        }
        
        private static double GenerateGaussianRandom()
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
        
        private static List<FractalPattern> DetectFractalPatterns(List<MarketDataPoint> data)
        {
            var patterns = new List<FractalPattern>();
            
            for (int start = 0; start < data.Count - 30; start += 10)
            {
                for (int length = 25; length <= Math.Min(50, data.Count - start); length += 5)
                {
                    var segment = data.Skip(start).Take(length).Select(d => d.Price).ToArray();
                    var fractalDimension = CalculateFractalDimension(segment);
                    
                    if (fractalDimension > 1.1 && fractalDimension < 1.9)
                    {
                        var confidence = CalculateConfidence(fractalDimension, length);
                        
                        if (confidence > 0.3)
                        {
                            var pattern = new FractalPattern
                            {
                                StartIndex = start,
                                EndIndex = start + length - 1,
                                FractalDimension = fractalDimension,
                                Confidence = confidence,
                                PatternType = ClassifyPattern(fractalDimension, segment),
                                StartTime = data[start].Timestamp,
                                EndTime = data[start + length - 1].Timestamp
                            };
                            
                            patterns.Add(pattern);
                            start += length / 2; // Skip ahead
                            break;
                        }
                    }
                }
            }
            
            return patterns.OrderByDescending(p => p.Confidence).Take(5).ToList();
        }
        
        private static double CalculateFractalDimension(double[] prices)
        {
            if (prices.Length < 4) return 1.0;
            
            var boxSizes = new int[] { 1, 2, 3, 4, 5 };
            var logBoxSizes = new List<double>();
            var logBoxCounts = new List<double>();
            
            var min = prices.Min();
            var max = prices.Max();
            var range = max - min;
            
            if (range == 0) return 1.0;
            
            var normalizedPrices = prices.Select(p => (p - min) / range).ToArray();
            
            foreach (var boxSize in boxSizes)
            {
                if (boxSize >= prices.Length / 2) break;
                
                var boxCount = CountBoxes(normalizedPrices, boxSize);
                if (boxCount > 0)
                {
                    logBoxSizes.Add(Math.Log(1.0 / boxSize));
                    logBoxCounts.Add(Math.Log(boxCount));
                }
            }
            
            if (logBoxSizes.Count < 3) return 1.0;
            
            return CalculateSlope(logBoxSizes.ToArray(), logBoxCounts.ToArray());
        }
        
        private static int CountBoxes(double[] normalizedPrices, int boxSize)
        {
            var boxes = new HashSet<string>();
            
            for (int i = 0; i < normalizedPrices.Length - 1; i++)
            {
                var x1 = i / boxSize;
                var y1 = (int)(normalizedPrices[i] / (1.0 / boxSize));
                var x2 = (i + 1) / boxSize;
                var y2 = (int)(normalizedPrices[i + 1] / (1.0 / boxSize));
                
                boxes.Add(x1 + "," + y1);
                boxes.Add(x2 + "," + y2);
            }
            
            return boxes.Count;
        }
        
        private static double CalculateSlope(double[] x, double[] y)
        {
            var n = x.Length;
            var sumX = x.Sum();
            var sumY = y.Sum();
            var sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            var sumXX = x.Sum(xi => xi * xi);
            
            var denominator = n * sumXX - sumX * sumX;
            if (Math.Abs(denominator) < 1e-10) return 1.0;
            
            return (n * sumXY - sumX * sumY) / denominator;
        }
        
        private static double CalculateConfidence(double fractalDimension, int length)
        {
            var dimensionScore = 1.0 - Math.Abs(fractalDimension - 1.4) / 0.4;
            var lengthScore = Math.Min(1.0, length / 40.0);
            
            return Math.Max(0, Math.Min(1, (dimensionScore * 0.7) + (lengthScore * 0.3)));
        }
        
        private static string ClassifyPattern(double fractalDimension, double[] prices)
        {
            if (fractalDimension < 1.3) return "Smooth Trend";
            if (fractalDimension > 1.6) return "Highly Volatile";
            
            var startPrice = prices[0];
            var endPrice = prices[prices.Length - 1];
            var change = (endPrice - startPrice) / startPrice;
            
            if (change > 0.05) return "Upward Fractal";
            if (change < -0.05) return "Downward Fractal";
            return "Sideways Fractal";
        }
        
        private static void DisplayMarketSummary(List<MarketDataPoint> data)
        {
            var startPrice = data[0].Price;
            var endPrice = data[data.Count - 1].Price;
            var totalReturn = (endPrice - startPrice) / startPrice;
            var avgVolatility = data.Where(d => d.Volatility > 0).Average(d => d.Volatility);
            
            Console.WriteLine("📊 Data Points:      {0:N0}", data.Count);
            Console.WriteLine("💰 Starting Price:   ${0:F2}", startPrice);
            Console.WriteLine("💰 Ending Price:     ${0:F2}", endPrice);
            Console.WriteLine("📈 Total Return:     {0:P2} {1}", totalReturn, totalReturn > 0 ? "▲" : "▼");
            Console.WriteLine("📊 Avg Volatility:   {0:P2}", avgVolatility);
        }
        
        private static void DisplayPatterns(List<FractalPattern> patterns)
        {
            if (patterns.Count == 0)
            {
                Console.WriteLine("❌ No significant patterns detected");
                return;
            }
            
            for (int i = 0; i < patterns.Count; i++)
            {
                var pattern = patterns[i];
                Console.WriteLine("{0}. {1} (Confidence: {2:P0})", 
                    i + 1, pattern.PatternType, pattern.Confidence);
                Console.WriteLine("   📐 Fractal Dimension: {0:F3}", pattern.FractalDimension);
                Console.WriteLine("   🕐 Duration: {0} periods", pattern.Length);
                Console.WriteLine();
            }
        }
        
        private static void ExportToCsv(List<MarketDataPoint> data, List<FractalPattern> patterns)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            // Export market data
            using (var writer = new StreamWriter("market_data_" + timestamp + ".csv"))
            {
                writer.WriteLine("Timestamp,Price,Volume,Returns,Volatility");
                foreach (var point in data)
                {
                    writer.WriteLine("{0:yyyy-MM-dd HH:mm:ss},{1:F4},{2:F2},{3:F6},{4:F6}",
                        point.Timestamp, point.Price, point.Volume, point.Returns, point.Volatility);
                }
            }
            
            // Export patterns
            using (var writer = new StreamWriter("fractal_patterns_" + timestamp + ".csv"))
            {
                writer.WriteLine("StartIndex,EndIndex,Duration,PatternType,FractalDimension,Confidence");
                foreach (var pattern in patterns)
                {
                    writer.WriteLine("{0},{1},{2},{3},{4:F4},{5:F4}",
                        pattern.StartIndex, pattern.EndIndex, pattern.Length, 
                        pattern.PatternType, pattern.FractalDimension, pattern.Confidence);
                }
            }
            
            Console.WriteLine("✅ Exported market_data_{0}.csv", timestamp);
            Console.WriteLine("✅ Exported fractal_patterns_{0}.csv", timestamp);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalMarketDemo
{
    /// <summary>
    /// Represents a detected fractal pattern
    /// </summary>
    public class FractalPattern
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public double FractalDimension { get; set; }
        public double SelfSimilarityIndex { get; set; }
        public double[] NormalizedPrices { get; set; } = Array.Empty<double>();
        public string PatternType { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public int Length => EndIndex - StartIndex + 1;
    }

    /// <summary>
    /// Core fractal pattern recognition using box-counting method and self-similarity detection
    /// </summary>
    public class FractalPatternRecognizer
    {
        private readonly double _minPatternLength;
        private readonly double _maxPatternLength;
        private readonly double _similarityThreshold;
        
        public FractalPatternRecognizer(int minPatternLength = 20, int maxPatternLength = 100, double similarityThreshold = 0.7)
        {
            _minPatternLength = minPatternLength;
            _maxPatternLength = maxPatternLength;
            _similarityThreshold = similarityThreshold;
        }

        /// <summary>
        /// Detects fractal patterns in market data using box-counting and self-similarity analysis
        /// </summary>
        public List<FractalPattern> DetectPatterns(List<MarketDataPoint> data)
        {
            var patterns = new List<FractalPattern>();
            
            Console.WriteLine($"🔍 Scanning {data.Count} data points for fractal patterns...");
            
            // Scan through the data looking for patterns
            for (int start = 0; start < data.Count - _minPatternLength; start++)
            {
                for (int length = (int)_minPatternLength; length <= Math.Min(_maxPatternLength, data.Count - start); length += 5)
                {
                    int end = start + length - 1;
                    
                    // Extract price segment
                    var priceSegment = data.Skip(start).Take(length).Select(d => d.Price).ToArray();
                    
                    // Calculate fractal dimension using box-counting method
                    var fractalDimension = CalculateFractalDimension(priceSegment);
                    
                    // Check if this looks like a fractal pattern (typically between 1.1 and 1.9)
                    if (fractalDimension > 1.1 && fractalDimension < 1.9)
                    {
                        // Normalize prices for pattern comparison
                        var normalizedPrices = NormalizePrices(priceSegment);
                        
                        // Check for self-similarity with existing patterns
                        var selfSimilarityIndex = CalculateSelfSimilarityWithKnownPatterns(normalizedPrices, patterns);
                        
                        // Calculate pattern confidence based on various factors
                        var confidence = CalculatePatternConfidence(fractalDimension, selfSimilarityIndex, length);
                        
                        if (confidence > 0.6) // Only keep high-confidence patterns
                        {
                            var pattern = new FractalPattern
                            {
                                StartIndex = start,
                                EndIndex = end,
                                FractalDimension = fractalDimension,
                                SelfSimilarityIndex = selfSimilarityIndex,
                                NormalizedPrices = normalizedPrices,
                                PatternType = ClassifyPatternType(fractalDimension, normalizedPrices),
                                Confidence = confidence,
                                StartTime = data[start].Timestamp,
                                EndTime = data[end].Timestamp
                            };
                            
                            patterns.Add(pattern);
                            
                            // Skip ahead to avoid overlapping patterns
                            start += length / 3;
                            break;
                        }
                    }
                }
            }
            
            Console.WriteLine($"✅ Found {patterns.Count} fractal patterns");
            return patterns.OrderByDescending(p => p.Confidence).Take(10).ToList(); // Return top 10 patterns
        }

        /// <summary>
        /// Calculates fractal dimension using the box-counting method
        /// </summary>
        private double CalculateFractalDimension(double[] prices)
        {
            if (prices.Length < 4) return 1.0;

            var boxSizes = new List<int> { 1, 2, 3, 4, 5, 8, 10 };
            var logBoxSizes = new List<double>();
            var logBoxCounts = new List<double>();

            // Normalize prices to [0, 1] range
            var minPrice = prices.Min();
            var maxPrice = prices.Max();
            var range = maxPrice - minPrice;
            
            if (range == 0) return 1.0; // No variation

            var normalizedPrices = prices.Select(p => (p - minPrice) / range).ToArray();

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

            // Calculate slope using linear regression (fractal dimension = slope)
            return CalculateLinearRegressionSlope(logBoxSizes.ToArray(), logBoxCounts.ToArray());
        }

        /// <summary>
        /// Counts the number of boxes needed to cover the price curve at a given box size
        /// </summary>
        private int CountBoxes(double[] normalizedPrices, int boxSize)
        {
            var boxes = new HashSet<(int x, int y)>();
            
            for (int i = 0; i < normalizedPrices.Length - 1; i++)
            {
                var x1 = i / boxSize;
                var y1 = (int)(normalizedPrices[i] / (1.0 / boxSize));
                var x2 = (i + 1) / boxSize;
                var y2 = (int)(normalizedPrices[i + 1] / (1.0 / boxSize));

                // Add boxes along the line from (x1,y1) to (x2,y2)
                var steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                for (int step = 0; step <= steps; step++)
                {
                    var x = x1 + (x2 - x1) * step / Math.Max(steps, 1);
                    var y = y1 + (y2 - y1) * step / Math.Max(steps, 1);
                    boxes.Add((x, y));
                }
            }

            return boxes.Count;
        }

        /// <summary>
        /// Calculates linear regression slope for fractal dimension calculation
        /// </summary>
        private double CalculateLinearRegressionSlope(double[] x, double[] y)
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

        /// <summary>
        /// Normalizes prices to [0, 1] range for pattern comparison
        /// </summary>
        private double[] NormalizePrices(double[] prices)
        {
            var min = prices.Min();
            var max = prices.Max();
            var range = max - min;
            
            if (range == 0) return prices.Select(p => 0.5).ToArray();
            
            return prices.Select(p => (p - min) / range).ToArray();
        }

        /// <summary>
        /// Calculates self-similarity index with existing known patterns using Dynamic Time Warping
        /// </summary>
        private double CalculateSelfSimilarityWithKnownPatterns(double[] normalizedPrices, List<FractalPattern> existingPatterns)
        {
            if (existingPatterns.Count == 0) return 0.0;

            var maxSimilarity = 0.0;
            
            foreach (var existingPattern in existingPatterns.Take(5)) // Check against top 5 existing patterns
            {
                var similarity = CalculateDynamicTimeWarpingDistance(normalizedPrices, existingPattern.NormalizedPrices);
                maxSimilarity = Math.Max(maxSimilarity, similarity);
            }

            return maxSimilarity;
        }

        /// <summary>
        /// Calculates similarity using simplified Dynamic Time Warping
        /// </summary>
        private double CalculateDynamicTimeWarpingDistance(double[] pattern1, double[] pattern2)
        {
            if (pattern1.Length == 0 || pattern2.Length == 0) return 0.0;

            var m = pattern1.Length;
            var n = pattern2.Length;
            
            // Create DTW matrix
            var dtw = new double[m + 1, n + 1];
            
            // Initialize with infinity
            for (int i = 0; i <= m; i++)
                for (int j = 0; j <= n; j++)
                    dtw[i, j] = double.PositiveInfinity;
            
            dtw[0, 0] = 0;

            // Fill DTW matrix
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    var cost = Math.Abs(pattern1[i - 1] - pattern2[j - 1]);
                    dtw[i, j] = cost + Math.Min(Math.Min(dtw[i - 1, j], dtw[i, j - 1]), dtw[i - 1, j - 1]);
                }
            }

            // Convert distance to similarity (0 to 1, where 1 is most similar)
            var maxDistance = Math.Max(m, n); // Normalize by maximum possible distance
            var similarity = Math.Max(0, 1.0 - (dtw[m, n] / maxDistance));
            
            return similarity;
        }

        /// <summary>
        /// Calculates pattern confidence based on various fractal properties
        /// </summary>
        private double CalculatePatternConfidence(double fractalDimension, double selfSimilarityIndex, int patternLength)
        {
            // Confidence factors:
            // 1. Fractal dimension should be between 1.2 and 1.8 for good patterns
            var dimensionConfidence = 1.0 - Math.Abs(fractalDimension - 1.5) / 0.5;
            dimensionConfidence = Math.Max(0, Math.Min(1, dimensionConfidence));
            
            // 2. Longer patterns are generally more reliable
            var lengthConfidence = Math.Min(1.0, patternLength / 50.0);
            
            // 3. Self-similarity adds to confidence
            var similarityConfidence = selfSimilarityIndex;
            
            // 4. Combine factors with weights
            var totalConfidence = (dimensionConfidence * 0.4) + 
                                (lengthConfidence * 0.3) + 
                                (similarityConfidence * 0.3);
            
            return Math.Max(0, Math.Min(1, totalConfidence));
        }

        /// <summary>
        /// Classifies the pattern type based on fractal dimension and shape characteristics
        /// </summary>
        private string ClassifyPatternType(double fractalDimension, double[] normalizedPrices)
        {
            if (normalizedPrices.Length < 4) return "Unknown";

            // Analyze trend direction
            var startPrice = normalizedPrices[0];
            var endPrice = normalizedPrices[^1];
            var midPrice = normalizedPrices[normalizedPrices.Length / 2];
            
            // Classify based on fractal dimension and trend
            if (fractalDimension < 1.3)
            {
                return "Smooth Trend";
            }
            else if (fractalDimension > 1.7)
            {
                return "Highly Volatile";
            }
            else if (endPrice > startPrice + 0.1)
            {
                return "Upward Fractal";
            }
            else if (endPrice < startPrice - 0.1)
            {
                return "Downward Fractal";
            }
            else if (Math.Abs(midPrice - (startPrice + endPrice) / 2) > 0.2)
            {
                return "Reversal Pattern";
            }
            else
            {
                return "Sideways Fractal";
            }
        }
    }
}
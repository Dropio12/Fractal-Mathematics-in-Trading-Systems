using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FMTS.FractalAnalysis
{
    [System.Serializable]
    public class MarketDataPoint
    {
        public float timestamp;
        public float price;
        public float volume;
        public MarketDirection direction;
        
        public MarketDataPoint(float time, float priceValue, float vol, MarketDirection dir)
        {
            timestamp = time;
            price = priceValue;
            volume = vol;
            direction = dir;
        }
    }
    
    public enum MarketDirection
    {
        Up,
        Down,
        Neutral
    }
    
    [System.Serializable]
    public class FractalPattern
    {
        public string patternId;
        public List&lt;Vector2&gt; normalizedPoints;
        public float selfSimilarityIndex;
        public int occurrenceCount;
        public float avgPredictionAccuracy;
        public MarketDirection dominantDirection;
        public float confidence;
        
        public FractalPattern()
        {
            normalizedPoints = new List&lt;Vector2&gt;();
            occurrenceCount = 1;
            avgPredictionAccuracy = 0f;
            confidence = 0f;
        }
    }
    
    public class FractalPatternRecognition : MonoBehaviour
    {
        [Header("Fractal Analysis Settings")]
        public int maxPatternLength = 50;
        public int minPatternLength = 10;
        public float similarityThreshold = 0.85f;
        public int maxStoredPatterns = 1000;
        
        [Header("Self-Similarity Detection")]
        public float selfSimilarityScale = 2.0f;
        public int fractalDepth = 3;
        
        private List&lt;MarketDataPoint&gt; marketHistory;
        private List&lt;FractalPattern&gt; discoveredPatterns;
        private Queue&lt;MarketDataPoint&gt; recentData;
        
        // Fractal dimension calculation parameters
        private const int MAX_BOXES = 1000;
        private const float MIN_SCALE = 0.001f;
        
        void Awake()
        {
            marketHistory = new List&lt;MarketDataPoint&gt;();
            discoveredPatterns = new List&lt;FractalPattern&gt;();
            recentData = new Queue&lt;MarketDataPoint&gt;();
        }
        
        public void AddMarketData(float price, float volume, MarketDirection direction)
        {
            var dataPoint = new MarketDataPoint(Time.time, price, volume, direction);
            marketHistory.Add(dataPoint);
            recentData.Enqueue(dataPoint);
            
            // Keep recent data queue manageable
            while (recentData.Count &gt; maxPatternLength * 3)
            {
                recentData.Dequeue();
            }
            
            // Analyze patterns periodically
            if (marketHistory.Count % 20 == 0)
            {
                AnalyzeRecentPatterns();
            }
        }
        
        private void AnalyzeRecentPatterns()
        {
            if (recentData.Count &lt; minPatternLength) return;
            
            var recentDataList = recentData.ToList();
            
            // Extract potential patterns of various lengths
            for (int patternLen = minPatternLength; patternLen &lt;= Math.Min(maxPatternLength, recentDataList.Count); patternLen++)
            {
                for (int start = 0; start &lt;= recentDataList.Count - patternLen; start++)
                {
                    var candidatePattern = ExtractPattern(recentDataList, start, patternLen);
                    
                    if (candidatePattern != null)
                    {
                        AnalyzeFractalProperties(candidatePattern);
                        CheckForSimilarPatterns(candidatePattern);
                    }
                }
            }
        }
        
        private FractalPattern ExtractPattern(List&lt;MarketDataPoint&gt; data, int start, int length)
        {
            if (start + length &gt; data.Count) return null;
            
            var pattern = new FractalPattern();
            pattern.patternId = GeneratePatternId(data, start, length);
            
            // Normalize the pattern to make it scale-invariant
            var segment = data.GetRange(start, length);
            pattern.normalizedPoints = NormalizePattern(segment);
            pattern.dominantDirection = CalculateDominantDirection(segment);
            
            // Calculate self-similarity index
            pattern.selfSimilarityIndex = CalculateSelfSimilarity(pattern.normalizedPoints);
            
            return pattern;
        }
        
        private List&lt;Vector2&gt; NormalizePattern(List&lt;MarketDataPoint&gt; segment)
        {
            if (segment.Count &lt; 2) return new List&lt;Vector2&gt;();
            
            var normalized = new List&lt;Vector2&gt;();
            
            float minPrice = segment.Min(p =&gt; p.price);
            float maxPrice = segment.Max(p =&gt; p.price);
            float priceRange = maxPrice - minPrice;
            
            float startTime = segment[0].timestamp;
            float timeRange = segment[segment.Count - 1].timestamp - startTime;
            
            // Avoid division by zero
            if (priceRange == 0) priceRange = 1;
            if (timeRange == 0) timeRange = 1;
            
            foreach (var point in segment)
            {
                float normalizedTime = (point.timestamp - startTime) / timeRange;
                float normalizedPrice = (point.price - minPrice) / priceRange;
                normalized.Add(new Vector2(normalizedTime, normalizedPrice));
            }
            
            return normalized;
        }
        
        private float CalculateSelfSimilarity(List&lt;Vector2&gt; pattern)
        {
            if (pattern.Count &lt; 4) return 0f;
            
            // Calculate fractal dimension using box-counting method
            float fractalDimension = CalculateFractalDimension(pattern);
            
            // Check for self-similar subpatterns at different scales
            float selfSimilarityScore = 0f;
            int comparisons = 0;
            
            for (int scale = 2; scale &lt;= fractalDepth; scale++)
            {
                int segmentSize = pattern.Count / scale;
                if (segmentSize &lt; 3) break;
                
                for (int i = 0; i &lt; scale - 1; i++)
                {
                    var segment1 = pattern.GetRange(i * segmentSize, segmentSize);
                    var segment2 = pattern.GetRange((i + 1) * segmentSize, segmentSize);
                    
                    float similarity = CalculatePatternSimilarity(segment1, segment2);
                    selfSimilarityScore += similarity;
                    comparisons++;
                }
            }
            
            return comparisons &gt; 0 ? (selfSimilarityScore / comparisons) * (fractalDimension / 2.0f) : 0f;
        }
        
        private float CalculateFractalDimension(List&lt;Vector2&gt; pattern)
        {
            if (pattern.Count &lt; 3) return 1.0f;
            
            List&lt;float&gt; scales = new List&lt;float&gt;();
            List&lt;float&gt; boxCounts = new List&lt;float&gt;();
            
            // Generate scales from 1/N to 1/2 where N is pattern length
            for (int n = 2; n &lt;= Math.Min(pattern.Count / 2, 20); n++)
            {
                float scale = 1.0f / n;
                int boxes = CountBoxesCovering(pattern, scale);
                
                if (boxes &gt; 0)
                {
                    scales.Add(Mathf.Log(scale));
                    boxCounts.Add(Mathf.Log(boxes));
                }
            }
            
            if (scales.Count &lt; 2) return 1.5f; // Default fractal dimension
            
            // Calculate slope of log-log plot (fractal dimension)
            float dimension = -CalculateSlope(scales, boxCounts);
            return Mathf.Clamp(dimension, 1.0f, 2.0f);
        }
        
        private int CountBoxesCovering(List&lt;Vector2&gt; pattern, float boxSize)
        {
            HashSet&lt;string&gt; boxes = new HashSet&lt;string&gt;();
            
            foreach (var point in pattern)
            {
                int boxX = Mathf.FloorToInt(point.x / boxSize);
                int boxY = Mathf.FloorToInt(point.y / boxSize);
                boxes.Add($"{boxX},{boxY}");
            }
            
            return boxes.Count;
        }
        
        private float CalculateSlope(List&lt;float&gt; x, List&lt;float&gt; y)
        {
            if (x.Count != y.Count || x.Count &lt; 2) return 0f;
            
            float n = x.Count;
            float sumX = x.Sum();
            float sumY = y.Sum();
            float sumXY = 0f;
            float sumXX = 0f;
            
            for (int i = 0; i &lt; n; i++)
            {
                sumXY += x[i] * y[i];
                sumXX += x[i] * x[i];
            }
            
            float denominator = n * sumXX - sumX * sumX;
            if (Mathf.Abs(denominator) &lt; 0.0001f) return 0f;
            
            return (n * sumXY - sumX * sumY) / denominator;
        }
        
        private MarketDirection CalculateDominantDirection(List&lt;MarketDataPoint&gt; segment)
        {
            int upCount = 0;
            int downCount = 0;
            
            foreach (var point in segment)
            {
                switch (point.direction)
                {
                    case MarketDirection.Up:
                        upCount++;
                        break;
                    case MarketDirection.Down:
                        downCount++;
                        break;
                }
            }
            
            if (upCount &gt; downCount * 1.2f) return MarketDirection.Up;
            if (downCount &gt; upCount * 1.2f) return MarketDirection.Down;
            return MarketDirection.Neutral;
        }
        
        private void CheckForSimilarPatterns(FractalPattern newPattern)
        {
            FractalPattern bestMatch = null;
            float bestSimilarity = 0f;
            
            foreach (var existingPattern in discoveredPatterns)
            {
                float similarity = CalculatePatternSimilarity(newPattern.normalizedPoints, existingPattern.normalizedPoints);
                
                if (similarity &gt; similarityThreshold && similarity &gt; bestSimilarity)
                {
                    bestMatch = existingPattern;
                    bestSimilarity = similarity;
                }
            }
            
            if (bestMatch != null)
            {
                // Update existing pattern
                bestMatch.occurrenceCount++;
                bestMatch.confidence = Mathf.Min(bestMatch.confidence + 0.1f, 1.0f);
            }
            else if (newPattern.selfSimilarityIndex &gt; 0.3f)
            {
                // Add new pattern if it has sufficient self-similarity
                newPattern.confidence = newPattern.selfSimilarityIndex;
                discoveredPatterns.Add(newPattern);
                
                // Keep patterns list manageable
                if (discoveredPatterns.Count &gt; maxStoredPatterns)
                {
                    discoveredPatterns = discoveredPatterns
                        .OrderByDescending(p =&gt; p.confidence * p.occurrenceCount)
                        .Take(maxStoredPatterns)
                        .ToList();
                }
            }
        }
        
        private float CalculatePatternSimilarity(List&lt;Vector2&gt; pattern1, List&lt;Vector2&gt; pattern2)
        {
            if (pattern1.Count == 0 || pattern2.Count == 0) return 0f;
            
            // Resample patterns to same length for comparison
            int targetLength = Mathf.Min(pattern1.Count, pattern2.Count);
            var resampled1 = ResamplePattern(pattern1, targetLength);
            var resampled2 = ResamplePattern(pattern2, targetLength);
            
            // Calculate Dynamic Time Warping distance
            float dtwDistance = CalculateDTWDistance(resampled1, resampled2);
            
            // Convert distance to similarity (0-1 range)
            return Mathf.Exp(-dtwDistance);
        }
        
        private List&lt;Vector2&gt; ResamplePattern(List&lt;Vector2&gt; pattern, int targetLength)
        {
            if (pattern.Count == targetLength) return new List&lt;Vector2&gt;(pattern);
            
            var resampled = new List&lt;Vector2&gt;();
            float step = (pattern.Count - 1) / (float)(targetLength - 1);
            
            for (int i = 0; i &lt; targetLength; i++)
            {
                float index = i * step;
                int lowerIndex = Mathf.FloorToInt(index);
                int upperIndex = Mathf.Min(lowerIndex + 1, pattern.Count - 1);
                float t = index - lowerIndex;
                
                if (lowerIndex == upperIndex)
                {
                    resampled.Add(pattern[lowerIndex]);
                }
                else
                {
                    Vector2 interpolated = Vector2.Lerp(pattern[lowerIndex], pattern[upperIndex], t);
                    resampled.Add(interpolated);
                }
            }
            
            return resampled;
        }
        
        private float CalculateDTWDistance(List&lt;Vector2&gt; series1, List&lt;Vector2&gt; series2)
        {
            int n = series1.Count;
            int m = series2.Count;
            
            float[,] dtw = new float[n + 1, m + 1];
            
            // Initialize with infinity
            for (int i = 0; i &lt;= n; i++)
                for (int j = 0; j &lt;= m; j++)
                    dtw[i, j] = float.PositiveInfinity;
            
            dtw[0, 0] = 0;
            
            for (int i = 1; i &lt;= n; i++)
            {
                for (int j = 1; j &lt;= m; j++)
                {
                    float cost = Vector2.Distance(series1[i - 1], series2[j - 1]);
                    dtw[i, j] = cost + Mathf.Min(
                        dtw[i - 1, j],      // insertion
                        Mathf.Min(
                            dtw[i, j - 1],  // deletion
                            dtw[i - 1, j - 1] // match
                        )
                    );
                }
            }
            
            return dtw[n, m] / (n + m); // Normalize by path length
        }
        
        private string GeneratePatternId(List&lt;MarketDataPoint&gt; data, int start, int length)
        {
            var segment = data.GetRange(start, length);
            float hash = 0f;
            
            foreach (var point in segment)
            {
                hash = (hash * 31 + point.price * 1000) % 1000000;
            }
            
            return $"Pattern_{hash:F0}_{length}_{Time.time:F0}";
        }
        
        public List&lt;FractalPattern&gt; GetDiscoveredPatterns()
        {
            return new List&lt;FractalPattern&gt;(discoveredPatterns);
        }
        
        public FractalPattern GetMostConfidentPattern()
        {
            return discoveredPatterns
                .Where(p =&gt; p.occurrenceCount &gt; 1)
                .OrderByDescending(p =&gt; p.confidence * p.selfSimilarityIndex)
                .FirstOrDefault();
        }
        
        public Dictionary&lt;MarketDirection, float&gt; GetDirectionRatios()
        {
            if (marketHistory.Count == 0)
                return new Dictionary&lt;MarketDirection, float&gt;
                {
                    { MarketDirection.Up, 0.33f },
                    { MarketDirection.Down, 0.33f },
                    { MarketDirection.Neutral, 0.34f }
                };
            
            int upCount = marketHistory.Count(p =&gt; p.direction == MarketDirection.Up);
            int downCount = marketHistory.Count(p =&gt; p.direction == MarketDirection.Down);
            int neutralCount = marketHistory.Count(p =&gt; p.direction == MarketDirection.Neutral);
            
            float total = marketHistory.Count;
            
            return new Dictionary&lt;MarketDirection, float&gt;
            {
                { MarketDirection.Up, upCount / total },
                { MarketDirection.Down, downCount / total },
                { MarketDirection.Neutral, neutralCount / total }
            };
        }
    }
}
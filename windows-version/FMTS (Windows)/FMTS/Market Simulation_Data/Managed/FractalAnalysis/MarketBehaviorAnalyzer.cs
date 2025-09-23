using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FMTS.FractalAnalysis
{
    [System.Serializable]
    public class MarketTrend
    {
        public MarketDirection direction;
        public float strength;
        public float duration;
        public float volatility;
        public float confidence;
        public DateTime startTime;
        public DateTime endTime;
        
        public MarketTrend(MarketDirection dir, float str, float dur, float vol)
        {
            direction = dir;
            strength = str;
            duration = dur;
            volatility = vol;
            confidence = 0f;
            startTime = DateTime.Now;
            endTime = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class MarketBehaviorMetrics
    {
        public float upTrendRatio;
        public float downTrendRatio;
        public float neutralTrendRatio;
        public float averageVolatility;
        public float trendPersistence;
        public float reversalFrequency;
        public float fractalComplexity;
        public int totalPatterns;
        public DateTime analysisTimestamp;
        
        public MarketBehaviorMetrics()
        {
            analysisTimestamp = DateTime.Now;
        }
    }
    
    public class MarketBehaviorAnalyzer : MonoBehaviour
    {
        [Header("Analysis Configuration")]
        public int trendAnalysisWindow = 100;
        public float volatilityThreshold = 0.02f;
        public float trendStrengthThreshold = 0.15f;
        public int behaviorUpdateFrequency = 50;
        
        [Header("Trend Detection")]
        public float trendConfidenceThreshold = 0.6f;
        public int minTrendDuration = 5;
        public float reversalSensitivity = 0.8f;
        
        private FractalPatternRecognition patternRecognition;
        private List&lt;MarketTrend&gt; identifiedTrends;
        private List&lt;MarketBehaviorMetrics&gt; behaviorHistory;
        private Queue&lt;float&gt; priceMovements;
        private Queue&lt;float&gt; volatilityBuffer;
        
        // Moving averages for trend analysis
        private Queue&lt;float&gt; shortTermMA;
        private Queue&lt;float&gt; mediumTermMA;
        private Queue&lt;float&gt; longTermMA;
        
        private float lastAnalysisTime;
        private MarketBehaviorMetrics currentMetrics;
        
        void Awake()
        {
            patternRecognition = GetComponent&lt;FractalPatternRecognition&gt;();
            if (patternRecognition == null)
                patternRecognition = gameObject.AddComponent&lt;FractalPatternRecognition&gt;();
            
            identifiedTrends = new List&lt;MarketTrend&gt;();
            behaviorHistory = new List&lt;MarketBehaviorMetrics&gt;();
            priceMovements = new Queue&lt;float&gt;();
            volatilityBuffer = new Queue&lt;float&gt;();
            
            shortTermMA = new Queue&lt;float&gt;();
            mediumTermMA = new Queue&lt;float&gt;();
            longTermMA = new Queue&lt;float&gt;();
            
            currentMetrics = new MarketBehaviorMetrics();
            lastAnalysisTime = Time.time;
        }
        
        public void AnalyzeMarketBehavior(float price, float volume)
        {
            // Calculate price movement
            if (priceMovements.Count &gt; 0)
            {
                float lastPrice = priceMovements.Last();
                float movement = (price - lastPrice) / lastPrice;
                AddPriceMovement(movement);
            }
            
            priceMovements.Enqueue(price);
            if (priceMovements.Count &gt; trendAnalysisWindow)
                priceMovements.Dequeue();
            
            // Update moving averages
            UpdateMovingAverages(price);
            
            // Calculate volatility
            float volatility = CalculateVolatility();
            volatilityBuffer.Enqueue(volatility);
            if (volatilityBuffer.Count &gt; trendAnalysisWindow / 2)
                volatilityBuffer.Dequeue();
            
            // Determine market direction
            MarketDirection currentDirection = DetermineMarketDirection();
            
            // Add data to pattern recognition
            patternRecognition.AddMarketData(price, volume, currentDirection);
            
            // Perform behavioral analysis periodically
            if (Time.time - lastAnalysisTime &gt;= behaviorUpdateFrequency)
            {
                PerformBehaviorAnalysis();
                lastAnalysisTime = Time.time;
            }
        }
        
        private void AddPriceMovement(float movement)
        {
            // Keep track of recent price movements for trend analysis
            while (priceMovements.Count &gt; trendAnalysisWindow)
                priceMovements.Dequeue();
        }
        
        private void UpdateMovingAverages(float price)
        {
            // Short-term MA (10 periods)
            shortTermMA.Enqueue(price);
            if (shortTermMA.Count &gt; 10)
                shortTermMA.Dequeue();
            
            // Medium-term MA (20 periods)
            mediumTermMA.Enqueue(price);
            if (mediumTermMA.Count &gt; 20)
                mediumTermMA.Dequeue();
            
            // Long-term MA (50 periods)
            longTermMA.Enqueue(price);
            if (longTermMA.Count &gt; 50)
                longTermMA.Dequeue();
        }
        
        private float CalculateVolatility()
        {
            if (priceMovements.Count &lt; 10) return 0f;
            
            var movements = priceMovements.TakeLast(10).ToArray();
            float mean = movements.Average();
            float variance = movements.Select(x =&gt; (x - mean) * (x - mean)).Average();
            
            return Mathf.Sqrt(variance);
        }
        
        private MarketDirection DetermineMarketDirection()
        {
            if (shortTermMA.Count &lt; 5 || mediumTermMA.Count &lt; 5) 
                return MarketDirection.Neutral;
            
            float shortAvg = shortTermMA.Average();
            float mediumAvg = mediumTermMA.Average();
            float longAvg = longTermMA.Count &gt; 0 ? longTermMA.Average() : mediumAvg;
            
            // Trend strength analysis
            float shortMediumDiff = (shortAvg - mediumAvg) / mediumAvg;
            float mediumLongDiff = (mediumAvg - longAvg) / longAvg;
            
            if (shortMediumDiff &gt; trendStrengthThreshold && mediumLongDiff &gt; 0)
                return MarketDirection.Up;
            else if (shortMediumDiff &lt; -trendStrengthThreshold && mediumLongDiff &lt; 0)
                return MarketDirection.Down;
            else
                return MarketDirection.Neutral;
        }
        
        private void PerformBehaviorAnalysis()
        {
            if (priceMovements.Count &lt; minTrendDuration) return;
            
            // Analyze trend patterns
            AnalyzeTrendPatterns();
            
            // Calculate market behavior metrics
            UpdateMarketMetrics();
            
            // Update trend identification
            UpdateTrendIdentification();
            
            // Store current metrics in history
            behaviorHistory.Add(new MarketBehaviorMetrics
            {
                upTrendRatio = currentMetrics.upTrendRatio,
                downTrendRatio = currentMetrics.downTrendRatio,
                neutralTrendRatio = currentMetrics.neutralTrendRatio,
                averageVolatility = currentMetrics.averageVolatility,
                trendPersistence = currentMetrics.trendPersistence,
                reversalFrequency = currentMetrics.reversalFrequency,
                fractalComplexity = currentMetrics.fractalComplexity,
                totalPatterns = currentMetrics.totalPatterns,
                analysisTimestamp = DateTime.Now
            });
            
            // Keep history manageable
            if (behaviorHistory.Count &gt; 1000)
            {
                behaviorHistory = behaviorHistory.TakeLast(500).ToList();
            }
        }
        
        private void AnalyzeTrendPatterns()
        {
            var directionRatios = patternRecognition.GetDirectionRatios();
            var patterns = patternRecognition.GetDiscoveredPatterns();
            
            // Calculate trend persistence
            float persistence = CalculateTrendPersistence();
            
            // Calculate reversal frequency
            float reversalFreq = CalculateReversalFrequency();
            
            // Calculate fractal complexity
            float complexity = CalculateFractalComplexity(patterns);
            
            currentMetrics.trendPersistence = persistence;
            currentMetrics.reversalFrequency = reversalFreq;
            currentMetrics.fractalComplexity = complexity;
            currentMetrics.totalPatterns = patterns.Count;
        }
        
        private float CalculateTrendPersistence()
        {
            if (identifiedTrends.Count &lt; 2) return 0f;
            
            float totalPersistence = 0f;
            int validTrends = 0;
            
            foreach (var trend in identifiedTrends.TakeLast(20))
            {
                if (trend.duration &gt; minTrendDuration)
                {
                    totalPersistence += trend.strength * trend.confidence;
                    validTrends++;
                }
            }
            
            return validTrends &gt; 0 ? totalPersistence / validTrends : 0f;
        }
        
        private float CalculateReversalFrequency()
        {
            if (identifiedTrends.Count &lt; 3) return 0f;
            
            int reversals = 0;
            var recentTrends = identifiedTrends.TakeLast(50).ToArray();
            
            for (int i = 1; i &lt; recentTrends.Length; i++)
            {
                if (recentTrends[i].direction != recentTrends[i - 1].direction)
                {
                    reversals++;
                }
            }
            
            return (float)reversals / recentTrends.Length;
        }
        
        private float CalculateFractalComplexity(List&lt;FractalPattern&gt; patterns)
        {
            if (patterns.Count == 0) return 0f;
            
            float avgComplexity = patterns
                .Where(p =&gt; p.selfSimilarityIndex &gt; 0)
                .Select(p =&gt; p.selfSimilarityIndex * p.confidence)
                .DefaultIfEmpty(0f)
                .Average();
            
            return avgComplexity;
        }
        
        private void UpdateMarketMetrics()
        {
            var directionRatios = patternRecognition.GetDirectionRatios();
            
            currentMetrics.upTrendRatio = directionRatios[MarketDirection.Up];
            currentMetrics.downTrendRatio = directionRatios[MarketDirection.Down];
            currentMetrics.neutralTrendRatio = directionRatios[MarketDirection.Neutral];
            
            // Calculate average volatility
            if (volatilityBuffer.Count &gt; 0)
            {
                currentMetrics.averageVolatility = volatilityBuffer.Average();
            }
            
            currentMetrics.analysisTimestamp = DateTime.Now;
        }
        
        private void UpdateTrendIdentification()
        {
            if (priceMovements.Count &lt; 10) return;
            
            var recentPrices = priceMovements.TakeLast(20).ToArray();
            var currentDirection = DetermineMarketDirection();
            
            // Calculate trend strength
            float trendStrength = CalculateTrendStrength(recentPrices);
            float trendVolatility = CalculateVolatility();
            
            // Check if we should start a new trend or continue existing one
            var lastTrend = identifiedTrends.LastOrDefault();
            
            if (lastTrend == null || 
                lastTrend.direction != currentDirection || 
                Time.time - lastTrend.startTime.Ticks &gt; 300) // 5 minutes
            {
                // Start new trend
                var newTrend = new MarketTrend(currentDirection, trendStrength, 1f, trendVolatility);
                newTrend.confidence = CalculateTrendConfidence(newTrend);
                identifiedTrends.Add(newTrend);
            }
            else
            {
                // Update existing trend
                lastTrend.duration += 1f;
                lastTrend.strength = (lastTrend.strength + trendStrength) / 2f;
                lastTrend.volatility = (lastTrend.volatility + trendVolatility) / 2f;
                lastTrend.confidence = CalculateTrendConfidence(lastTrend);
                lastTrend.endTime = DateTime.Now;
            }
            
            // Remove old trends
            if (identifiedTrends.Count &gt; 200)
            {
                identifiedTrends = identifiedTrends.TakeLast(100).ToList();
            }
        }
        
        private float CalculateTrendStrength(float[] prices)
        {
            if (prices.Length &lt; 2) return 0f;
            
            float totalChange = (prices[prices.Length - 1] - prices[0]) / prices[0];
            return Mathf.Abs(totalChange);
        }
        
        private float CalculateTrendConfidence(MarketTrend trend)
        {
            float baseConfidence = Mathf.Min(trend.strength * 2f, 1f);
            float durationBonus = Mathf.Min(trend.duration / 20f, 0.3f);
            float volatilityPenalty = Mathf.Min(trend.volatility * 2f, 0.4f);
            
            return Mathf.Clamp01(baseConfidence + durationBonus - volatilityPenalty);
        }
        
        public MarketBehaviorMetrics GetCurrentMetrics()
        {
            return currentMetrics;
        }
        
        public List&lt;MarketTrend&gt; GetRecentTrends(int count = 20)
        {
            return identifiedTrends.TakeLast(count).ToList();
        }
        
        public List&lt;MarketBehaviorMetrics&gt; GetBehaviorHistory(int hours = 24)
        {
            var cutoffTime = DateTime.Now.AddHours(-hours);
            return behaviorHistory
                .Where(m =&gt; m.analysisTimestamp &gt; cutoffTime)
                .ToList();
        }
        
        public float GetTrendPredictionAccuracy()
        {
            if (identifiedTrends.Count &lt; 5) return 0f;
            
            var recentTrends = identifiedTrends.TakeLast(20);
            float totalAccuracy = 0f;
            int validPredictions = 0;
            
            foreach (var trend in recentTrends)
            {
                if (trend.confidence &gt; trendConfidenceThreshold && trend.duration &gt; minTrendDuration)
                {
                    totalAccuracy += trend.confidence;
                    validPredictions++;
                }
            }
            
            return validPredictions &gt; 0 ? totalAccuracy / validPredictions : 0f;
        }
        
        public Dictionary&lt;string, object&gt; GetAnalyticsSummary()
        {
            var summary = new Dictionary&lt;string, object&gt;
            {
                ["CurrentUpRatio"] = currentMetrics.upTrendRatio,
                ["CurrentDownRatio"] = currentMetrics.downTrendRatio,
                ["CurrentVolatility"] = currentMetrics.averageVolatility,
                ["TrendPersistence"] = currentMetrics.trendPersistence,
                ["ReversalFrequency"] = currentMetrics.reversalFrequency,
                ["FractalComplexity"] = currentMetrics.fractalComplexity,
                ["TotalPatterns"] = currentMetrics.totalPatterns,
                ["PredictionAccuracy"] = GetTrendPredictionAccuracy(),
                ["ActiveTrends"] = identifiedTrends.Count(t =&gt; t.confidence &gt; trendConfidenceThreshold),
                ["LastUpdate"] = currentMetrics.analysisTimestamp
            };
            
            return summary;
        }
    }
}
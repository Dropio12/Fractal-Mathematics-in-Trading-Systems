using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FMTS.FractalAnalysis
{
    [System.Serializable]
    public class MarketPrediction
    {
        public MarketDirection predictedDirection;
        public float confidence;
        public float expectedMagnitude;
        public float timeHorizon;
        public string reasoning;
        public DateTime predictionTime;
        public List&lt;string&gt; supportingPatternIds;
        
        public MarketPrediction()
        {
            supportingPatternIds = new List&lt;string&gt;();
            predictionTime = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class PredictionResult
    {
        public MarketPrediction prediction;
        public MarketDirection actualDirection;
        public float actualMagnitude;
        public bool wasCorrect;
        public float accuracyScore;
        public DateTime evaluationTime;
        
        public PredictionResult(MarketPrediction pred)
        {
            prediction = pred;
            evaluationTime = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class PatternWeight
    {
        public string patternId;
        public float weight;
        public float successRate;
        public int totalPredictions;
        public int correctPredictions;
        public DateTime lastUpdate;
        
        public PatternWeight(string id)
        {
            patternId = id;
            weight = 0.5f;
            successRate = 0f;
            totalPredictions = 0;
            correctPredictions = 0;
            lastUpdate = DateTime.Now;
        }
    }
    
    public class PredictiveInsightEngine : MonoBehaviour
    {
        [Header("Prediction Configuration")]
        public float minPredictionConfidence = 0.6f;
        public float learningRate = 0.1f;
        public int maxPatternWeights = 500;
        public float predictionTimeHorizon = 30f; // seconds
        
        [Header("Pattern Matching")]
        public float patternSimilarityThreshold = 0.7f;
        public int minPatternOccurrences = 3;
        public float recentWeightMultiplier = 1.5f;
        
        [Header("Ensemble Settings")]
        public int ensembleSize = 5;
        public float consensusThreshold = 0.6f;
        
        private FractalPatternRecognition patternRecognition;
        private MarketBehaviorAnalyzer behaviorAnalyzer;
        private Dictionary&lt;string, PatternWeight&gt; patternWeights;
        private List&lt;MarketPrediction&gt; activePredictions;
        private List&lt;PredictionResult&gt; predictionHistory;
        private Queue&lt;MarketDataPoint&gt; recentMarketData;
        
        // Neural network-inspired components
        private float[] inputLayer;
        private float[] hiddenLayer;
        private float[] outputLayer;
        private float[,] inputToHidden;
        private float[,] hiddenToOutput;
        
        private const int INPUT_SIZE = 20;
        private const int HIDDEN_SIZE = 10;
        private const int OUTPUT_SIZE = 3; // Up, Down, Neutral
        
        void Awake()
        {
            patternRecognition = GetComponent&lt;FractalPatternRecognition&gt;();
            behaviorAnalyzer = GetComponent&lt;MarketBehaviorAnalyzer&gt;();
            
            if (patternRecognition == null)
                patternRecognition = gameObject.AddComponent&lt;FractalPatternRecognition&gt;();
            if (behaviorAnalyzer == null)
                behaviorAnalyzer = gameObject.AddComponent&lt;MarketBehaviorAnalyzer&gt;();
            
            patternWeights = new Dictionary&lt;string, PatternWeight&gt;();
            activePredictions = new List&lt;MarketPrediction&gt;();
            predictionHistory = new List&lt;PredictionResult&gt;();
            recentMarketData = new Queue&lt;MarketDataPoint&gt;();
            
            InitializeNeuralNetwork();
        }
        
        void Start()
        {
            InvokeRepeating(nameof(GeneratePredictions), 5f, predictionTimeHorizon);
            InvokeRepeating(nameof(EvaluatePredictions), predictionTimeHorizon, predictionTimeHorizon);
        }
        
        private void InitializeNeuralNetwork()
        {
            inputLayer = new float[INPUT_SIZE];
            hiddenLayer = new float[HIDDEN_SIZE];
            outputLayer = new float[OUTPUT_SIZE];
            
            inputToHidden = new float[INPUT_SIZE, HIDDEN_SIZE];
            hiddenToOutput = new float[HIDDEN_SIZE, OUTPUT_SIZE];
            
            // Initialize weights randomly
            for (int i = 0; i &lt; INPUT_SIZE; i++)
            {
                for (int j = 0; j &lt; HIDDEN_SIZE; j++)
                {
                    inputToHidden[i, j] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
            }
            
            for (int i = 0; i &lt; HIDDEN_SIZE; i++)
            {
                for (int j = 0; j &lt; OUTPUT_SIZE; j++)
                {
                    hiddenToOutput[i, j] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
            }
        }
        
        public void ProcessMarketUpdate(float price, float volume, MarketDirection actualDirection)
        {
            var dataPoint = new MarketDataPoint(Time.time, price, volume, actualDirection);
            recentMarketData.Enqueue(dataPoint);
            
            // Keep recent data manageable
            if (recentMarketData.Count &gt; 100)
                recentMarketData.Dequeue();
            
            // Update neural network with new data
            UpdateNeuralNetwork(dataPoint);
        }
        
        private void GeneratePredictions()
        {
            try
            {
                var patterns = patternRecognition.GetDiscoveredPatterns();
                var behaviorMetrics = behaviorAnalyzer.GetCurrentMetrics();
                
                if (patterns.Count &lt; minPatternOccurrences || recentMarketData.Count &lt; 10)
                    return;
                
                // Generate ensemble predictions
                var ensemblePredictions = GenerateEnsemblePredictions(patterns, behaviorMetrics);
                
                foreach (var prediction in ensemblePredictions)
                {
                    if (prediction.confidence &gt; minPredictionConfidence)
                    {
                        activePredictions.Add(prediction);
                        Debug.Log($"FMTS Prediction: {prediction.predictedDirection} with {prediction.confidence:F2} confidence - {prediction.reasoning}");
                    }
                }
                
                // Clean up old predictions
                CleanupOldPredictions();
                
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating predictions: {e.Message}");
            }
        }
        
        private List&lt;MarketPrediction&gt; GenerateEnsemblePredictions(List&lt;FractalPattern&gt; patterns, MarketBehaviorMetrics metrics)
        {
            var predictions = new List&lt;MarketPrediction&gt;();
            
            // Method 1: Pattern-based prediction
            var patternPrediction = GeneratePatternBasedPrediction(patterns);
            if (patternPrediction != null) predictions.Add(patternPrediction);
            
            // Method 2: Trend-based prediction
            var trendPrediction = GenerateTrendBasedPrediction(metrics);
            if (trendPrediction != null) predictions.Add(trendPrediction);
            
            // Method 3: Neural network prediction
            var neuralPrediction = GenerateNeuralNetworkPrediction();
            if (neuralPrediction != null) predictions.Add(neuralPrediction);
            
            // Method 4: Fractal dimension prediction
            var fractalPrediction = GenerateFractalDimensionPrediction(patterns);
            if (fractalPrediction != null) predictions.Add(fractalPrediction);
            
            // Method 5: Volume-weighted prediction
            var volumePrediction = GenerateVolumeWeightedPrediction();
            if (volumePrediction != null) predictions.Add(volumePrediction);
            
            // Create consensus prediction if enough agreement
            var consensusPrediction = CreateConsensusPrediction(predictions);
            if (consensusPrediction != null) predictions.Add(consensusPrediction);
            
            return predictions;
        }
        
        private MarketPrediction GeneratePatternBasedPrediction(List&lt;FractalPattern&gt; patterns)
        {
            var recentData = recentMarketData.TakeLast(30).ToList();
            if (recentData.Count &lt; 10) return null;
            
            var currentPattern = ExtractCurrentPattern(recentData);
            var bestMatch = FindBestMatchingPattern(currentPattern, patterns);
            
            if (bestMatch == null || bestMatch.occurrenceCount &lt; minPatternOccurrences) return null;
            
            var prediction = new MarketPrediction
            {
                predictedDirection = bestMatch.dominantDirection,
                confidence = bestMatch.confidence * GetPatternWeight(bestMatch.patternId).weight,
                expectedMagnitude = CalculateExpectedMagnitude(bestMatch),
                timeHorizon = predictionTimeHorizon,
                reasoning = $"Pattern match: {bestMatch.patternId} ({bestMatch.occurrenceCount} occurrences, {bestMatch.selfSimilarityIndex:F2} similarity)",
            };
            
            prediction.supportingPatternIds.Add(bestMatch.patternId);
            return prediction;
        }
        
        private MarketPrediction GenerateTrendBasedPrediction(MarketBehaviorMetrics metrics)
        {
            var trends = behaviorAnalyzer.GetRecentTrends(10);
            if (trends.Count == 0) return null;
            
            var dominantDirection = DetermineDominantTrendDirection(trends);
            var trendStrength = CalculateAverageTrendStrength(trends);
            var confidence = Math.Min(trendStrength * metrics.trendPersistence, 1f);
            
            if (confidence &lt; 0.3f) return null;
            
            return new MarketPrediction
            {
                predictedDirection = dominantDirection,
                confidence = confidence,
                expectedMagnitude = trendStrength,
                timeHorizon = predictionTimeHorizon,
                reasoning = $"Trend analysis: {metrics.trendPersistence:F2} persistence, {trends.Count} recent trends"
            };
        }
        
        private MarketPrediction GenerateNeuralNetworkPrediction()
        {
            if (recentMarketData.Count &lt; INPUT_SIZE) return null;
            
            PrepareInputLayer();
            ForwardPass();
            
            // Find the direction with highest output
            int maxIndex = 0;
            for (int i = 1; i &lt; OUTPUT_SIZE; i++)
            {
                if (outputLayer[i] &gt; outputLayer[maxIndex])
                    maxIndex = i;
            }
            
            MarketDirection direction = (MarketDirection)maxIndex;
            float confidence = outputLayer[maxIndex];
            
            if (confidence &lt; 0.4f) return null;
            
            return new MarketPrediction
            {
                predictedDirection = direction,
                confidence = confidence,
                expectedMagnitude = confidence * 0.1f,
                timeHorizon = predictionTimeHorizon,
                reasoning = $"Neural network prediction: {confidence:F2} activation"
            };
        }
        
        private MarketPrediction GenerateFractalDimensionPrediction(List&lt;FractalPattern&gt; patterns)
        {
            if (patterns.Count == 0) return null;
            
            var highComplexityPatterns = patterns
                .Where(p =&gt; p.selfSimilarityIndex &gt; 0.6f)
                .OrderByDescending(p =&gt; p.confidence)
                .Take(5)
                .ToList();
            
            if (highComplexityPatterns.Count == 0) return null;
            
            var avgDirection = CalculateAverageDirection(highComplexityPatterns);
            var avgComplexity = highComplexityPatterns.Average(p =&gt; p.selfSimilarityIndex);
            var confidence = Math.Min(avgComplexity * 0.8f, 0.9f);
            
            return new MarketPrediction
            {
                predictedDirection = avgDirection,
                confidence = confidence,
                expectedMagnitude = avgComplexity * 0.15f,
                timeHorizon = predictionTimeHorizon,
                reasoning = $"Fractal dimension analysis: {avgComplexity:F2} complexity, {highComplexityPatterns.Count} patterns"
            };
        }
        
        private MarketPrediction GenerateVolumeWeightedPrediction()
        {
            var recentData = recentMarketData.TakeLast(20).ToList();
            if (recentData.Count &lt; 10) return null;
            
            float totalVolumeUp = 0f, totalVolumeDown = 0f;
            
            foreach (var data in recentData)
            {
                if (data.direction == MarketDirection.Up)
                    totalVolumeUp += data.volume;
                else if (data.direction == MarketDirection.Down)
                    totalVolumeDown += data.volume;
            }
            
            float totalVolume = totalVolumeUp + totalVolumeDown;
            if (totalVolume == 0) return null;
            
            MarketDirection direction = totalVolumeUp &gt; totalVolumeDown ? MarketDirection.Up : MarketDirection.Down;
            float confidence = Math.Abs(totalVolumeUp - totalVolumeDown) / totalVolume;
            
            if (confidence &lt; 0.3f) return null;
            
            return new MarketPrediction
            {
                predictedDirection = direction,
                confidence = confidence,
                expectedMagnitude = confidence * 0.08f,
                timeHorizon = predictionTimeHorizon,
                reasoning = $"Volume analysis: {totalVolumeUp:F0} up vs {totalVolumeDown:F0} down"
            };
        }
        
        private MarketPrediction CreateConsensusPrediction(List&lt;MarketPrediction&gt; predictions)
        {
            if (predictions.Count &lt; 2) return null;
            
            var directionVotes = new Dictionary&lt;MarketDirection, float&gt;();
            
            foreach (var pred in predictions)
            {
                if (!directionVotes.ContainsKey(pred.predictedDirection))
                    directionVotes[pred.predictedDirection] = 0f;
                
                directionVotes[pred.predictedDirection] += pred.confidence;
            }
            
            var winningDirection = directionVotes.OrderByDescending(kv =&gt; kv.Value).First();
            float totalConfidence = directionVotes.Values.Sum();
            float consensusStrength = winningDirection.Value / totalConfidence;
            
            if (consensusStrength &lt; consensusThreshold) return null;
            
            var consensusPrediction = new MarketPrediction
            {
                predictedDirection = winningDirection.Key,
                confidence = Math.Min(consensusStrength, 0.95f),
                expectedMagnitude = predictions.Average(p =&gt; p.expectedMagnitude),
                timeHorizon = predictionTimeHorizon,
                reasoning = $"Ensemble consensus: {predictions.Count} methods, {consensusStrength:F2} agreement"
            };
            
            // Combine supporting pattern IDs
            foreach (var pred in predictions)
            {
                consensusPrediction.supportingPatternIds.AddRange(pred.supportingPatternIds);
            }
            
            return consensusPrediction;
        }
        
        private void EvaluatePredictions()
        {
            var currentData = recentMarketData.LastOrDefault();
            if (currentData == null) return;
            
            var predictionsToEvaluate = activePredictions
                .Where(p =&gt; (DateTime.Now - p.predictionTime).TotalSeconds &gt;= p.timeHorizon)
                .ToList();
            
            foreach (var prediction in predictionsToEvaluate)
            {
                var result = EvaluatePrediction(prediction, currentData);
                predictionHistory.Add(result);
                
                // Update pattern weights based on results
                UpdatePatternWeights(result);
                
                // Update neural network
                if (result.wasCorrect)
                    ReinforcePrediction(prediction);
                else
                    PenalizePrediction(prediction);
            }
            
            // Remove evaluated predictions
            activePredictions.RemoveAll(p =&gt; predictionsToEvaluate.Contains(p));
            
            // Keep prediction history manageable
            if (predictionHistory.Count &gt; 1000)
                predictionHistory = predictionHistory.TakeLast(500).ToList();
        }
        
        private PredictionResult EvaluatePrediction(MarketPrediction prediction, MarketDataPoint actualData)
        {
            var result = new PredictionResult(prediction)
            {
                actualDirection = actualData.direction,
                actualMagnitude = CalculateActualMagnitude(actualData)
            };
            
            result.wasCorrect = result.actualDirection == prediction.predictedDirection;
            
            // Calculate accuracy score based on direction correctness and magnitude
            if (result.wasCorrect)
            {
                float magnitudeAccuracy = 1f - Math.Abs(prediction.expectedMagnitude - result.actualMagnitude) / Math.Max(prediction.expectedMagnitude, result.actualMagnitude);
                result.accuracyScore = 0.7f + 0.3f * magnitudeAccuracy; // 70% direction, 30% magnitude
            }
            else
            {
                result.accuracyScore = 0f;
            }
            
            return result;
        }
        
        private void UpdatePatternWeights(PredictionResult result)
        {
            foreach (var patternId in result.prediction.supportingPatternIds)
            {
                var weight = GetPatternWeight(patternId);
                weight.totalPredictions++;
                
                if (result.wasCorrect)
                {
                    weight.correctPredictions++;
                    weight.weight = Mathf.Min(weight.weight + learningRate * result.accuracyScore, 1f);
                }
                else
                {
                    weight.weight = Mathf.Max(weight.weight - learningRate * 0.5f, 0.1f);
                }
                
                weight.successRate = (float)weight.correctPredictions / weight.totalPredictions;
                weight.lastUpdate = DateTime.Now;
            }
            
            // Clean up unused patterns
            if (patternWeights.Count &gt; maxPatternWeights)
            {
                var oldWeights = patternWeights.Values
                    .OrderBy(w =&gt; w.lastUpdate)
                    .Take(patternWeights.Count - maxPatternWeights)
                    .ToList();
                
                foreach (var weight in oldWeights)
                {
                    patternWeights.Remove(weight.patternId);
                }
            }
        }
        
        // Additional helper methods...
        private PatternWeight GetPatternWeight(string patternId)
        {
            if (!patternWeights.ContainsKey(patternId))
                patternWeights[patternId] = new PatternWeight(patternId);
            return patternWeights[patternId];
        }
        
        private List&lt;Vector2&gt; ExtractCurrentPattern(List&lt;MarketDataPoint&gt; data)
        {
            // Simple extraction - convert to normalized points
            var points = new List&lt;Vector2&gt;();
            if (data.Count == 0) return points;
            
            float minPrice = data.Min(d =&gt; d.price);
            float maxPrice = data.Max(d =&gt; d.price);
            float priceRange = maxPrice - minPrice;
            if (priceRange == 0) priceRange = 1;
            
            for (int i = 0; i &lt; data.Count; i++)
            {
                float normalizedTime = (float)i / (data.Count - 1);
                float normalizedPrice = (data[i].price - minPrice) / priceRange;
                points.Add(new Vector2(normalizedTime, normalizedPrice));
            }
            
            return points;
        }
        
        private FractalPattern FindBestMatchingPattern(List&lt;Vector2&gt; currentPattern, List&lt;FractalPattern&gt; patterns)
        {
            FractalPattern bestMatch = null;
            float bestSimilarity = 0f;
            
            foreach (var pattern in patterns)
            {
                float similarity = CalculatePatternSimilarity(currentPattern, pattern.normalizedPoints);
                if (similarity &gt; patternSimilarityThreshold && similarity &gt; bestSimilarity)
                {
                    bestMatch = pattern;
                    bestSimilarity = similarity;
                }
            }
            
            return bestMatch;
        }
        
        private float CalculatePatternSimilarity(List&lt;Vector2&gt; pattern1, List&lt;Vector2&gt; pattern2)
        {
            if (pattern1.Count == 0 || pattern2.Count == 0) return 0f;
            
            // Simplified DTW similarity calculation
            int minLength = Math.Min(pattern1.Count, pattern2.Count);
            float totalDistance = 0f;
            
            for (int i = 0; i &lt; minLength; i++)
            {
                float t1 = (float)i / (pattern1.Count - 1);
                float t2 = (float)i / (pattern2.Count - 1);
                
                int index1 = Mathf.RoundToInt(t1 * (pattern1.Count - 1));
                int index2 = Mathf.RoundToInt(t2 * (pattern2.Count - 1));
                
                totalDistance += Vector2.Distance(pattern1[index1], pattern2[index2]);
            }
            
            return Mathf.Exp(-totalDistance / minLength);
        }
        
        // More helper methods for neural network, calculations, etc...
        private void PrepareInputLayer()
        {
            var data = recentMarketData.TakeLast(INPUT_SIZE).ToArray();
            for (int i = 0; i &lt; INPUT_SIZE && i &lt; data.Length; i++)
            {
                inputLayer[i] = data[i].price / 100f; // Normalize price
            }
        }
        
        private void ForwardPass()
        {
            // Input to hidden
            for (int j = 0; j &lt; HIDDEN_SIZE; j++)
            {
                hiddenLayer[j] = 0f;
                for (int i = 0; i &lt; INPUT_SIZE; i++)
                {
                    hiddenLayer[j] += inputLayer[i] * inputToHidden[i, j];
                }
                hiddenLayer[j] = Mathf.Tanh(hiddenLayer[j]); // Activation function
            }
            
            // Hidden to output
            for (int k = 0; k &lt; OUTPUT_SIZE; k++)
            {
                outputLayer[k] = 0f;
                for (int j = 0; j &lt; HIDDEN_SIZE; j++)
                {
                    outputLayer[k] += hiddenLayer[j] * hiddenToOutput[j, k];
                }
                outputLayer[k] = 1f / (1f + Mathf.Exp(-outputLayer[k])); // Sigmoid activation
            }
        }
        
        private void ReinforcePrediction(MarketPrediction prediction)
        {
            // Simple reinforcement learning - increase weights that led to correct predictions
            // This is a simplified version - a full implementation would use backpropagation
        }
        
        private void PenalizePrediction(MarketPrediction prediction)
        {
            // Decrease weights that led to incorrect predictions
        }
        
        private void UpdateNeuralNetwork(MarketDataPoint newData)
        {
            // Update network with new data point
        }
        
        private float CalculateExpectedMagnitude(FractalPattern pattern)
        {
            return pattern.selfSimilarityIndex * 0.1f; // Simple magnitude estimation
        }
        
        private float CalculateActualMagnitude(MarketDataPoint data)
        {
            return data.volume * 0.001f; // Simple magnitude calculation
        }
        
        private MarketDirection DetermineDominantTrendDirection(List&lt;MarketTrend&gt; trends)
        {
            var directionCounts = trends.GroupBy(t =&gt; t.direction)
                .ToDictionary(g =&gt; g.Key, g =&gt; g.Count());
            
            return directionCounts.OrderByDescending(kv =&gt; kv.Value).First().Key;
        }
        
        private float CalculateAverageTrendStrength(List&lt;MarketTrend&gt; trends)
        {
            return trends.Average(t =&gt; t.strength * t.confidence);
        }
        
        private MarketDirection CalculateAverageDirection(List&lt;FractalPattern&gt; patterns)
        {
            var directionCounts = patterns.GroupBy(p =&gt; p.dominantDirection)
                .ToDictionary(g =&gt; g.Key, g =&gt; g.Sum(p =&gt; p.confidence));
            
            return directionCounts.OrderByDescending(kv =&gt; kv.Value).First().Key;
        }
        
        private void CleanupOldPredictions()
        {
            var cutoffTime = DateTime.Now.AddSeconds(-predictionTimeHorizon * 2);
            activePredictions.RemoveAll(p =&gt; p.predictionTime &lt; cutoffTime);
        }
        
        public List&lt;MarketPrediction&gt; GetActivePredictions()
        {
            return new List&lt;MarketPrediction&gt;(activePredictions);
        }
        
        public float GetOverallPredictionAccuracy()
        {
            if (predictionHistory.Count == 0) return 0f;
            
            var recentResults = predictionHistory.TakeLast(100);
            return recentResults.Average(r =&gt; r.accuracyScore);
        }
        
        public Dictionary&lt;string, object&gt; GetPredictionStats()
        {
            var stats = new Dictionary&lt;string, object&gt;
            {
                ["ActivePredictions"] = activePredictions.Count,
                ["TotalPredictionsMade"] = predictionHistory.Count,
                ["OverallAccuracy"] = GetOverallPredictionAccuracy(),
                ["PatternWeights"] = patternWeights.Count,
                ["AverageConfidence"] = activePredictions.Count &gt; 0 ? activePredictions.Average(p =&gt; p.confidence) : 0f
            };
            
            return stats;
        }
    }
}
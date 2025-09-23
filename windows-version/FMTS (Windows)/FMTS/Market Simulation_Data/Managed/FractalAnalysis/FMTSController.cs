using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FMTS.FractalAnalysis
{
    /// &lt;summary&gt;
    /// Main controller for the Fractal Mathematics in Trading Systems (FMTS) application.
    /// Coordinates all fractal analysis systems and provides the main interface.
    /// &lt;/summary&gt;
    public class FMTSController : MonoBehaviour
    {
        [Header("Market Simulation Settings")]
        public bool enableAutoSimulation = true;
        public float simulationSpeed = 1.0f;
        public float basePrice = 100.0f;
        public float volatility = 0.02f;
        public float trendStrength = 0.1f;
        
        [Header("UI References")]
        public Text statusDisplay;
        public Text predictionDisplay;
        public Text metricsDisplay;
        public Button exportButton;
        public Button newSessionButton;
        
        [Header("System References")]
        public FractalPatternRecognition patternRecognition;
        public MarketBehaviorAnalyzer behaviorAnalyzer;
        public PredictiveInsightEngine predictionEngine;
        public SessionDataExporter dataExporter;
        
        // Market simulation state
        private float currentPrice;
        private float currentVolume;
        private float timeAccumulator;
        private int tickCounter;
        private System.Random random;
        
        // Performance monitoring
        private float lastUpdateTime;
        private int updatesPerSecond;
        private float performanceTimer;
        
        void Awake()
        {
            // Initialize components if not assigned
            if (patternRecognition == null)
                patternRecognition = GetComponent&lt;FractalPatternRecognition&gt;() ?? gameObject.AddComponent&lt;FractalPatternRecognition&gt;();
            
            if (behaviorAnalyzer == null)
                behaviorAnalyzer = GetComponent&lt;MarketBehaviorAnalyzer&gt;() ?? gameObject.AddComponent&lt;MarketBehaviorAnalyzer&gt;();
            
            if (predictionEngine == null)
                predictionEngine = GetComponent&lt;PredictiveInsightEngine&gt;() ?? gameObject.AddComponent&lt;PredictiveInsightEngine&gt;();
            
            if (dataExporter == null)
                dataExporter = GetComponent&lt;SessionDataExporter&gt;() ?? gameObject.AddComponent&lt;SessionDataExporter&gt;();
            
            // Initialize simulation
            currentPrice = basePrice;
            currentVolume = 1000f;
            random = new System.Random();
            
            Debug.Log("FMTS: Fractal Mathematics Trading System initialized");
        }
        
        void Start()
        {
            // Setup UI if available
            if (exportButton != null)
                exportButton.onClick.AddListener(() =&gt; dataExporter.ForceExport());
            
            if (newSessionButton != null)
                newSessionButton.onClick.AddListener(() =&gt; StartNewSession());
            
            // Start market simulation
            if (enableAutoSimulation)
            {
                StartCoroutine(MarketSimulationLoop());
            }
            
            // Start UI update loop
            StartCoroutine(UpdateUI());
            
            Debug.Log("FMTS: System started successfully");
        }
        
        /// &lt;summary&gt;
        /// Main market simulation loop that generates realistic market data
        /// &lt;/summary&gt;
        IEnumerator MarketSimulationLoop()
        {
            while (true)
            {
                // Generate next market tick
                GenerateMarketTick();
                
                // Process the data through all analysis systems
                ProcessMarketData();
                
                // Update performance metrics
                UpdatePerformanceMetrics();
                
                // Wait for next tick based on simulation speed
                yield return new WaitForSeconds(1f / simulationSpeed);
                
                tickCounter++;
            }
        }
        
        /// &lt;summary&gt;
        /// Generates realistic market data using fractal-inspired algorithms
        /// &lt;/summary&gt;
        private void GenerateMarketTick()
        {
            // Use Geometric Brownian Motion with fractal components
            float dt = Time.deltaTime;
            float randomWalk = (float)(random.NextDouble() - 0.5) * 2f;
            
            // Add trend component
            float trendComponent = Mathf.Sin(Time.time * 0.1f) * trendStrength;
            
            // Add volatility clustering (fractal property)
            float volatilityCluster = Mathf.Abs(Mathf.Sin(Time.time * 0.05f)) * volatility;
            
            // Add self-similar noise at multiple scales
            float fractalNoise = GenerateFractalNoise(Time.time);
            
            // Calculate price change
            float priceChange = currentPrice * (
                trendComponent * dt +
                volatilityCluster * randomWalk * Mathf.Sqrt(dt) +
                fractalNoise * dt
            );
            
            currentPrice += priceChange;
            currentPrice = Mathf.Max(currentPrice, 0.01f); // Prevent negative prices
            
            // Generate volume with realistic patterns
            float baseVolume = 1000f + Mathf.Sin(Time.time * 0.2f) * 500f;
            float volumeVariability = (float)(random.NextDouble() * 0.5f + 0.75f);
            currentVolume = baseVolume * volumeVariability;
            
            // Add volume spikes during high volatility
            if (Mathf.Abs(priceChange / currentPrice) &gt; volatility * 2f)
            {
                currentVolume *= 1.5f + (float)random.NextDouble();
            }
        }
        
        /// &lt;summary&gt;
        /// Generates fractal noise at multiple scales for realistic market behavior
        /// &lt;/summary&gt;
        private float GenerateFractalNoise(float time)
        {
            float noise = 0f;
            float amplitude = 0.01f;
            float frequency = 1f;
            
            // Sum noise at different scales (fractal property)
            for (int i = 0; i &lt; 4; i++)
            {
                noise += amplitude * Mathf.PerlinNoise(time * frequency, tickCounter * 0.001f);
                amplitude *= 0.5f;
                frequency *= 2f;
            }
            
            return noise;
        }
        
        /// &lt;summary&gt;
        /// Processes market data through all analysis systems
        /// &lt;/summary&gt;
        private void ProcessMarketData()
        {
            // Determine market direction
            MarketDirection direction = DetermineMarketDirection();
            
            // Process through behavior analyzer (which also feeds the pattern recognition)
            behaviorAnalyzer.AnalyzeMarketBehavior(currentPrice, currentVolume);
            
            // Process through prediction engine
            predictionEngine.ProcessMarketUpdate(currentPrice, currentVolume, direction);
            
            // Add to data exporter
            var dataPoint = new MarketDataPoint(Time.time, currentPrice, currentVolume, direction);
            dataExporter.AddMarketDataPoint(dataPoint);
        }
        
        /// &lt;summary&gt;
        /// Determines market direction based on recent price movement
        /// &lt;/summary&gt;
        private MarketDirection DetermineMarketDirection()
        {
            // Simple direction determination based on recent price change
            if (tickCounter &lt; 2) return MarketDirection.Neutral;
            
            // Look at price change over last few ticks
            // This is simplified - in reality you'd use more sophisticated analysis
            float recentChange = currentPrice - basePrice;
            float changeThreshold = basePrice * volatility * 0.5f;
            
            if (recentChange &gt; changeThreshold)
                return MarketDirection.Up;
            else if (recentChange &lt; -changeThreshold)
                return MarketDirection.Down;
            else
                return MarketDirection.Neutral;
        }
        
        /// &lt;summary&gt;
        /// Updates performance metrics for monitoring
        /// &lt;/summary&gt;
        private void UpdatePerformanceMetrics()
        {
            performanceTimer += Time.deltaTime;
            updatesPerSecond++;
            
            if (performanceTimer &gt;= 1f)
            {
                // Reset counters
                performanceTimer = 0f;
                updatesPerSecond = 0;
            }
        }
        
        /// &lt;summary&gt;
        /// Updates UI display with current system status
        /// &lt;/summary&gt;
        IEnumerator UpdateUI()
        {
            while (true)
            {
                if (statusDisplay != null)
                {
                    UpdateStatusDisplay();
                }
                
                if (predictionDisplay != null)
                {
                    UpdatePredictionDisplay();
                }
                
                if (metricsDisplay != null)
                {
                    UpdateMetricsDisplay();
                }
                
                yield return new WaitForSeconds(1f); // Update UI every second
            }
        }
        
        private void UpdateStatusDisplay()
        {
            var sessionStats = dataExporter.GetCurrentSession();
            var patterns = patternRecognition.GetDiscoveredPatterns();
            
            statusDisplay.text = $"FMTS Status\n" +
                               $"Session: {sessionStats.sessionId}\n" +
                               $"Price: ${currentPrice:F2}\n" +
                               $"Volume: {currentVolume:F0}\n" +
                               $"Ticks: {tickCounter:N0}\n" +
                               $"Patterns: {patterns.Count}\n" +
                               $"UPS: {updatesPerSecond}";
        }
        
        private void UpdatePredictionDisplay()
        {
            var predictions = predictionEngine.GetActivePredictions();
            var accuracy = predictionEngine.GetOverallPredictionAccuracy();
            
            string predText = $"Predictions ({accuracy:P1} accuracy)\n";
            
            if (predictions.Count == 0)
            {
                predText += "No active predictions";
            }
            else
            {
                foreach (var pred in predictions.Take(3)) // Show top 3
                {
                    predText += $"• {pred.predictedDirection} ({pred.confidence:P0})\n";
                    predText += $"  {pred.reasoning}\n";
                }
            }
            
            predictionDisplay.text = predText;
        }
        
        private void UpdateMetricsDisplay()
        {
            var metrics = behaviorAnalyzer.GetCurrentMetrics();
            var directionRatios = patternRecognition.GetDirectionRatios();
            
            metricsDisplay.text = $"Market Metrics\n" +
                                $"Up: {directionRatios[MarketDirection.Up]:P1}\n" +
                                $"Down: {directionRatios[MarketDirection.Down]:P1}\n" +
                                $"Neutral: {directionRatios[MarketDirection.Neutral]:P1}\n" +
                                $"Volatility: {metrics.averageVolatility:F4}\n" +
                                $"Complexity: {metrics.fractalComplexity:F3}\n" +
                                $"Persistence: {metrics.trendPersistence:F3}";
        }
        
        /// &lt;summary&gt;
        /// Starts a new trading session
        /// &lt;/summary&gt;
        public void StartNewSession()
        {
            Debug.Log("FMTS: Starting new session");
            
            // Reset simulation state
            currentPrice = basePrice;
            currentVolume = 1000f;
            tickCounter = 0;
            
            // Start new session in data exporter
            dataExporter.StartNewSession();
            
            Debug.Log("FMTS: New session started");
        }
        
        /// &lt;summary&gt;
        /// Manually adds market data (for external data feeds)
        /// &lt;/summary&gt;
        public void AddMarketData(float price, float volume, MarketDirection direction)
        {
            currentPrice = price;
            currentVolume = volume;
            ProcessMarketData();
        }
        
        /// &lt;summary&gt;
        /// Gets comprehensive system statistics
        /// &lt;/summary&gt;
        public Dictionary&lt;string, object&gt; GetSystemStats()
        {
            var stats = new Dictionary&lt;string, object&gt;();
            
            // Add stats from all subsystems
            var sessionStats = dataExporter.GetCurrentSession();
            var predictionStats = predictionEngine.GetPredictionStats();
            var analyticsStats = behaviorAnalyzer.GetAnalyticsSummary();
            var exportStats = dataExporter.GetExportStats();
            
            stats["SessionInfo"] = sessionStats;
            stats["PredictionInfo"] = predictionStats;
            stats["AnalyticsInfo"] = analyticsStats;
            stats["ExportInfo"] = exportStats;
            stats["SimulationInfo"] = new Dictionary&lt;string, object&gt;
            {
                ["CurrentPrice"] = currentPrice,
                ["CurrentVolume"] = currentVolume,
                ["TickCount"] = tickCounter,
                ["UpdatesPerSecond"] = updatesPerSecond,
                ["SimulationSpeed"] = simulationSpeed
            };
            
            return stats;
        }
        
        /// &lt;summary&gt;
        /// Exports current system state and data
        /// &lt;/summary&gt;
        public void ExportSystemData()
        {
            dataExporter.ForceExport();
        }
        
        /// &lt;summary&gt;
        /// Pauses or resumes the market simulation
        /// &lt;/summary&gt;
        public void SetSimulationPaused(bool paused)
        {
            enableAutoSimulation = !paused;
            Debug.Log($"FMTS: Simulation {(paused ? "paused" : "resumed")}");
        }
        
        /// &lt;summary&gt;
        /// Sets the simulation speed multiplier
        /// &lt;/summary&gt;
        public void SetSimulationSpeed(float speed)
        {
            simulationSpeed = Mathf.Clamp(speed, 0.1f, 10f);
            Debug.Log($"FMTS: Simulation speed set to {simulationSpeed}x");
        }
        
        /// &lt;summary&gt;
        /// Gets the most confident fractal pattern found
        /// &lt;/summary&gt;
        public FractalPattern GetMostConfidentPattern()
        {
            return patternRecognition.GetMostConfidentPattern();
        }
        
        /// &lt;summary&gt;
        /// Gets recent market trends
        /// &lt;/summary&gt;
        public List&lt;MarketTrend&gt; GetRecentTrends()
        {
            return behaviorAnalyzer.GetRecentTrends();
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("FMTS: Application paused, exporting data");
                dataExporter.ExportSessionData();
            }
        }
        
        void OnDestroy()
        {
            Debug.Log("FMTS: System shutting down");
        }
    }
}
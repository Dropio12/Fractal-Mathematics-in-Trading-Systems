using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FMTS.FractalAnalysis
{
    [System.Serializable]
    public class SessionSummary
    {
        public DateTime sessionStart;
        public DateTime sessionEnd;
        public int totalDataPoints;
        public int totalPatternsFound;
        public int totalPredictionsMade;
        public float overallPredictionAccuracy;
        public float averageVolatility;
        public float upTrendRatio;
        public float downTrendRatio;
        public float neutralTrendRatio;
        public float fractalComplexity;
        public string sessionId;
        
        public SessionSummary()
        {
            sessionStart = DateTime.Now;
            sessionId = Guid.NewGuid().ToString("N")[..8];
        }
    }
    
    public class SessionDataExporter : MonoBehaviour
    {
        [Header("Export Configuration")]
        public bool autoExportOnSessionEnd = true;
        public bool exportRealTime = false;
        public float realTimeExportInterval = 300f; // 5 minutes
        public int maxRowsPerFile = 10000;
        
        [Header("Export Settings")]
        public string exportDirectory = "FMTS_Data";
        public bool includePatternDetails = true;
        public bool includePredictionHistory = true;
        public bool includeMarketData = true;
        
        private FractalPatternRecognition patternRecognition;
        private MarketBehaviorAnalyzer behaviorAnalyzer;
        private PredictiveInsightEngine predictionEngine;
        
        private SessionSummary currentSession;
        private List&lt;MarketDataPoint&gt; sessionMarketData;
        private List&lt;string&gt; exportLog;
        private float lastExportTime;
        
        private string exportPath;
        
        void Awake()
        {
            patternRecognition = GetComponent&lt;FractalPatternRecognition&gt;();
            behaviorAnalyzer = GetComponent&lt;MarketBehaviorAnalyzer&gt;();
            predictionEngine = GetComponent&lt;PredictiveInsightEngine&gt;();
            
            sessionMarketData = new List&lt;MarketDataPoint&gt;();
            exportLog = new List&lt;string&gt;();
            currentSession = new SessionSummary();
            
            // Setup export directory
            SetupExportDirectory();
            
            lastExportTime = Time.time;
        }
        
        void Start()
        {
            if (exportRealTime)
            {
                InvokeRepeating(nameof(ExportSessionData), realTimeExportInterval, realTimeExportInterval);
            }
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && autoExportOnSessionEnd)
            {
                ExportSessionData();
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && autoExportOnSessionEnd)
            {
                ExportSessionData();
            }
        }
        
        void OnDestroy()
        {
            if (autoExportOnSessionEnd)
            {
                ExportSessionData();
            }
        }
        
        private void SetupExportDirectory()
        {
            try
            {
                exportPath = Path.Combine(Application.persistentDataPath, exportDirectory);
                
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                    Debug.Log($"FMTS: Created export directory at {exportPath}");
                }
                
                // Create subdirectories
                Directory.CreateDirectory(Path.Combine(exportPath, "Sessions"));
                Directory.CreateDirectory(Path.Combine(exportPath, "Patterns"));
                Directory.CreateDirectory(Path.Combine(exportPath, "Predictions"));
                Directory.CreateDirectory(Path.Combine(exportPath, "MarketData"));
                
            }
            catch (Exception e)
            {
                Debug.LogError($"FMTS: Failed to setup export directory: {e.Message}");
                exportPath = Application.persistentDataPath; // Fallback
            }
        }
        
        public void AddMarketDataPoint(MarketDataPoint dataPoint)
        {
            sessionMarketData.Add(dataPoint);
            
            // Update session summary
            currentSession.totalDataPoints = sessionMarketData.Count;
        }
        
        public void ExportSessionData()
        {
            try
            {
                UpdateSessionSummary();
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string sessionFolder = Path.Combine(exportPath, "Sessions", $"Session_{currentSession.sessionId}_{timestamp}");
                
                Directory.CreateDirectory(sessionFolder);
                
                // Export session summary
                ExportSessionSummary(sessionFolder);
                
                // Export market data
                if (includeMarketData)
                {
                    ExportMarketData(sessionFolder);
                }
                
                // Export patterns
                if (includePatternDetails)
                {
                    ExportPatternData(sessionFolder);
                }
                
                // Export predictions
                if (includePredictionHistory)
                {
                    ExportPredictionData(sessionFolder);
                }
                
                // Export behavioral metrics
                ExportBehaviorMetrics(sessionFolder);
                
                // Create export manifest
                CreateExportManifest(sessionFolder);
                
                AddToExportLog($"Session data exported successfully to {sessionFolder}");
                Debug.Log($"FMTS: Session data exported to {sessionFolder}");
                
            }
            catch (Exception e)
            {
                Debug.LogError($"FMTS: Failed to export session data: {e.Message}");
                AddToExportLog($"Export failed: {e.Message}");
            }
        }
        
        private void UpdateSessionSummary()
        {
            currentSession.sessionEnd = DateTime.Now;
            
            if (patternRecognition != null)
            {
                var patterns = patternRecognition.GetDiscoveredPatterns();
                currentSession.totalPatternsFound = patterns.Count;
                currentSession.fractalComplexity = patterns.Count &gt; 0 ? 
                    patterns.Average(p =&gt; p.selfSimilarityIndex) : 0f;
                
                var directionRatios = patternRecognition.GetDirectionRatios();
                currentSession.upTrendRatio = directionRatios[MarketDirection.Up];
                currentSession.downTrendRatio = directionRatios[MarketDirection.Down];
                currentSession.neutralTrendRatio = directionRatios[MarketDirection.Neutral];
            }
            
            if (behaviorAnalyzer != null)
            {
                var metrics = behaviorAnalyzer.GetCurrentMetrics();
                currentSession.averageVolatility = metrics.averageVolatility;
            }
            
            if (predictionEngine != null)
            {
                currentSession.overallPredictionAccuracy = predictionEngine.GetOverallPredictionAccuracy();
                // Get total predictions from prediction engine
                var stats = predictionEngine.GetPredictionStats();
                currentSession.totalPredictionsMade = (int)stats["TotalPredictionsMade"];
            }
        }
        
        private void ExportSessionSummary(string folder)
        {
            string filePath = Path.Combine(folder, "session_summary.csv");
            
            var csv = new StringBuilder();
            csv.AppendLine("SessionId,StartTime,EndTime,Duration,TotalDataPoints,TotalPatternsFound,TotalPredictions,PredictionAccuracy,AverageVolatility,UpRatio,DownRatio,NeutralRatio,FractalComplexity");
            
            var duration = (currentSession.sessionEnd - currentSession.sessionStart).TotalMinutes;
            
            csv.AppendLine($"{currentSession.sessionId}," +
                          $"{currentSession.sessionStart:yyyy-MM-dd HH:mm:ss}," +
                          $"{currentSession.sessionEnd:yyyy-MM-dd HH:mm:ss}," +
                          $"{duration:F2}," +
                          $"{currentSession.totalDataPoints}," +
                          $"{currentSession.totalPatternsFound}," +
                          $"{currentSession.totalPredictionsMade}," +
                          $"{currentSession.overallPredictionAccuracy:F4}," +
                          $"{currentSession.averageVolatility:F6}," +
                          $"{currentSession.upTrendRatio:F4}," +
                          $"{currentSession.downTrendRatio:F4}," +
                          $"{currentSession.neutralTrendRatio:F4}," +
                          $"{currentSession.fractalComplexity:F4}");
            
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private void ExportMarketData(string folder)
        {
            string filePath = Path.Combine(folder, "market_data.csv");
            
            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,Price,Volume,Direction,UnixTime,SessionId");
            
            foreach (var dataPoint in sessionMarketData)
            {
                var unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                csv.AppendLine($"{dataPoint.timestamp:F3}," +
                              $"{dataPoint.price:F6}," +
                              $"{dataPoint.volume:F3}," +
                              $"{dataPoint.direction}," +
                              $"{unixTime}," +
                              $"{currentSession.sessionId}");
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private void ExportPatternData(string folder)
        {
            if (patternRecognition == null) return;
            
            string filePath = Path.Combine(folder, "fractal_patterns.csv");
            var patterns = patternRecognition.GetDiscoveredPatterns();
            
            var csv = new StringBuilder();
            csv.AppendLine("PatternId,SelfSimilarityIndex,OccurrenceCount,Confidence,DominantDirection,PointCount,SessionId,DiscoveryTime");
            
            foreach (var pattern in patterns)
            {
                csv.AppendLine($"{pattern.patternId}," +
                              $"{pattern.selfSimilarityIndex:F6}," +
                              $"{pattern.occurrenceCount}," +
                              $"{pattern.confidence:F4}," +
                              $"{pattern.dominantDirection}," +
                              $"{pattern.normalizedPoints.Count}," +
                              $"{currentSession.sessionId}," +
                              $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
            
            File.WriteAllText(filePath, csv.ToString());
            
            // Export detailed pattern points if requested
            ExportPatternPoints(folder, patterns);
        }
        
        private void ExportPatternPoints(string folder, List&lt;FractalPattern&gt; patterns)
        {
            string filePath = Path.Combine(folder, "pattern_points.csv");
            
            var csv = new StringBuilder();
            csv.AppendLine("PatternId,PointIndex,NormalizedTime,NormalizedPrice,SessionId");
            
            foreach (var pattern in patterns.Take(100)) // Limit to first 100 patterns to avoid huge files
            {
                for (int i = 0; i &lt; pattern.normalizedPoints.Count; i++)
                {
                    var point = pattern.normalizedPoints[i];
                    csv.AppendLine($"{pattern.patternId}," +
                                  $"{i}," +
                                  $"{point.x:F6}," +
                                  $"{point.y:F6}," +
                                  $"{currentSession.sessionId}");
                }
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private void ExportPredictionData(string folder)
        {
            if (predictionEngine == null) return;
            
            // Note: This would need to be integrated with PredictiveInsightEngine to get historical predictions
            // For now, we'll export current active predictions
            string filePath = Path.Combine(folder, "predictions.csv");
            var predictions = predictionEngine.GetActivePredictions();
            
            var csv = new StringBuilder();
            csv.AppendLine("PredictionTime,PredictedDirection,Confidence,ExpectedMagnitude,TimeHorizon,Reasoning,SupportingPatterns,SessionId");
            
            foreach (var prediction in predictions)
            {
                var supportingPatterns = string.Join(";", prediction.supportingPatternIds);
                csv.AppendLine($"{prediction.predictionTime:yyyy-MM-dd HH:mm:ss}," +
                              $"{prediction.predictedDirection}," +
                              $"{prediction.confidence:F4}," +
                              $"{prediction.expectedMagnitude:F6}," +
                              $"{prediction.timeHorizon:F1}," +
                              $"\"{prediction.reasoning.Replace("\"", "\"\"")}\"," + // Escape quotes
                              $"\"{supportingPatterns}\"," +
                              $"{currentSession.sessionId}");
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private void ExportBehaviorMetrics(string folder)
        {
            if (behaviorAnalyzer == null) return;
            
            string filePath = Path.Combine(folder, "behavior_metrics.csv");
            var metrics = behaviorAnalyzer.GetCurrentMetrics();
            var trends = behaviorAnalyzer.GetRecentTrends(50);
            
            // Export current metrics
            var csv = new StringBuilder();
            csv.AppendLine("MetricType,Value,Timestamp,SessionId");
            
            csv.AppendLine($"UpTrendRatio,{metrics.upTrendRatio:F6},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{currentSession.sessionId}");
            csv.AppendLine($"DownTrendRatio,{metrics.downTrendRatio:F6},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{currentSession.sessionId}");
            csv.AppendLine($"NeutralTrendRatio,{metrics.neutralTrendRatio:F6},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{currentSession.sessionId}");
            csv.AppendLine($"AverageVolatility,{metrics.averageVolatility:F6},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{currentSession.sessionId}");
            csv.AppendLine($"TrendPersistence,{metrics.trendPersistence:F6},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{currentSession.sessionId}");
            csv.AppendLine($"ReversalFrequency,{metrics.reversalFrequency:F6},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{currentSession.sessionId}");
            csv.AppendLine($"FractalComplexity,{metrics.fractalComplexity:F6},{DateTime.Now:yyyy-MM-dd HH:mm:ss},{currentSession.sessionId}");
            
            File.WriteAllText(filePath, csv.ToString());
            
            // Export trends
            ExportTrendsData(folder, trends);
        }
        
        private void ExportTrendsData(string folder, List&lt;MarketTrend&gt; trends)
        {
            string filePath = Path.Combine(folder, "market_trends.csv");
            
            var csv = new StringBuilder();
            csv.AppendLine("TrendDirection,Strength,Duration,Volatility,Confidence,StartTime,EndTime,SessionId");
            
            foreach (var trend in trends)
            {
                csv.AppendLine($"{trend.direction}," +
                              $"{trend.strength:F6}," +
                              $"{trend.duration:F2}," +
                              $"{trend.volatility:F6}," +
                              $"{trend.confidence:F4}," +
                              $"{trend.startTime:yyyy-MM-dd HH:mm:ss}," +
                              $"{trend.endTime:yyyy-MM-dd HH:mm:ss}," +
                              $"{currentSession.sessionId}");
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private void CreateExportManifest(string folder)
        {
            string manifestPath = Path.Combine(folder, "export_manifest.txt");
            
            var manifest = new StringBuilder();
            manifest.AppendLine("FMTS Session Export Manifest");
            manifest.AppendLine("================================");
            manifest.AppendLine($"Export Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            manifest.AppendLine($"Session ID: {currentSession.sessionId}");
            manifest.AppendLine($"Unity Version: {Application.unityVersion}");
            manifest.AppendLine($"Platform: {Application.platform}");
            manifest.AppendLine();
            
            manifest.AppendLine("Files Exported:");
            var files = Directory.GetFiles(folder);
            foreach (var file in files.Where(f =&gt; f != manifestPath))
            {
                var fileInfo = new FileInfo(file);
                manifest.AppendLine($"- {fileInfo.Name} ({fileInfo.Length} bytes)");
            }
            
            manifest.AppendLine();
            manifest.AppendLine("Export Configuration:");
            manifest.AppendLine($"- Include Pattern Details: {includePatternDetails}");
            manifest.AppendLine($"- Include Prediction History: {includePredictionHistory}");
            manifest.AppendLine($"- Include Market Data: {includeMarketData}");
            manifest.AppendLine($"- Real-time Export: {exportRealTime}");
            
            if (exportLog.Count &gt; 0)
            {
                manifest.AppendLine();
                manifest.AppendLine("Export Log:");
                foreach (var logEntry in exportLog.TakeLast(10))
                {
                    manifest.AppendLine($"- {logEntry}");
                }
            }
            
            File.WriteAllText(manifestPath, manifest.ToString());
        }
        
        private void AddToExportLog(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            exportLog.Add(logEntry);
            
            // Keep log manageable
            if (exportLog.Count &gt; 100)
            {
                exportLog = exportLog.TakeLast(50).ToList();
            }
        }
        
        public void StartNewSession()
        {
            // Export current session before starting new one
            if (currentSession.totalDataPoints &gt; 0)
            {
                ExportSessionData();
            }
            
            // Reset for new session
            currentSession = new SessionSummary();
            sessionMarketData.Clear();
            AddToExportLog("New session started");
        }
        
        public void ForceExport()
        {
            ExportSessionData();
        }
        
        public string GetExportPath()
        {
            return exportPath;
        }
        
        public SessionSummary GetCurrentSession()
        {
            UpdateSessionSummary();
            return currentSession;
        }
        
        public List&lt;string&gt; GetExportLog()
        {
            return new List&lt;string&gt;(exportLog);
        }
        
        public Dictionary&lt;string, object&gt; GetExportStats()
        {
            var stats = new Dictionary&lt;string, object&gt;
            {
                ["CurrentSessionId"] = currentSession.sessionId,
                ["SessionStartTime"] = currentSession.sessionStart,
                ["TotalDataPoints"] = currentSession.totalDataPoints,
                ["ExportPath"] = exportPath,
                ["LastExportTime"] = lastExportTime,
                ["AutoExportEnabled"] = autoExportOnSessionEnd,
                ["RealTimeExportEnabled"] = exportRealTime
            };
            
            return stats;
        }
    }
}
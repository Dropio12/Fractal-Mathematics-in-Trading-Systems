# Fractal Mathematics in Trading Systems

<img width="1248" height="793" alt="image" src="https://github.com/user-attachments/assets/514200f3-a946-4c72-adf7-346d8adae28d" />

## Overview
This repository contains the FMTS application, an advanced market simulation software that explores fractal mathematics in trading systems. The application features sophisticated pattern recognition, predictive analytics, and real-time market behavior analysis. Available for both Mac and Windows platforms.

## What is FMTS?
FMTS (Fractal Mathematics in Trading Systems) is an intelligent market simulation application built with Unity, designed to:
- **Identify self-similar market fractal patterns** using advanced mathematical algorithms
- **Model complex market behaviors** with fractal dimension calculations and box-counting methods
- **Generate predictive insights** through ensemble machine learning approaches
- **Analyze market trends** using multi-scale volatility clustering and pattern matching
- **Export comprehensive data** for further research and analysis

## Repository Structure
- **mac-version/**: Contains the Mac version of the application
  - `FMTS (Mac).app/`: The extracted Mac application bundle
  - `FMTS (Mac).app.zip`: The original compressed Mac application (39.3 MB)

- **windows-version/**: Contains the Windows version of the application  
  - `FMTS (Windows)/`: The extracted Windows application files
  - `FMTS (Windows).zip`: The original compressed Windows application (27.5 MB)

## Technical Details
- **Platform**: Cross-platform (Mac & Windows)
- **Engine**: Unity (based on file structure and dependencies)
- **Runtime**: Uses MonoBleedingEdge for managed code execution
- **Graphics**: DirectX 12 support (Windows version)
- **Architecture**: 64-bit (x86_64)

## Key Features

### 🔍 Advanced Fractal Pattern Recognition
- **Self-Similarity Detection**: Identifies recurring patterns at multiple time scales using fractal dimension analysis
- **Box-Counting Algorithm**: Calculates fractal dimensions with precision for pattern complexity measurement
- **Dynamic Time Warping**: Compares pattern similarities across different time horizons
- **Pattern Confidence Scoring**: Evaluates pattern reliability based on occurrence frequency and mathematical properties

### 📊 Intelligent Market Behavior Analysis
- **Multi-Scale Moving Averages**: Short, medium, and long-term trend analysis
- **Volatility Clustering Detection**: Identifies periods of high/low market volatility
- **Trend Persistence Calculation**: Measures market momentum and direction stability
- **Reversal Frequency Analysis**: Tracks market turning points and cycles

### 🎯 Predictive Insight Engine
- **Ensemble Prediction Methods**: Combines 5 different prediction algorithms for robust forecasting
- **Neural Network Integration**: Simple feed-forward network for pattern-based predictions
- **Volume-Weighted Analysis**: Incorporates trading volume for enhanced prediction accuracy
- **Confidence-Based Filtering**: Only presents predictions above configurable confidence thresholds

### 📈 Real-Time Market Simulation
- **Fractal Market Generation**: Creates realistic market data using geometric Brownian motion with fractal noise
- **Volatility Clustering**: Simulates real market volatility patterns
- **Volume Spike Modeling**: Generates realistic volume patterns during market events
- **Configurable Parameters**: Adjustable volatility, trend strength, and simulation speed

### 💾 Comprehensive Data Export
- **CSV Data Export**: Exports session data, patterns, predictions, and market metrics
- **Real-time Logging**: Continuous data capture during simulation sessions
- **Pattern Point Export**: Detailed fractal pattern coordinates for external analysis
- **Behavioral Metrics Export**: Trend analysis and market behavior statistics

### 🎮 Unity-Based Platform
- **Cross-platform compatibility** (Mac & Windows)
- **Real-time visualization** of market data and patterns
- **Interactive UI** for system monitoring and control
- **Performance-optimized** with Burst compilation
- **Comprehensive .NET framework** integration

## Installation & Usage

### Mac Version
1. Extract or download the `FMTS (Mac).app` from the mac-version directory
2. Run the application bundle
3. The system will automatically start market simulation and pattern analysis
4. Data exports will be saved to the application's persistent data directory

### Windows Version  
1. Extract or download the contents from `FMTS (Windows)` directory
2. Run `Market Simulation.exe`
3. The system will automatically start market simulation and pattern analysis
4. Data exports will be saved to the application's persistent data directory

### Key Controls
- **Auto-simulation**: Runs continuously, generating realistic market data
- **Real-time Analysis**: Patterns are identified and predictions generated automatically
- **Data Export**: Session data is exported automatically on application exit
- **Session Management**: Start new sessions to reset analysis and begin fresh data collection

## Dependencies
The application includes all necessary runtime dependencies:
- Unity Player/Engine
- Mono runtime (.NET implementation)
- Platform-specific graphics libraries
- Burst-compiled optimized code

## Technical Architecture

### Core Systems
1. **FractalPatternRecognition.cs**: Core pattern detection using fractal dimension analysis
2. **MarketBehaviorAnalyzer.cs**: Market trend and volatility analysis system
3. **PredictiveInsightEngine.cs**: Multi-method prediction system with neural network integration
4. **SessionDataExporter.cs**: Comprehensive data logging and CSV export functionality
5. **FMTSController.cs**: Main system coordinator and market data generator

### Mathematical Foundations
- **Fractal Dimension Calculation**: Box-counting method for pattern complexity analysis
- **Self-Similarity Detection**: Multi-scale pattern comparison using Dynamic Time Warping
- **Geometric Brownian Motion**: Realistic market price simulation with fractal noise components
- **Volatility Clustering**: Fractal-based volatility modeling for realistic market behavior
- **Ensemble Learning**: Combines multiple prediction methods for robust forecasting

### Data Export Structure
Exported CSV files include:
- `session_summary.csv`: Overall session metrics and performance
- `market_data.csv`: Tick-by-tick price, volume, and direction data
- `fractal_patterns.csv`: Discovered patterns with similarity indices
- `pattern_points.csv`: Detailed normalized pattern coordinates
- `predictions.csv`: All predictions with confidence scores and reasoning
- `behavior_metrics.csv`: Market behavior analysis results
- `market_trends.csv`: Identified trends with strength and persistence metrics

## Research Applications

### Academic Research
- **Fractal market hypothesis testing** with real-time pattern identification
- **Market efficiency studies** using self-similarity analysis
- **Volatility clustering research** with fractal dimension measurements
- **Pattern recognition validation** for financial time series

### Quantitative Finance
- **Algorithm development** for pattern-based trading strategies
- **Risk assessment modeling** using fractal complexity measures
- **Market regime identification** through behavioral pattern analysis
- **Backtesting framework** for fractal-based prediction models

### Educational Applications
- **Fractal mathematics visualization** in financial contexts
- **Market simulation training** with realistic price generation
- **Pattern recognition learning** with immediate feedback
- **Data analysis skills development** using exported CSV datasets

## File Sizes
- Mac Version: ~39.3 MB (compressed)
- Windows Version: ~27.5 MB (compressed)

## System Capabilities Summary

**FMTS delivers a comprehensive fractal mathematics trading analysis platform featuring:**

✅ **Real-time pattern recognition** using advanced fractal dimension calculations  
✅ **Self-similarity detection** across multiple market time scales  
✅ **Ensemble prediction engine** combining 5 different forecasting methods  
✅ **Comprehensive data export** with detailed CSV analytics  
✅ **Live market simulation** using fractal-enhanced Brownian motion  
✅ **Behavioral trend analysis** with persistence and reversal tracking  
✅ **Neural network integration** for pattern-based market predictions  
✅ **Cross-platform deployment** on Mac and Windows systems  

---
*This repository contains the complete FMTS (Fractal Mathematics in Trading Systems) application - a sophisticated research platform that combines advanced mathematical analysis with real-time market simulation. The system identifies self-similar market patterns, models complex behaviors, and explores predictive insights through fractal mathematics, providing researchers and analysts with powerful tools for understanding market dynamics.*

**🔬 Research Features**: Pattern recognition • Fractal analysis • Predictive modeling  
**📊 Data Analytics**: Real-time CSV export • Behavioral metrics • Trend analysis  
**⚡ Performance**: Unity-optimized • Real-time processing • Configurable simulation  
**📈 Applications**: Academic research • Algorithm development • Educational training

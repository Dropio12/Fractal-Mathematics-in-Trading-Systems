# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview
FMTS (Fractal Mathematics in Trading Systems) is a Unity-based market simulation application that explores fractal mathematics in financial markets. The repository contains compiled applications for both Windows and Mac platforms, along with utility scripts for locating generated data.

## Core Architecture

### Application Structure
- **Unity Engine**: Built with Unity (Company: Krafer, Project: Market Simulation)
- **Platform Support**: Cross-platform Windows (.exe) and macOS (.app) builds
- **Runtime**: MonoBleedingEdge with .NET framework integration
- **Performance**: Burst-compiled optimized code for mathematical calculations

### Key Components (from README analysis)
1. **FractalPatternRecognition.cs**: Core pattern detection using fractal dimension analysis
2. **MarketBehaviorAnalyzer.cs**: Market trend and volatility analysis system  
3. **PredictiveInsightEngine.cs**: Multi-method prediction system with neural network integration
4. **SessionDataExporter.cs**: Comprehensive data logging and CSV export functionality
5. **FMTSController.cs**: Main system coordinator and market data generator

### Data Flow
The application generates market data using fractal-enhanced Brownian motion and exports comprehensive analytics to CSV files in the Unity persistent data directory (`%USERPROFILE%\AppData\LocalLow\DefaultCompany\Market Simulation\FMTS_Data\Sessions\`).

## Development Commands

### Running the Application
**Windows:**
```powershell
# Navigate to the Windows build directory
cd "windows-version\FMTS (Windows)\FMTS"
# Run the market simulation
.\Market Simulation.exe
```

**Mac:**
```bash
# Navigate to the Mac build directory  
cd "mac-version"
# Run the application bundle
open "FMTS (Mac).app"
```

### Data Location and Management
```powershell
# Find FMTS CSV exports using the built-in script
.\FindFMTSData.ps1

# Alternative batch file for Windows
.\FindCSVFiles.bat

# Manual location check
explorer "$env:USERPROFILE\AppData\LocalLow\DefaultCompany\Market Simulation\FMTS_Data\Sessions"
```

### Data Analysis
```powershell
# List all generated session data
Get-ChildItem "$env:USERPROFILE\AppData\LocalLow\DefaultCompany\Market Simulation\FMTS_Data\Sessions" -Recurse -Filter "*.csv"

# Quick CSV preview (first session found)
$sessionPath = Get-ChildItem "$env:USERPROFILE\AppData\LocalLow\DefaultCompany\Market Simulation\FMTS_Data\Sessions" -Directory | Select-Object -First 1
Get-Content "$($sessionPath.FullName)\session_summary.csv" -Head 10
```

## Data Export Structure

The application exports 7 types of CSV files per session:
- `session_summary.csv`: Overall session metrics and performance
- `market_data.csv`: Tick-by-tick price, volume, and direction data  
- `fractal_patterns.csv`: Discovered patterns with similarity indices
- `pattern_points.csv`: Detailed normalized pattern coordinates
- `predictions.csv`: All predictions with confidence scores and reasoning
- `behavior_metrics.csv`: Market behavior analysis results
- `market_trends.csv`: Identified trends with strength and persistence metrics

## Build Information
- **Unity Build GUID**: cf9565fa11f340408d1ea5af46b2ea1e
- **Graphics Threading**: Mode 6 (Multi-threaded rendering)
- **DirectX 12**: Supported on Windows with dedicated D3D12Core.dll
- **Burst Compilation**: Enabled for mathematical performance optimization

## Research Applications

### Mathematical Focus Areas
- Fractal dimension calculations using box-counting methods
- Self-similarity detection across multiple time scales
- Dynamic time warping for pattern comparison
- Geometric Brownian motion with fractal noise components
- Volatility clustering using fractal-based modeling

### Machine Learning Components  
- Ensemble prediction methods (5 different algorithms)
- Simple feed-forward neural network integration
- Volume-weighted analysis for prediction accuracy
- Confidence-based filtering for prediction quality

## File Management Notes
- Compiled applications are distributed as compressed archives (.zip)
- Windows version: ~27.5 MB compressed  
- Mac version: ~39.3 MB compressed
- Data exports are automatically managed by Unity's persistent data system
- Session data includes timestamps and unique identifiers for organization
# Fractal Market Demo

A standalone C# console application demonstrating fractal pattern recognition in simulated market data, showcasing the core concepts from the FMTS (Fractal Mathematics in Trading Systems).

## Overview

This demo application illustrates how fractal mathematics can be applied to financial market analysis, combining rigorous mathematical methods with practical trading system concepts.

## Features

### 🎲 Market Data Simulation
- Geometric Brownian motion with fractal noise
- Golden ratio scaling for self-similar patterns
- Volume correlation and volatility modeling
- Realistic market event simulation

### 🔍 Fractal Pattern Recognition
- Box-counting method for fractal dimension calculation
- Self-similarity detection across multiple scales
- Dynamic Time Warping for pattern comparison  
- Pattern classification and confidence scoring

### 📊 Market Behavior Analysis
- Trend detection and persistence measurement
- Volatility clustering identification
- Mean reversion tendency calculation
- Momentum indicators

### 🎯 Ensemble Prediction Engine
- Trend following analysis
- Mean reversion signals
- Fractal pattern-based predictions
- Volatility analysis
- Momentum indicators
- Weighted consensus calculation

### 💾 Data Export & Visualization
- Comprehensive console visualizations
- ASCII price charts and pattern displays
- CSV export for detailed analysis
- Session summary reports

## Key Components

### Core Classes

1. **MarketDataGenerator**: Generates realistic market data using fractal-enhanced Brownian motion
2. **FractalPatternRecognizer**: Detects patterns using box-counting and self-similarity analysis
3. **MarketBehaviorAnalyzer**: Analyzes trends, volatility clustering, and market dynamics
4. **DataExportAndVisualization**: Handles display and CSV export functionality
5. **Program**: Main orchestrator that runs the complete demonstration

### Mathematical Concepts

- **Fractal Dimension**: Calculated using the box-counting method
- **Self-Similarity**: Detected using Dynamic Time Warping
- **Geometric Brownian Motion**: Enhanced with multi-octave fractal noise
- **Volatility Clustering**: Identified through autocorrelation analysis
- **Ensemble Methods**: Multiple prediction algorithms combined with confidence weighting

## How to Run

### Prerequisites
- .NET 6.0 or later
- Windows, macOS, or Linux

### Compilation and Execution

```bash
# Navigate to the project directory
cd FractalMarketDemo

# Build the project
dotnet build

# Run the application
dotnet run
```

### Alternative Compilation (if dotnet CLI not available)
The project can also be compiled using Visual Studio or any C# compiler that supports .NET 6.0.

## Demo Workflow

The application runs through 7 interactive steps:

1. **Market Data Generation**: Creates 500 data points using fractal-enhanced simulation
2. **Market Events**: Adds volatility clustering and market shocks
3. **Pattern Recognition**: Detects fractal patterns using box-counting method
4. **Behavior Analysis**: Analyzes trends, volatility, and market dynamics  
5. **Ensemble Predictions**: Generates predictions using 5 different methods
6. **Results Display**: Shows comprehensive analysis with visualizations
7. **Data Export**: Exports all results to timestamped CSV files

## Output Files

The application exports 5 CSV files per session:

- `market_data_[timestamp].csv` - Raw simulated market data
- `fractal_patterns_[timestamp].csv` - Detected fractal patterns  
- `market_behavior_[timestamp].csv` - Market behavior analysis results
- `predictions_[timestamp].csv` - Ensemble prediction results
- `session_summary_[timestamp].csv` - Overall session summary

## Educational Value

This demonstration illustrates:

- How fractal mathematics applies to financial markets
- Box-counting method for measuring complexity
- Self-similarity detection in time series data
- Multi-scale pattern recognition techniques
- Ensemble machine learning approaches
- Volatility clustering and mean reversion concepts

## Customization

Key parameters can be modified in the `FractalMarketDemoApplication` constructor:

```csharp
_dataGenerator = new MarketDataGenerator(
    drift: 0.0002,           // Market drift (trend bias)
    volatility: 0.025,       // Base volatility (2.5%)
    fractalDimension: 1.4    // Fractal dimension (1.0-2.0)
);
```

## Technical Notes

- **Fractal Dimension Range**: Typically 1.1 to 1.9 for market data
- **Pattern Length**: 20 to 100 data points for reliable detection
- **Confidence Threshold**: 0.6 minimum for pattern acceptance
- **Volatility Clustering**: 1.5x threshold for cluster detection

## Future Enhancements

- Real-time data integration
- Additional pattern types (Elliott waves, head & shoulders, etc.)
- Neural network pattern classification
- Multi-timeframe analysis
- Portfolio optimization using fractal insights

## Related Research

This demo is inspired by:
- Mandelbrot's work on fractal market hypothesis
- Box-counting methods in complexity science
- Dynamic time warping for pattern matching
- Ensemble methods in machine learning
- Volatility clustering in financial econometrics

## License

This is a demonstration application for educational purposes. The concepts and algorithms are based on established mathematical and financial research.
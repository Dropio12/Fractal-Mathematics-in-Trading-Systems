# Fractal Trading Analysis

Started this as a side project to figure out if fractal mathematics could actually help with trading decisions. Ended up building a distributed system that processes thousands of market ticks per second and generates some pretty interesting patterns.

## What It Does

Analyzes market data using fractal geometry - basically measuring how "rough" price movements are at different time scales. When volatility spikes, fractal dimensions change, and that can signal interesting trading opportunities.

Built the core analysis in 4 different languages because I wanted to see performance differences:
- **C#**: Full-featured version with all the bells and whistles
- **Go**: Concurrent processing with goroutines (surprisingly fast)
- **C++**: Maximum performance, minimal dependencies
- **C**: Bare metal implementation for when you need every microsecond

Plus a complete distributed setup with Kafka, Spark, and real-time monitoring.

## Quick Start

For the basic analysis, just run:
```bash
run_all.bat
```

For the full distributed system:
```bash
cd docker-infrastructure
deploy-fractal-system.bat
```

## What You Get

Each implementation spits out CSV files with:
- Raw market data (10,000 simulated candles)
- Detected fractal patterns with confidence scores
- Volatility clustering analysis
- Performance metrics

## Real-Time System

The distributed version is where things get interesting:
- Kafka streams processing thousands of ticks/second
- Spark cluster for heavy fractal calculations
- HFT simulation with microsecond precision
- Grafana dashboards that don't suck

Access at http://localhost/ after deployment.

## Notes

Built on Windows but should work elsewhere with minor tweaks. The Docker setup handles most dependencies automatically.

If you're just experimenting, start with the C# or Go versions - they're the most user-friendly.

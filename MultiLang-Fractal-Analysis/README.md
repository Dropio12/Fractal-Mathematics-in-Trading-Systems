# Multi-Language Fractal Analysis

This project demonstrates fractal market analysis (box-counting dimension, self-similarity, volatility clustering) implemented in multiple languages: C#, Go, C++, and C. It simulates 10,000 candles (hourly data) and exports comparable CSV outputs for cross-language validation.

Languages:
- csharp/: .NET console app (10,000 candles, CSV export)
- go/: Go CLI (goroutines to parallelize analysis)
- cpp/: C++17 console app (high-performance STL)
- c/: ANSI C console app (portable, minimal deps)
- shared/: Shared CSV schema, sample configs

Build prerequisites:
- Windows: Recommended (msvc for C/C++, .NET for C#, Go toolchain for Go)
- If .NET SDK is missing, use the provided C# .NET Framework fallback or run Go/C++/C variants

Outputs (per implementation):
- market_data.csv
- fractal_patterns.csv
- behavior_metrics.csv
- session_summary.csv

Run order suggestion:
1) csharp/ (if dotnet available), otherwise go/
2) cpp/
3) c/

All implementations write outputs to their own subfolder.

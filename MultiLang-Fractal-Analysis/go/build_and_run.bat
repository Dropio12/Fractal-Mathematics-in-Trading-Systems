@echo off
echo Building Go Fractal Analysis (10,000 candles with goroutines)...

where go >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: Go compiler not found in PATH
    echo Please install Go from https://golang.org/dl/
    pause
    exit /b 1
)

echo Building...
go build -o fractal-analysis.exe main.go
if %errorlevel% neq 0 (
    echo Build failed
    pause
    exit /b 1
)

echo Running...
fractal-analysis.exe

pause
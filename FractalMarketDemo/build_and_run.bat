@echo off
echo.
echo ====================================================================
echo                   FRACTAL MARKET DEMO - BUILD & RUN
echo ====================================================================
echo.

REM Check if dotnet is available
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found in PATH
    echo.
    echo Please install .NET 6.0 SDK or later from:
    echo https://dotnet.microsoft.com/download/dotnet
    echo.
    echo Alternatively, compile using Visual Studio:
    echo 1. Open FractalMarketDemo.csproj in Visual Studio
    echo 2. Build Solution (Ctrl+Shift+B)
    echo 3. Run the application (F5 or Ctrl+F5)
    echo.
    pause
    exit /b 1
)

echo Checking .NET version...
dotnet --version
echo.

echo Building Fractal Market Demo...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo.
    echo BUILD FAILED! Please check for compilation errors above.
    echo.
    pause
    exit /b 1
)

echo.
echo BUILD SUCCESSFUL!
echo.
echo Starting Fractal Market Demo...
echo This will demonstrate fractal pattern recognition in simulated market data.
echo.
pause

dotnet run

echo.
echo Demo completed. Check the FractalMarketResults folder for exported CSV files.
echo.
pause
@echo off
echo Building C# Fractal Analysis (10,000 candles)...

REM Try .NET SDK first
where dotnet >nul 2>nul
if %errorlevel% == 0 (
    echo Using .NET SDK...
    dotnet run --project SimpleFractalDemo.cs
    goto :end
)

REM Fall back to .NET Framework compiler
if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    echo Using .NET Framework 4.0 compiler...
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" SimpleFractalDemo.cs
    if exist SimpleFractalDemo.exe (
        SimpleFractalDemo.exe
    )
) else (
    echo ERROR: No C# compiler found
    echo Please install .NET SDK or ensure .NET Framework is available
    pause
)

:end
pause
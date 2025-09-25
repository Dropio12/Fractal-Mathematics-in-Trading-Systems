@echo off
echo Building C++ Fractal Analysis (10,000 candles with STL)...

REM Try to find Visual Studio C++ compiler
where cl >nul 2>nul
if %errorlevel% == 0 (
    echo Using MSVC compiler...
    cl /EHsc /std:c++17 main.cpp /Fe:fractal-analysis.exe
    if exist fractal-analysis.exe (
        fractal-analysis.exe
        goto :end
    )
)

REM Try g++ (MinGW)
where g++ >nul 2>nul
if %errorlevel% == 0 (
    echo Using g++ compiler...
    g++ -std=c++17 -O2 main.cpp -o fractal-analysis.exe
    if exist fractal-analysis.exe (
        fractal-analysis.exe
        goto :end
    )
)

REM Try clang++
where clang++ >nul 2>nul
if %errorlevel% == 0 (
    echo Using clang++ compiler...
    clang++ -std=c++17 -O2 main.cpp -o fractal-analysis.exe
    if exist fractal-analysis.exe (
        fractal-analysis.exe
        goto :end
    )
)

echo ERROR: No C++ compiler found
echo Please install Visual Studio, MinGW, or Clang
pause

:end
pause
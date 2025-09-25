@echo off
echo Building C Fractal Analysis (10,000 candles, pure C)...

REM Try Visual Studio C compiler
where cl >nul 2>nul
if %errorlevel% == 0 (
    echo Using MSVC compiler...
    cl main.c /Fe:fractal-analysis.exe
    if exist fractal-analysis.exe (
        fractal-analysis.exe
        goto :end
    )
)

REM Try gcc (MinGW)
where gcc >nul 2>nul
if %errorlevel% == 0 (
    echo Using gcc compiler...
    gcc -std=c99 -O2 -lm main.c -o fractal-analysis.exe
    if exist fractal-analysis.exe (
        fractal-analysis.exe
        goto :end
    )
)

REM Try clang
where clang >nul 2>nul
if %errorlevel% == 0 (
    echo Using clang compiler...
    clang -std=c99 -O2 main.c -o fractal-analysis.exe -lm
    if exist fractal-analysis.exe (
        fractal-analysis.exe
        goto :end
    )
)

echo ERROR: No C compiler found
echo Please install Visual Studio, MinGW, or Clang
pause

:end
pause
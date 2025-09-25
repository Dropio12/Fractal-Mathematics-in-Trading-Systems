@echo off
echo =========================================================================
echo        MULTI-LANGUAGE FRACTAL ANALYSIS - 10,000 CANDLES
echo                    C# | Go | C++ | C
echo =========================================================================
echo.

echo Running all implementations...
echo Each implementation generates 10,000 market candles and analyzes
echo fractal dimensions using box-counting method.
echo.

REM C# Implementation
echo [1/4] Running C# implementation...
echo ---------------------------------
cd csharp
call build_and_run.bat
cd ..
echo.

REM Go Implementation  
echo [2/4] Running Go implementation...
echo ---------------------------------
cd go
call build_and_run.bat
cd ..
echo.

REM C++ Implementation
echo [3/4] Running C++ implementation...
echo ----------------------------------
cd cpp
call build_and_run.bat
cd ..
echo.

REM C Implementation
echo [4/4] Running C implementation...
echo --------------------------------
cd c
call build_and_run.bat
cd ..
echo.

echo =========================================================================
echo                            ANALYSIS COMPLETE
echo =========================================================================
echo.
echo Check the following directories for CSV outputs:
echo   - csharp/out-csharp/
echo   - go/out-go/
echo   - cpp/out-cpp/  
echo   - c/out-c/
echo.
echo Each contains:
echo   - market_data.csv     (10,000 candles with OHLC data)
echo   - session_summary.csv (fractal dimension results)
echo   - fractal_patterns.csv (detailed pattern analysis)
echo.

pause
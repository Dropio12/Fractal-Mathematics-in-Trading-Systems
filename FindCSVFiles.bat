@echo off
echo.
echo ==========================================
echo    FMTS CSV File Locator
echo ==========================================
echo.
echo Searching for your FMTS CSV export files...
echo.

set LOCALLOW=%USERPROFILE%\AppData\LocalLow

echo Checking common Unity application paths:
echo.

REM Check for DefaultCompany/Market Simulation
if exist "%LOCALLOW%\DefaultCompany\Market Simulation\FMTS_Data\Sessions" (
    echo [FOUND] %LOCALLOW%\DefaultCompany\Market Simulation\FMTS_Data\Sessions
    dir "%LOCALLOW%\DefaultCompany\Market Simulation\FMTS_Data\Sessions" /B 2>nul
    if exist "%LOCALLOW%\DefaultCompany\Market Simulation\FMTS_Data\Sessions\*.csv" (
        echo CSV files found in this directory!
        dir "%LOCALLOW%\DefaultCompany\Market Simulation\FMTS_Data\Sessions\*.csv" /B 2>nul
    )
    echo.
    echo Opening this folder...
    explorer "%LOCALLOW%\DefaultCompany\Market Simulation\FMTS_Data\Sessions"
    goto :found
)

REM Check for DefaultCompany/FMTS
if exist "%LOCALLOW%\DefaultCompany\FMTS\FMTS_Data\Sessions" (
    echo [FOUND] %LOCALLOW%\DefaultCompany\FMTS\FMTS_Data\Sessions
    dir "%LOCALLOW%\DefaultCompany\FMTS\FMTS_Data\Sessions" /B 2>nul
    if exist "%LOCALLOW%\DefaultCompany\FMTS\FMTS_Data\Sessions\*.csv" (
        echo CSV files found in this directory!
        dir "%LOCALLOW%\DefaultCompany\FMTS\FMTS_Data\Sessions\*.csv" /B 2>nul
    )
    echo.
    echo Opening this folder...
    explorer "%LOCALLOW%\DefaultCompany\FMTS\FMTS_Data\Sessions"
    goto :found
)

REM Search for any FMTS_Data directories
echo Searching for any FMTS_Data directories...
for /d /r "%LOCALLOW%" %%d in (FMTS_Data) do (
    echo [FOUND] %%d
    if exist "%%d\Sessions" (
        echo   - Has Sessions subdirectory
        dir "%%d\Sessions" /B 2>nul
        if exist "%%d\Sessions\*.csv" (
            echo   - Contains CSV files!
            explorer "%%d\Sessions"
            goto :found
        )
    )
)

echo.
echo [NOT FOUND] No FMTS data directories found yet.
echo.
echo This is normal if you haven't run FMTS yet.
echo.
echo INSTRUCTIONS:
echo 1. Run Market Simulation.exe from the FMTS Windows folder
echo 2. Let it run for 2-3 minutes to generate market data
echo 3. Close the application properly (don't force close)
echo 4. Run this script again to find your CSV files
echo.
echo The files will be created at:
echo %LOCALLOW%\DefaultCompany\Market Simulation\FMTS_Data\Sessions\
echo.
echo Opening LocalLow folder for you...
explorer "%LOCALLOW%"
goto :end

:found
echo.
echo SUCCESS! Your FMTS CSV files should be in the opened folder.
echo.

:end
echo.
pause
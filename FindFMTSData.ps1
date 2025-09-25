# PowerShell Script to Find FMTS CSV Export Locations
# Run this script after using FMTS to locate your exported data

Write-Host "🔍 FMTS Data Finder" -ForegroundColor Green
Write-Host "===================" -ForegroundColor Green
Write-Host ""

$localLowPath = "$env:USERPROFILE\AppData\LocalLow"
Write-Host "📂 Searching in Unity's LocalLow directory:" -ForegroundColor Yellow
Write-Host "   $localLowPath" -ForegroundColor Gray
Write-Host ""

# Common Unity company names for FMTS
$companyNames = @(
    "DefaultCompany",
    "Unity", 
    "FMTS",
    "FractalMathematics",
    "MarketSimulation"
)

# Common application names
$appNames = @(
    "Market Simulation",
    "FMTS",
    "Fractal Mathematics in Trading Systems"
)

$foundPaths = @()

foreach ($company in $companyNames) {
    foreach ($app in $appNames) {
        $testPath = Join-Path $localLowPath "$company\$app"
        if (Test-Path $testPath) {
            $foundPaths += $testPath
            Write-Host "✅ Found Unity app data:" -ForegroundColor Green
            Write-Host "   $testPath" -ForegroundColor White
            
            # Look for FMTS_Data subdirectory
            $fmtsDataPath = Join-Path $testPath "FMTS_Data"
            if (Test-Path $fmtsDataPath) {
                Write-Host "   📊 FMTS_Data directory found!" -ForegroundColor Cyan
                
                # Look for Sessions subdirectory
                $sessionsPath = Join-Path $fmtsDataPath "Sessions"
                if (Test-Path $sessionsPath) {
                    Write-Host "   📁 Sessions directory found!" -ForegroundColor Cyan
                    
                    # List session folders
                    $sessionFolders = Get-ChildItem $sessionsPath -Directory -ErrorAction SilentlyContinue
                    if ($sessionFolders.Count -gt 0) {
                        Write-Host "   🎯 Found $($sessionFolders.Count) session(s):" -ForegroundColor Magenta
                        foreach ($session in $sessionFolders) {
                            Write-Host "      📂 $($session.Name)" -ForegroundColor Gray
                            
                            # List CSV files in this session
                            $csvFiles = Get-ChildItem $session.FullName -Filter "*.csv" -ErrorAction SilentlyContinue
                            if ($csvFiles.Count -gt 0) {
                                Write-Host "         💾 CSV Files:" -ForegroundColor Yellow
                                foreach ($csv in $csvFiles) {
                                    $size = [math]::Round($csv.Length / 1KB, 2)
                                    Write-Host "            📄 $($csv.Name) (${size}KB)" -ForegroundColor White
                                }
                            }
                        }
                    } else {
                        Write-Host "   ⚠️  No session data yet (run FMTS first)" -ForegroundColor Yellow
                    }
                }
            }
            Write-Host ""
        }
    }
}

# Search for any FMTS_Data directories
Write-Host "🔍 Searching for any FMTS_Data directories..." -ForegroundColor Yellow
$allFMTSData = Get-ChildItem $localLowPath -Recurse -Directory -Name "FMTS_Data" -ErrorAction SilentlyContinue
if ($allFMTSData.Count -gt 0) {
    Write-Host "✅ Additional FMTS_Data directories found:" -ForegroundColor Green
    foreach ($dir in $allFMTSData) {
        $fullPath = $dir.FullName
        Write-Host "   📂 $fullPath" -ForegroundColor White
    }
} else {
    Write-Host "❌ No FMTS_Data directories found yet" -ForegroundColor Red
}

Write-Host ""
Write-Host "📝 Instructions:" -ForegroundColor Green
Write-Host "1. Run the FMTS application (Market Simulation.exe)" -ForegroundColor White
Write-Host "2. Let it run for 2-3 minutes to generate data" -ForegroundColor White
Write-Host "3. Close the application properly (don't force-close)" -ForegroundColor White
Write-Host "4. Run this script again to find your CSV files" -ForegroundColor White
Write-Host ""

if ($foundPaths.Count -eq 0) {
    Write-Host "💡 Quick Access Command:" -ForegroundColor Cyan
    Write-Host "   explorer `"$localLowPath`"" -ForegroundColor Gray
    Write-Host "   This will open the LocalLow folder where FMTS will create its data" -ForegroundColor Gray
} else {
    Write-Host "💡 Quick Access Commands:" -ForegroundColor Cyan
    foreach ($path in $foundPaths) {
        Write-Host "   explorer `"$path`"" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "🎯 Most likely location after running FMTS:" -ForegroundColor Green
Write-Host "   $localLowPath\DefaultCompany\Market Simulation\FMTS_Data\Sessions\" -ForegroundColor White
# Zen Tetris build script. Unity Editor KAPALI olmalı (batchmode projeyi kilitler).
# Kullanım:  .\build.ps1            -> her iki hedefi de alır
#            .\build.ps1 windows    -> sadece Windows
#            .\build.ps1 webgl      -> sadece WebGL
param([string]$target = "all")

$editors = Get-Content "$env:APPDATA\UnityHub\editors-v2.json" -Raw | ConvertFrom-Json
$exe = $editors.data[0].location[0]
$proj = "C:\CUSTOM-Projects\Unity\Tetris-Unity"
if (-not (Test-Path $exe)) { throw "Unity.exe bulunamadı: $exe" }

function Invoke-Build([string]$method, [string]$log) {
    Write-Host "Building $method ..." -ForegroundColor Cyan
    $p = Start-Process -FilePath $exe -PassThru -Wait -ArgumentList `
        '-batchmode','-quit','-projectPath',$proj,'-executeMethod',$method,`
        '-logFile',(Join-Path $proj "Logs\$log")
    if ($p.ExitCode -ne 0) {
        Write-Host "FAILED ($method), exit $($p.ExitCode). Bkz: Logs\$log" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK: $method" -ForegroundColor Green
}

if ($target -eq "all" -or $target -eq "windows") { Invoke-Build "BuildScript.BuildWindows" "build-win.log" }
if ($target -eq "all" -or $target -eq "webgl")   { Invoke-Build "BuildScript.BuildWebGL"   "build-web.log" }

Write-Host "Bitti. Çıktı: Build\Windows ve/veya Build\WebGL" -ForegroundColor Green

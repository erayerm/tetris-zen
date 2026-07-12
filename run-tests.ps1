$editors = Get-Content "$env:APPDATA\UnityHub\editors-v2.json" -Raw | ConvertFrom-Json
$exe = $editors.data[0].location[0]
if (-not (Test-Path $exe)) { throw "Unity.exe not found: $exe" }
$proj = "C:\CUSTOM-Projects\Unity\Tetris-Unity"
$results = Join-Path $proj "Logs\test-results.xml"
if (Test-Path $results) { Remove-Item $results }
$p = Start-Process -FilePath $exe -ArgumentList '-batchmode','-projectPath',$proj,'-runTests','-testPlatform','EditMode','-testResults',$results,'-logFile',(Join-Path $proj "Logs\test-run.log") -PassThru -Wait
if (-not (Test-Path $results)) { throw "No test results produced (Unity exit code $($p.ExitCode)). Is the project open in the Editor?" }
[xml]$r = Get-Content $results
"{0}: total={1} passed={2} failed={3}" -f $r.'test-run'.result, $r.'test-run'.total, $r.'test-run'.passed, $r.'test-run'.failed
if ($r.'test-run'.failed -ne "0") {
    $r.SelectNodes("//test-case[@result='Failed']") | ForEach-Object { $_.fullname; $_.failure.message.'#cdata-section' }
    exit 1
}

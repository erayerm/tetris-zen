$latest = Get-ChildItem "C:\Program Files\Unity\Hub\Editor" | Sort-Object Name -Descending | Select-Object -First 1
$exe = Join-Path $latest.FullName "Editor\Unity.exe"
$proj = "C:\CUSTOM-Projects\Unity\Tetris-Unity"
$results = Join-Path $proj "Logs\test-results.xml"
if (Test-Path $results) { Remove-Item $results }
& $exe -batchmode -projectPath $proj -runTests -testPlatform EditMode -testResults $results -logFile (Join-Path $proj "Logs\test-run.log")
[xml]$r = Get-Content $results
"{0}: total={1} passed={2} failed={3}" -f $r.'test-run'.result, $r.'test-run'.total, $r.'test-run'.passed, $r.'test-run'.failed
if ($r.'test-run'.failed -ne "0") {
    $r.SelectNodes("//test-case[@result='Failed']") | ForEach-Object { $_.fullname; $_.failure.message.'#cdata-section' }
    exit 1
}

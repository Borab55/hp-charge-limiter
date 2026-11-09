$ErrorActionPreference = "Stop"

$rootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$srcDir  = $PSScriptRoot

# Ensure UTF-8 with BOM on source files so csc.exe parses Turkish characters and Unicode icons accurately
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
foreach ($csFile in @("I18n.cs", "Backend.cs", "MainWindow.cs", "Program.cs")) {
    $fullPath = Join-Path $srcDir $csFile
    $text = [System.IO.File]::ReadAllText($fullPath, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText($fullPath, $text, $utf8Bom)
}

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$wpfDir = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF"

$coreEngine = Join-Path $srcDir "core\hp-charge-limit-rs.exe"
$iconFile   = Join-Path $srcDir "app_icon.ico"
$outRoot    = Join-Path $rootDir "HPChargeLimiter.exe"

Write-Host "Compiling HPChargeLimiter v2.4.0 (Native DirectX/WPF Edition)..." -ForegroundColor Cyan

& $csc /nologo /target:winexe /optimize+ /platform:x64 /utf8output `
    /out:$outRoot `
    /win32icon:$iconFile `
    /resource:"$coreEngine,HPChargeLimiter.CoreEngine.exe" `
    /resource:"$iconFile,HPChargeLimiter.app_icon.ico" `
    /reference:"$wpfDir\PresentationFramework.dll" `
    /reference:"$wpfDir\PresentationCore.dll" `
    /reference:"$wpfDir\WindowsBase.dll" `
    /reference:"System.Xaml.dll" `
    /reference:"System.Drawing.dll" `
    /reference:"System.Windows.Forms.dll" `
    "$srcDir\I18n.cs" `
    "$srcDir\Backend.cs" `
    "$srcDir\MainWindow.cs" `
    "$srcDir\Program.cs"

if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE"
}

$sizeMB = [Math]::Round((Get-Item $outRoot).Length / 1MB, 2)
Write-Host "Build succeeded! Output: $outRoot ($sizeMB MB)" -ForegroundColor Green

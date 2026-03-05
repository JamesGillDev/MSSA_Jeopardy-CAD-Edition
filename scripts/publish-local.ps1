[CmdletBinding()]
param(
    [string]$Runtime = "win10-x64",
    [string]$Framework = "net8.0-windows10.0.19041.0",
    [string]$Configuration = "Release",
    [switch]$NoRestore,
    [switch]$SkipExplorerIconRefresh
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
$projectPath = Join-Path $repoRoot "MSSA_Jeopardy.Maui\MSSA_Jeopardy.Maui.csproj"
$publishRoot = Join-Path $repoRoot "publish"
$outputPath = Join-Path $publishRoot "current"

[xml]$projectXml = Get-Content -Raw $projectPath
$projectVersion = $projectXml.Project.PropertyGroup.Version

Write-Host "Project version: $projectVersion"
Write-Host "Target framework: $Framework"
Write-Host "Target runtime: $Runtime"
Write-Host "Configuration: $Configuration"

Write-Host "Stopping any running MSSA Jeopardy desktop processes..."
Get-Process "MSSA_Jeopardy.Maui" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process "MSSA_Jeopardy" -ErrorAction SilentlyContinue | Stop-Process -Force

if ([System.IO.Directory]::Exists($publishRoot))
{
    Write-Host "Cleaning existing publish folder: $publishRoot"
    [System.IO.Directory]::Delete($publishRoot, $true)
}

[System.IO.Directory]::CreateDirectory($outputPath) | Out-Null

if (-not $NoRestore)
{
    Write-Host "Running restore..."
    dotnet restore $projectPath -r $Runtime -p:TargetFramework=$Framework
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }
}

Write-Host "Publishing MAUI Blazor desktop app..."
dotnet publish $projectPath `
    -f $Framework `
    -c $Configuration `
    -r $Runtime `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -p:SelfContained=true `
    -p:PublishSingleFile=true `
    -o $outputPath `
    --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

$exe = Get-ChildItem -Path $outputPath -Filter "*.exe" -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1

Write-Host "Publish complete."
Write-Host "Output: $outputPath"
if ($exe)
{
    Write-Host "Run: `"$($exe.FullName)`""
}

if (-not $SkipExplorerIconRefresh)
{
    $ie4uinitPath = Join-Path $env:SystemRoot "System32\ie4uinit.exe"
    if (Test-Path $ie4uinitPath)
    {
        Write-Host "Refreshing Windows Explorer icon cache..."
        & $ie4uinitPath -ClearIconCache | Out-Null
        & $ie4uinitPath -show | Out-Null
    }
}

[CmdletBinding()]
param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
$projectPath = Join-Path $repoRoot "MSSA Jeopardy\MSSA Jeopardy.csproj"
$publishRoot = Join-Path $repoRoot "publish"
$outputPath = Join-Path $publishRoot "current"

[xml]$projectXml = Get-Content -Raw $projectPath
$projectVersion = $projectXml.Project.PropertyGroup.Version

Write-Host "Project version: $projectVersion"
Write-Host "Target runtime: $Runtime"
Write-Host "Configuration: $Configuration"

Write-Host "Stopping any running MSSA_Jeopardy processes..."
Get-Process MSSA_Jeopardy -ErrorAction SilentlyContinue | Stop-Process -Force

if ([System.IO.Directory]::Exists($publishRoot))
{
    Write-Host "Cleaning existing publish folder: $publishRoot"
    [System.IO.Directory]::Delete($publishRoot, $true)
}

[System.IO.Directory]::CreateDirectory($outputPath) | Out-Null

if (-not $NoRestore)
{
    Write-Host "Running restore..."
    dotnet restore $projectPath -r $Runtime
}

Write-Host "Publishing single-file self-contained build..."
dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    /p:PublishSingleFile=true `
    -o $outputPath `
    --no-restore

Write-Host "Publish complete."
Write-Host "Output: $outputPath"
Write-Host "Run: `"$outputPath\MSSA_Jeopardy.exe`""

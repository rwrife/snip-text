param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $repoRoot 'src/SnipText/SnipText.csproj'
$outDir = Join-Path $repoRoot 'artifacts/publish/win-x64'
$zipPath = Join-Path $repoRoot 'artifacts/snip-text-win-x64.zip'

Write-Host "Publishing self-contained win-x64 build from: $project"

dotnet publish $project `
  -c $Configuration `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o $outDir

New-Item -ItemType Directory -Force -Path (Split-Path $zipPath -Parent) | Out-Null
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path (Join-Path $outDir '*') -DestinationPath $zipPath -Force

Write-Host "Created portable zip: $zipPath"

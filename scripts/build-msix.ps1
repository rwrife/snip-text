param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [ValidateSet('x64')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $repoRoot 'packaging/SnipText.Package/SnipText.Package.wapproj'

Write-Host "Building MSIX packaging project: $project"

msbuild $project `
  /restore `
  /p:Configuration=$Configuration `
  /p:Platform=$Platform `
  /p:UapAppxPackageBuildMode=SideloadOnly `
  /p:AppxPackageSigningEnabled=false

Write-Host "MSIX build finished. Artifacts are under packaging/SnipText.Package/AppPackages/."

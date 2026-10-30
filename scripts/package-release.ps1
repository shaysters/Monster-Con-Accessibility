# Builds the release zip into dist\MonsterConAccessibility.zip.
# Usage (from the repo root):
#   powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1 -GameDir "D:\Games\Monster Prom 4 - Monster Con"
param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 4 - Monster Con"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (-not (Test-Path $GameDir)) {
    throw "Game folder not found: $GameDir. Pass -GameDir with the folder that contains MonsterCon.exe."
}

dotnet build MonsterConAccessibilityMod.csproj -c Release -p:GameDir="$GameDir"
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

$out = Join-Path $root "bin\Release\net472"
$dist = Join-Path $root "dist"
$stage = Join-Path $dist "stage"
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Path $stage -Force | Out-Null

# Entries starting with @ come from the repo root instead of the build output.
$files = @("MonsterConAccessibilityMod.dll", "ManualPages.en.txt", "prism.dll", "Prism")
foreach ($entry in $files) {
    if ($entry.StartsWith("@")) { $source = Join-Path $root $entry.Substring(1) } else { $source = Join-Path $out $entry }
    if (-not (Test-Path $source)) { throw "Missing release file: $source" }
    if ((Get-Item $source).PSIsContainer) {
        Copy-Item $source -Destination $stage -Recurse -Force
    } else {
        Copy-Item $source -Destination $stage -Force
    }
}

# The Prism folder carries only Prism's license files; prism.dll sits next to the mod.
$nestedPrism = Join-Path $stage "Prism\prism.dll"
if (Test-Path $nestedPrism) { Remove-Item $nestedPrism -Force }

$zip = Join-Path $dist "MonsterConAccessibility.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
# Write entries with forward slashes (Windows PowerShell's ZipFile writes backslashes).
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::Open($zip, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    Get-ChildItem $stage -Recurse -File | ForEach-Object {
        $name = $_.FullName.Substring($stage.Length + 1).Replace([char]92, [char]47)
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $name) | Out-Null
    }
} finally {
    $archive.Dispose()
}
Remove-Item $stage -Recurse -Force
Write-Host "Built: $zip"

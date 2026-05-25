# Локально подмешивает Packages/manifest.local.json в manifest.json (Unity MCP).
# Не коммить manifest.json после запуска — в репозитории MCP только в manifest.local.json.

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $PSScriptRoot "manifest.json"
$localPath = Join-Path $PSScriptRoot "manifest.local.json"
$examplePath = Join-Path $PSScriptRoot "manifest.local.json.example"

if (-not (Test-Path $localPath)) {
    if (Test-Path $examplePath) {
        Copy-Item $examplePath $localPath
        Write-Host "Created manifest.local.json from example."
    } else {
        Write-Error "manifest.local.json not found. Copy manifest.local.json.example first."
    }
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$local = Get-Content $localPath -Raw | ConvertFrom-Json

foreach ($prop in $local.dependencies.PSObject.Properties) {
    $manifest.dependencies | Add-Member -NotePropertyName $prop.Name -NotePropertyValue $prop.Value -Force
}

$manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath -Encoding utf8
Write-Host "Merged local packages into manifest.json. Do not commit manifest.json if it lists com.local.unitymcp."

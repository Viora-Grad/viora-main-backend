# Downloads the all-MiniLM-L6-v2 ONNX model for local embedding generation.
# Run this script after cloning the repo to restore model.onnx (90 MB).

$ModelDir = Join-Path $PSScriptRoot "Models/all-MiniLM-L6-V2"
$OnnxUrl = "https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx"
$OutputFile = "$ModelDir/model.onnx"

if (Test-Path $OutputFile) {
    Write-Host "model.onnx already exists at $OutputFile" -ForegroundColor Green
    exit 0
}

Write-Host "Downloading model.onnx (90 MB) from HuggingFace..." -ForegroundColor Cyan
if (-not (Test-Path $ModelDir)) {
    Write-Host "Creating Models directory in Viora.Api..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $ModelDir -Force | Out-Null
}

try {
    Invoke-WebRequest -Uri $OnnxUrl -OutFile $OutputFile -UseBasicParsing
    Write-Host "Done! Saved to $OutputFile" -ForegroundColor Green
}
catch {
    Write-Error "Download failed: $_"
    exit 1
}

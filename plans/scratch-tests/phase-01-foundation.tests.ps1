$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) { throw "FAIL: $message" }
    Write-Output "PASS: $message"
}

function Assert-Contains([string]$path, [string]$needle, [string]$message) {
    $content = Get-Content -Raw -LiteralPath $path
    Assert-True ($content.Contains($needle)) $message
}

$reviewMediaPath = Join-Path $root 'Models\ReviewMedia.cs'
$reviewPath = Join-Path $root 'Models\Review.cs'
$contextPath = Join-Path $root 'Data\ApplicationDbContext.cs'
$cloudinaryPath = Join-Path $root 'Interfaces\ICloudinaryService.cs'

Assert-True (Test-Path -LiteralPath $reviewMediaPath) 'ReviewMedia model exists'
Assert-Contains $reviewPath 'ICollection<ReviewMedia>' 'Review exposes ReviewMedia navigation'
Assert-Contains $contextPath 'DbSet<ReviewMedia>' 'DbContext registers ReviewMedia'
Assert-Contains $contextPath 'HasIndex(e => new { e.UserId, e.OrderId, e.ProductId })' 'Review duplicate unique index is configured'
Assert-Contains $contextPath 'HasIndex(e => new { e.ProductId, e.IsVisible, e.CreatedAt })' 'Review public-read index is configured'
Assert-Contains $cloudinaryPath 'Delete' 'Cloudinary service exposes deletion capability'

Write-Output 'Phase 01 foundation assertions passed.'

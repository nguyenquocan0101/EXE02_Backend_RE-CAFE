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

$dtoPath = Join-Path $root 'DTOs\ReviewDto.cs'
$interfacePath = Join-Path $root 'Interfaces\IReviewService.cs'
$servicePath = Join-Path $root 'Services\ReviewService.cs'
$controllerPath = Join-Path $root 'Controllers\Review\ReviewsController.cs'
$adminControllerPath = Join-Path $root 'Controllers\Review\AdminReviewsController.cs'
$orderDtoPath = Join-Path $root 'DTOs\OrderDto.cs'

Assert-True (Test-Path -LiteralPath $dtoPath) 'Review DTOs exist'
Assert-True (Test-Path -LiteralPath $interfacePath) 'Review service interface exists'
Assert-True (Test-Path -LiteralPath $servicePath) 'Review service exists'
Assert-True (Test-Path -LiteralPath $controllerPath) 'Customer review controller exists'
Assert-True (Test-Path -LiteralPath $adminControllerPath) 'Admin review controller exists'
Assert-Contains $controllerPath '[Authorize]' 'Customer mutations require authorization'
Assert-Contains $controllerPath 'RequestSizeLimit' 'Review upload request has an explicit size limit'
Assert-Contains $servicePath 'OrderStatus.Completed' 'Review creation enforces completed orders'
Assert-Contains $servicePath 'MaxImages = 2' 'Review service enforces two-image limit'
Assert-Contains $servicePath 'MaxVideos = 1' 'Review service enforces one-video limit'
Assert-Contains $servicePath 'DeleteAsync' 'Review service cleans Cloudinary assets'
Assert-Contains $orderDtoPath 'ReviewId' 'Order item DTO exposes review state'

Write-Output 'Phase 02 API assertions passed.'

$ErrorActionPreference = 'Stop'

$feRoot = 'W:/DevPool/RECAFE_EXE01/RECAFE_EXE01'
$checks = @(
    @{ Path = "$feRoot/src/services/api/reviews.ts"; Pattern = 'FormData' },
    @{ Path = "$feRoot/src/components/reviews/ReviewModal.tsx"; Pattern = 'ReviewModal' },
    @{ Path = "$feRoot/src/components/reviews/StarRating.tsx"; Pattern = 'StarRating' },
    @{ Path = "$feRoot/src/components/reviews/ProductReviews.tsx"; Pattern = 'ProductReviews' },
    @{ Path = "$feRoot/src/pages/Profile.tsx"; Pattern = 'ReviewModal' },
    @{ Path = "$feRoot/src/pages/ProductDetail.tsx"; Pattern = 'ProductReviews' },
    @{ Path = "$feRoot/src/locales/vi-VN.json"; Pattern = 'reviews.title' },
    @{ Path = "$feRoot/src/locales/en-US.json"; Pattern = 'reviews.title' }
)

$passed = 0
foreach ($check in $checks) {
    if (-not (Test-Path -LiteralPath $check.Path)) {
        throw "Missing expected file: $($check.Path)"
    }

    $content = Get-Content -Raw -LiteralPath $check.Path
    if ($content -notmatch [regex]::Escape($check.Pattern)) {
        throw "Missing expected pattern '$($check.Pattern)' in $($check.Path)"
    }

    $passed++
}

Write-Output "Phase 03 frontend smoke checks: $passed/$($checks.Count)"

$ErrorActionPreference = 'Stop'

$feRoot = 'W:/DevPool/RECAFE_EXE01/RECAFE_EXE01'
$checks = @(
    @{ Path = "$feRoot/src/pages/admin/AdminReviews.tsx"; Pattern = 'AdminReviews' },
    @{ Path = "$feRoot/src/services/api/admin.ts"; Pattern = 'getAdminReviews' },
    @{ Path = "$feRoot/src/services/api/admin.ts"; Pattern = 'setReviewVisibility' },
    @{ Path = "$feRoot/src/App.tsx"; Pattern = 'path="reviews"' },
    @{ Path = "$feRoot/src/layouts/AdminLayout.tsx"; Pattern = '/admin/reviews' },
    @{ Path = "$feRoot/src/pages/admin/AdminDashboard.tsx"; Pattern = '/admin/reviews' },
    @{ Path = "$feRoot/src/locales/vi-VN.json"; Pattern = 'adminReviews.title' },
    @{ Path = "$feRoot/src/locales/en-US.json"; Pattern = 'adminReviews.title' }
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

$controller = Get-Content -Raw -LiteralPath 'W:/DevPool/EXE02_Backend_RE-CAFE/Controllers/Review/AdminReviewsController.cs'
if ($controller -notmatch '\[Authorize\(Roles = "Admin"\)\]') {
    throw 'Admin review controller must remain Admin-only.'
}

Write-Output "Phase 04 admin smoke checks: $passed/$($checks.Count) + backend role guard"

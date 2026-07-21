# Phase 05: Verification, performance và rollout

## Goal

Xác nhận toàn bộ luồng trước khi merge/deploy, đặc biệt các boundary liên quan tiền và quyền.

## Verification Steps

1. Chạy backend build/test và validate migration trên database test.
2. Seed tối thiểu 10.000 payment records, benchmark list page size 100; xác nhận p95 < 500 ms.
3. Chạy API contract/security tests cho list/detail/export và role matrix.
4. Chạy FE build và manual smoke test `/admin/payments`.
5. Regression test checkout COD, bank-transfer QR, profile “pay unpaid order”, successful webhook, duplicate webhook và failed polling.
6. Kiểm tra production bundle/config không chứa SePay API key hoặc simulator.
7. Kiểm tra audit log cho view list/detail/export và review migration/deploy notes.

## Rollout

- Deploy migration trước API/FE nếu migration backward-compatible.
- Bật API list/detail trước, sau đó bật FE route; P2 export có thể feature-gate nếu benchmark chưa đạt.
- Theo dõi 401/403, 5xx, query latency, duplicate webhook và audit-log failures sau deploy.

## Exit Criteria

- Tất cả P1 success criteria pass.
- P2 success criteria pass hoặc được feature-gated với lý do được ghi nhận.
- Không còn blocker security/data-integrity.

## Execution Evidence — 2026-07-21

- Release build: 0 warnings, 0 errors.
- PostgreSQL integration suite: 18 passed, 0 failed, 0 skipped.
- Performance: 10,001 payment rows, page size 100, 20 samples; p95 45.58 ms (limit 500 ms).
- Migration: fresh database migration, `Payments.CreatedAt` and payment query indexes verified; existing rows are backfilled from paid/order timestamps.
- Security/contracts: Admin/Staff/Customer/anonymous role matrix, list/detail/summary/export, CSV safety, audit events, webhook credential validation, concurrent replay and cross-order reference reuse covered.
- Checkout regression: COD, bank transfer QR, profile retrieval of an unpaid order, successful/replayed/concurrent webhook and underpayment covered.
- NuGet audit: backend and test project have no vulnerable packages in the configured sources.
- FE: production build passed; production dependency audit reports 0 vulnerabilities; bundle scan contains no SePay key, dev key variable, or simulator code/copy.
- Test isolation: generated `recafe_payment_tests_*` databases are dropped; post-run count is 0.
- Deferred post-push validation: authenticated browser smoke for `/admin/payments`, empty/error states, CSV download, and polling failure/recovery. Browser-control had no browser instance available; the user approved commit/push with this gate recorded as deferred.

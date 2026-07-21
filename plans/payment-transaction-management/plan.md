# Plan: Quản lý giao dịch thanh toán

**Date:** 2026-07-21
**Status:** Complete — approved; browser smoke deferred to post-push validation
**Mode:** hard (auto-detected)
**Risk:** high-risk — thay đổi schema Payment, thêm API quản trị có dữ liệu tài chính, phân quyền Staff/Admin, audit log và tích hợp webhook SePay.

**Spec:** [plans/payment-transaction-management/spec.md](./spec.md)

## Scope Challenge

- **Exists?** Có payment flow theo đơn hàng và admin order detail, nhưng chưa có payment transaction module riêng.
- **Minimum?** P1 gồm API list/detail, server-side filter/pagination, FE `/admin/payments`, role enforcement, audit log và bảo toàn checkout/profile.
- **Complexity?** Hard/high-risk. P2 summary + CSV export triển khai sau khi P1 ổn định; payment-attempt ledger, refund và chargeback nằm ngoài scope.

## Approach

Mở rộng domain Payment hiện tại thay vì tạo một hệ thống thanh toán mới. Backend là nguồn dữ liệu và phân quyền; FE chỉ hiển thị dữ liệu đã lọc từ server. SePay webhook tiếp tục là nguồn cập nhật trạng thái tự động, còn màn hình payment management MVP là read-only để tránh admin sửa sai dữ liệu tài chính.

## Phase Map

| Phase | Mục tiêu | Phụ thuộc | Stories |
|---|---|---|---|
| 01 | Payment domain, migration, query và admin API | — | P1 list/detail |
| 02 | Auth alignment, audit log và webhook/data integrity | 01 | P1 security/consistency |
| 03 | FE admin payment management | 01, 02 | P1 list/detail/filter |
| 04 | Summary, CSV export và navigation/UX polish | 03 | P2 reporting |
| 05 | Regression, performance, security verification và rollout | 01–04 | Tất cả |

## Cross-Phase Decisions

- `Admin` và `Staff` được read-only payment access; backend và FE phải dùng cùng role set.
- Thêm `Payment.CreatedAt` nếu chưa có, lưu UTC; hiển thị Asia/Ho_Chi_Minh.
- MVP giữ mô hình một Payment settlement cho một Order; duplicate successful webhook phải idempotent.
- Paid revenue chỉ tính payment có status `Paid`, không tính theo order status.
- Export dùng `GET /api/admin/payments/export`, CSV UTF-8, tối đa 10.000 dòng/request.
- Audit log lưu actor, action (`view_list`, `view_detail`, `export`), filters, row count và timestamp; không lưu raw webhook payload.
- Không đưa SePay API key vào browser production; simulator chỉ tồn tại trong dev/test build.

## Risks and Mitigations

1. **Schema/migration risk:** chạy migration trên dữ liệu hiện có và kiểm tra quan hệ Payment–Order trước khi deploy; migration phải backward-compatible.
2. **Financial correctness:** dùng server-side paid-only summary, transaction/duplicate webhook tests và không cho sửa status trong MVP.
3. **Authorization mismatch:** test Admin, Staff, Customer và anonymous ở cả FE route guard lẫn BE endpoint; không chỉ dựa vào FE guard.
4. **Large dataset:** query projection + indexes + server pagination; benchmark với 10.000 payment records trước khi bật export.
5. **Sensitive data exposure:** audit access/export, giới hạn export 10.000 dòng, không trả raw webhook và không bundle API key.

## Verification Gates

- Backend build/test và migration validation trước khi nối FE.
- Contract test cho list/detail/export, filter combination, 401/403/404 và data ownership.
- FE build và route smoke test cho loading/error/empty/pagination/detail/export.
- End-to-end regression cho COD, unpaid bank transfer, successful SePay webhook, duplicate webhook và failed polling.
- Performance check p95 < 500 ms với page size 100 trên 10.000 records.

## Red-Team Review (inline fallback)

- Không dùng order status để suy ra paid revenue.
- Không cho FE paginate local toàn bộ payment history.
- Không thêm manual “mark paid” vào MVP vì có nguy cơ lệch với SePay settlement.
- Không coi route guard là authorization; BE phải enforce role.
- Không expose `TransactionCode`/amount cho role ngoài policy đã chốt.

## Handoff

Sau khi các câu hỏi validation được chốt, triển khai bằng:

`$ck-cook --hard --tdd plans/payment-transaction-management/plan.md`

## Session Notes
<!-- Updated by cook automatically — do not edit manually -->

**Last active:** 2026-07-21 14:00
**Phase in progress:** phase-05-verification-and-rollout
**Status:** Approved for commit/push by the user. Automated phase-05 gates pass: clean Release build, PostgreSQL suite 18/18, 10,001-row list p95 45.58 ms, and production dependency/bundle checks. All 10 test-project files are kept. Authenticated browser smoke is deferred to post-push validation because no browser instance was available.

### Decisions made this session

- Added a dedicated xUnit/WebApplicationFactory test project backed by an isolated real PostgreSQL database that is created, migrated, seeded and dropped per suite.
- Covered role/security contracts, list/detail/summary/export, combined filters, CSV/audit behavior, migration/indexes, COD/bank checkout, profile unpaid-order QR, underpayment and webhook idempotency.
- Reproduced concurrent webhook replay failures (7/8 responses were 500) and fixed them with PostgreSQL transaction advisory locks keyed by order and bank reference; concurrent regression now passes.
- Backfilled migrated `Payment.CreatedAt` values from `PaidAt` or `Order.CreatedAt` and used constant-time hashed SePay API-key comparison.
- Upgraded vulnerable `Microsoft.OpenApi` to 2.7.5 and React Router to 6.30.4; production dependency audits are clean.
- Removed all simulator code/copy and SePay dev-key references from the production FE bundle while retaining the development-only control.
- Documented deploy order, monitoring, feature-gating and rollback in `docs/PAYMENT_TRANSACTION_MANAGEMENT_ROLLOUT.md`.

### Next immediate action

After push/deploy, run the authenticated `/admin/payments` browser smoke and polling failure/recovery checks documented in the rollout runbook.

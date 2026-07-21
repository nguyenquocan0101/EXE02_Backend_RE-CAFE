# Spec: Quản lý giao dịch thanh toán

**Date:** 2026-07-21
**Status:** Draft

---

## Problem Statement

FE hiện đã hỗ trợ khách thanh toán theo từng đơn hàng bằng VietQR/SePay và hiển thị `paymentStatus`, nhưng admin chưa có màn hình quản lý các giao dịch thanh toán riêng. Dữ liệu backend đã lưu mã giao dịch và thời điểm thanh toán nhưng chưa có API/DTO để tra cứu, lọc và đối soát tập trung.

---

## User Stories

- **[P1]** As an admin/staff, I want to view a paginated list of customer payment transactions so that I can monitor payment activity without opening each order.
  Accepted when: the page shows order code, customer, payment method, amount, payment status, transaction code, and paid time; pagination works with at least 100 records.

- **[P1]** As an admin/staff, I want to search and filter transactions so that I can quickly find a payment by order, customer, transaction code, status, method, or date range.
  Accepted when: each filter can be combined with the others, the result count is accurate, and an empty result has a clear empty state.

- **[P1]** As an admin/staff, I want to open a transaction detail linked to its order so that I can verify the payment context and amount.
  Accepted when: detail shows payment ID, order ID/code, customer, order total, received amount, method, status, transaction code, paid time, and a link/action to view the related order.

- **[P1]** As a customer, I want my existing payment flow and order history to continue showing the correct payment status so that adding admin transaction management does not interrupt checkout.
  Accepted when: checkout, SePay webhook confirmation, polling, and the profile order-history payment action continue to work for COD and bank-transfer orders.

- **[P2]** As an admin, I want payment summary metrics and CSV export so that I can reconcile daily payment activity.
  Accepted when: the page can show paid/unpaid counts and paid amount for the selected filters, and export contains the same filtered rows.

- **[P3]** _(out of scope — noted for future)_ Support a full payment-attempt ledger with multiple partial payments, failed attempts, refunds, and chargebacks per order.

---

## Functional Requirements

1. **FR-01 — Payment data contract:** Add an admin payment DTO containing `id`, `orderId`, `orderCode`, `customerName`, `paymentMethod`, `paymentStatus`, `orderTotalAmount`, `amount`, `transactionCode`, `paidAt`, and `createdAt`. Dates are stored/transmitted in UTC and displayed in Asia/Ho_Chi_Minh.

2. **FR-02 — Payment list API:** Add `GET /api/admin/payments` with `page` (default 1), `pageSize` (default 20, maximum 100), `keyword`, `status`, `method`, `from`, and `to` query parameters. `keyword` searches order code, customer name, and transaction code. The response must contain `items`, `total`, `page`, `pageSize`, and `totalPages`.

3. **FR-03 — Payment detail API:** Add `GET /api/admin/payments/{id}`. Return the payment DTO plus the minimum related order/customer context needed by the detail view. Return 404 when the payment does not exist.

4. **FR-04 — Payment management screen:** Add an authenticated FE route `/admin/payments` with a table, loading state, error state, empty state, server-side pagination, keyword search, status filter, method filter, and date-range filter. The table must not load the entire payment history into the browser just to paginate locally.

5. **FR-05 — Transaction detail view:** From the payment table, admin/staff can open a detail drawer or modal. It must show the payment fields from FR-01 and provide navigation to the related order detail. It is read-only in MVP.

6. **FR-06 — Summary calculation:** Payment summaries must count only records with `Paid` status toward paid amount/revenue. `Unpaid` records must not increase paid revenue. Summary values must respect the active filters and date range.

7. **FR-07 — Payment status consistency:** SePay webhook processing remains the source of truth for automatic payment confirmation. Processing the same successful webhook more than once must not create a second successful payment record or double-count the amount.

8. **FR-08 — Authorization:** Payment list/detail APIs require authentication. Admin and Staff may view payment data according to the agreed role policy; no customer may access another customer's payment data. The FE route guard and backend policy must use the same role set.

9. **FR-09 — Production safety:** The browser must not contain a production SePay API key. The existing “simulate payment” control is available only in development/test builds, and production webhook credentials are kept server-side.

10. **FR-10 — Payment model readiness:** Add `CreatedAt` and database indexes needed for list/filter performance if they do not already exist. At minimum, index payment status, paid time, transaction code, and the order relationship.

11. **FR-11 — Customer-flow regression:** Existing checkout and profile flows continue to use order payment status. Adding the admin payment module must not change COD behavior, bank-transfer QR generation, SePay polling, or the “pay unpaid order” action.

12. **FR-12 — P2 reporting:** Add `GET /api/admin/payments/export` for filtered CSV export and expose summary cards only after the P1 list/detail/filter flow is complete. Export must use the same server-side filters, return UTF-8 CSV, be limited to 10,000 rows per request, and must not expose payment data to unauthorized users.

---

## Non-Functional Requirements

- Performance: `GET /api/admin/payments` p95 latency < 500 ms for 10,000 payment records with the maximum page size of 100; filtering must be server-side.
- Security: all admin payment endpoints require JWT authorization; production FE builds must not expose the SePay API key or raw webhook credentials; users can only access data permitted by their role.
- Availability: a payment-list API failure shows a recoverable error and retry action in FE; it must not block customer checkout or order-history rendering.
- Data integrity: duplicate webhook delivery must be idempotent; paid amount and payment status must remain consistent with the stored payment record.
- Compatibility: existing order/payment API response fields used by `Checkout.tsx` and `Profile.tsx` remain backward compatible.

---

## Success Criteria

- [ ] An authorized admin/staff user can open `/admin/payments` and see the first page of payment records with all required columns.
- [ ] Search and all P1 filters return correct server-side results and preserve pagination across at least 10,000 payment records.
- [ ] Opening a payment detail shows the related order and transaction fields without requiring a second manual lookup.
- [ ] Paid revenue summary excludes every `Unpaid` record and matches the filtered paid payment amounts exactly.
- [ ] Replaying the same SePay webhook does not create a duplicate successful payment or increase the paid summary twice.
- [ ] Existing checkout and profile payment flows pass regression tests for COD, unpaid bank transfer, successful SePay confirmation, and failed payment polling.
- [ ] A production FE build contains no sandbox/production SePay API key and does not render the simulator control.
- [ ] The payment list endpoint meets p95 < 500 ms for a seeded dataset of 10,000 payment records and page size 100.

---

## Out of Scope

- Manual editing of payment status or amount in the MVP.
- Refunds, chargebacks, cancellations after settlement, and payment dispute workflows.
- Supporting multiple payment attempts or partial payments per order as separate ledger entries.
- Adding a new payment provider beyond the existing SePay/bank-transfer flow.
- Storing and displaying the complete raw SePay webhook payload.
- Customer-facing payment-history UI separate from the existing order history.

---

## Assumptions

- The current domain treats one `Payment` record as the settlement record for one order; duplicate successful webhooks are ignored.
- `Admin` and `Staff` have read-only access to payment management. Only authorized roles can view or export payment data.
- Payment statuses remain `Unpaid` and `Paid` for this scope.
- The existing order owner/customer relationship is sufficient to identify the customer in the payment list.
- Server-side filtering and pagination are required because the existing admin order page currently loads the full order list and paginates locally.
- CSV export is CSV UTF-8, contains the active filtered columns, and is limited to 10,000 rows per request.
- Admin/staff payment list and export actions are recorded in the audit log with actor, action, filters, row count, and timestamp.

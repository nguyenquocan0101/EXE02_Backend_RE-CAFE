# Phase 03: FE admin payment management

## Goal

Thêm màn hình `/admin/payments` để Admin/Staff theo dõi giao dịch bằng dữ liệu server-side.

## Scope

- Tạo API client cho list/detail.
- Thêm route và navigation item trong admin layout.
- Table columns: order code, customer, method, amount, status, transaction code, paid time.
- Server-side pagination, keyword search, status/method/date filters.
- Loading, error + retry, empty, filter-reset và detail modal/drawer.
- Link từ payment detail tới order detail.

## Implementation Steps

1. Tạo typed response/query types trong `src/services/api`.
2. Tạo `AdminPayments.tsx` theo pattern AdminOrders nhưng không paginate local.
3. Thêm route `/admin/payments` và nav link; enforce Admin/Staff guard.
4. Chuẩn hóa currency/date/status labels và accessibility cho table/filter controls.
5. Debounce keyword query và reset page khi filter thay đổi.
6. Xử lý stale request/race condition khi filter nhanh hoặc đổi page.

## Tests to Write

- Route chỉ hiển thị cho authorized role.
- Query đúng params và không fetch toàn bộ history.
- Pagination/filter/search/detail/error/empty states.
- Amount/date/status render đúng theo API contract.
- Existing Checkout/Profile smoke tests không bị ảnh hưởng.

## Exit Criteria

- User có thể lọc và mở detail trong ≤ 3 thao tác sau khi vào trang.
- Không còn dependency vào local pagination cho payment history.
- FE build pass và route smoke test pass.

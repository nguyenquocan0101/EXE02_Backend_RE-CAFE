# Phase 04: Summary và CSV export

## Goal

Hoàn thiện P2 reporting sau khi P1 list/detail/filter đã ổn định.

## Scope

- Summary cards theo active filters: paid count, unpaid count, paid amount.
- `GET /api/admin/payments/export` dùng cùng filter contract.
- CSV UTF-8, tối đa 10.000 dòng/request, gồm các cột đang hiển thị và transaction fields.
- Audit log export với filter và row count.

## Implementation Steps

1. Tách query builder dùng chung cho list, summary và export để tránh lệch kết quả.
2. Thêm export response headers/filename và giới hạn row count.
3. Thêm nút export, disabled/loading state và lỗi export trong FE.
4. Verify Excel/Google Sheets mở đúng UTF-8 và currency/date format.

## Tests to Write

- Summary chỉ tính `Paid` và tôn trọng mọi filter.
- Export dùng cùng filter với table.
- Export chặn vượt 10.000 dòng và ghi audit log.
- Unauthorized export bị từ chối.

## Exit Criteria

- Summary khớp chính xác với filtered dataset.
- CSV mở được bằng spreadsheet phổ biến và không lộ dữ liệu ngoài policy.

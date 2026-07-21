# Phase 01: Payment domain và admin API

## Goal

Tạo nguồn dữ liệu/query API ổn định cho danh sách và chi tiết giao dịch thanh toán, không thay đổi hành vi checkout hiện tại.

## Scope

- Bổ sung `Payment.CreatedAt` và migration nếu chưa có.
- Thêm index cho status, paid time, transaction code và order relationship.
- Tạo `AdminPaymentDto`, query/filter model và paginated response.
- Implement `GET /api/admin/payments` và `GET /api/admin/payments/{id}`.
- Projection trực tiếp từ Payment → Order → User/shipping context; không load toàn bộ entity graph không cần thiết.

## Implementation Steps

1. Kiểm tra migration/schema hiện tại và cardinality Payment–Order.
2. Thêm field/index/migration theo backward-compatible strategy.
3. Chuẩn hóa enum-to-string cho method/status và date UTC.
4. Implement query với `page`, `pageSize <= 100`, `keyword`, `status`, `method`, `from`, `to`.
5. Trả `items`, `total`, `page`, `pageSize`, `totalPages`; detail trả 404 nếu không tồn tại.
6. Bổ sung OpenAPI/Swagger contract và sample response.

## Tests to Write

- List default pagination và page size cap.
- Search theo order code, customer name, transaction code.
- Kết hợp status/method/date range.
- Detail found/not-found.
- Không trả raw webhook payload hoặc dữ liệu ngoài DTO.
- Query dùng index/projection và không gây N+1 query.

## Exit Criteria

- API contract khớp FR-01–FR-03.
- Migration chạy được trên database hiện tại và rollback strategy được ghi nhận.
- Test list/detail/filter đạt 100% pass trong phase.

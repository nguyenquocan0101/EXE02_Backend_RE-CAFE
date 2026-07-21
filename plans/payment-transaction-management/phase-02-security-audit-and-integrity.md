# Phase 02: Security, audit và payment integrity

## Goal

Đảm bảo dữ liệu payment chỉ được truy cập đúng role, mọi thao tác quản trị được audit, và webhook không tạo duplicate settlement.

## Scope

- Cho phép `Admin` và `Staff` read-only trên payment endpoints; customer/anonymous bị từ chối.
- Ghi audit log cho list, detail và export với actor/action/filters/row count/timestamp.
- Kiểm tra idempotency của SePay webhook theo order/payment settlement.
- Loại bỏ API key khỏi client production; simulator chỉ được compile/render trong dev/test.
- Kiểm tra lỗi/response không làm lộ credential hoặc raw provider payload.

## Implementation Steps

1. Xác định cơ chế audit log hiện có và tạo service/helper dùng chung.
2. Áp policy role ở controller/service, sau đó cập nhật FE guard cùng role set.
3. Bọc webhook update trong transaction phù hợp và kiểm thử duplicate delivery.
4. Tách simulator khỏi production path; đọc credential chỉ từ server-side configuration.
5. Thêm security tests cho 401/403, role boundary và sensitive-field exposure.

## Tests to Write

- Admin/Staff được list/detail; Customer/anonymous nhận 401/403.
- Audit record đúng actor/action/filter/row count.
- Replay cùng webhook không tạo payment thứ hai hoặc double-count.
- Production build không chứa sandbox/production API key và không render simulator.

## Exit Criteria

- FE/BE role policy đồng nhất.
- Có audit evidence cho list/detail/export.
- Webhook idempotency test pass.

# FE Guide: Voucher Product Scope (Huong 2 - List ProductId)

## 1) Muc tieu

Tai lieu nay mo ta chi tiet luong FE khi user nhap voucher o checkout:
- Bam `Ap dung` -> goi API de kiem tra voucher.
- Neu hop le -> hien so tien giam ngay tren UI.
- Khi bam `Dat hang` -> backend tinh lai voucher de dam bao an toan.

Phuong an duoc chot: voucher ap dung theo danh sach san pham (`list productId`).

## 2) Trang thai backend hien tai

Da co:
1. `POST /api/orders/checkout` ho tro `couponCode`.
2. `POST /api/coupons/preview` cho FE bam `Ap dung`.
3. Coupon metadata:
   - `Scope` (`Order`, `Product`, `Category`)
   - `MaxDiscountAmount` cho voucher `%`
   - mapping `CouponProducts(CouponId, ProductId)`
4. Admin voucher CRUD:
   - `GET /api/admin/coupons`
   - `GET /api/admin/coupons/{id}`
   - `POST /api/admin/coupons`
   - `PUT /api/admin/coupons/{id}`
   - `DELETE /api/admin/coupons/{id}` (soft delete)

## 3) Thu tu API FE can goi

1. `POST /api/auth/login` (neu chua co token).
2. `GET /api/cart` de lay danh sach item, gia, tong tien.
3. User nhap ma voucher, bam `Ap dung`:
   - `POST /api/coupons/preview`.
4. User bam `Dat hang`:
   - `POST /api/orders/checkout` voi `couponCode` vua ap dung.
5. Sau khi dat hang thanh cong:
   - (tu chon) `GET /api/orders/{id}` hoac vao man chi tiet don.

## 4) Header chung

- `Authorization: Bearer <token>`
- `Content-Type: application/json`

## 5) API chi tiet

## 5.1 Login

Endpoint:
- `POST /api/auth/login`

Request:
```json
{
  "usernameOrEmail": "customer01",
  "password": "123456"
}
```

Response:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Login successful. Access token issued.",
  "action": "Login",
  "data": {
    "token": "<jwt-token>",
    "username": "customer01",
    "email": "customer01@example.com",
    "role": "Customer"
  },
  "timestamp": "2026-06-02T10:00:00Z",
  "path": "/api/auth/login",
  "traceId": "..."
}
```

## 5.2 Lay gio hang

Endpoint:
- `GET /api/cart`

Muc dich:
- Lay `cartItems` va `totalAmount`.
- FE dung `cartItem.id` de gui vao `cartItemIds` khi preview va checkout.

Response `data` rut gon:
```json
{
  "id": "cart-guid",
  "userId": "user-guid",
  "cartItems": [
    {
      "id": "cart-item-1",
      "productId": "product-1",
      "productName": "Eco Cup",
      "unitPrice": 120000,
      "quantity": 2,
      "totalPrice": 240000
    },
    {
      "id": "cart-item-2",
      "productId": "product-2",
      "productName": "Eco Bag",
      "unitPrice": 80000,
      "quantity": 1,
      "totalPrice": 80000
    }
  ],
  "totalAmount": 320000
}
```

## 5.3 Preview voucher

Endpoint:
- `POST /api/coupons/preview`

Request:
```json
{
  "couponCode": "SALE20",
  "cartItemIds": [
    "cart-item-1",
    "cart-item-2"
  ]
}
```

Quy tac tinh giam gia de xuat (Scope = Product):
1. `eligibleSubtotal` = tong `totalPrice` cua cac cart item co `productId` nam trong `CouponProducts`.
2. Neu coupon `%`:
   - `rawDiscount = eligibleSubtotal * (value / 100)`
   - `discountAmount = min(rawDiscount, maxDiscountAmount)` neu co `maxDiscountAmount`
3. Neu coupon giam tien truc tiep:
   - `discountAmount = min(value, eligibleSubtotal)`
4. `discountAmount` khong duoc am va khong vuot `eligibleSubtotal`.
5. Neu co `minimumOrderAmount`:
   - Kiem tra tren `eligibleSubtotal` (khuyen nghi cho product-scope).

Response hop le:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Coupon is valid and applied.",
  "action": "PreviewCoupon",
  "data": {
    "couponCode": "SALE20",
    "scope": "Product",
    "discountType": "Percentage",
    "discountValue": 20,
    "maxDiscountAmount": 50000,
    "eligibleSubtotal": 240000,
    "cartSubtotal": 320000,
    "discountAmount": 48000,
    "shippingFee": 0,
    "totalAfterDiscount": 272000,
    "applicableCartItemIds": ["cart-item-1"],
    "inapplicableCartItemIds": ["cart-item-2"]
  },
  "timestamp": "2026-06-02T10:05:00Z",
  "path": "/api/coupons/preview",
  "traceId": "..."
}
```

Response khong hop le (vi du):
```json
{
  "success": false,
  "statusCode": 400,
  "message": "The coupon code provided is invalid or inactive.",
  "action": "PreviewCoupon",
  "data": null,
  "timestamp": "2026-06-02T10:06:00Z",
  "path": "/api/coupons/preview",
  "traceId": "..."
}
```

Loi FE can hien message truc tiep:
1. `invalid or inactive`
2. `expired or is not yet active`
3. `has reached its maximum usage limit`
4. `subtotal must be at least ...`
5. `None of the selected items were found in your cart.`

## 5.4 Checkout (API da co)

Endpoint:
- `POST /api/orders/checkout`

Request:
```json
{
  "shippingAddress": {
    "receiverName": "Nguyen Van A",
    "phone": "0901234567",
    "province": "Ho Chi Minh",
    "district": "Quan 1",
    "ward": "Ben Nghe",
    "detailAddress": "123 Le Loi",
    "isDefault": false
  },
  "note": "Giao gio hanh chinh",
  "couponCode": "SALE20",
  "paymentMethod": 1,
  "cartItemIds": [
    "cart-item-1",
    "cart-item-2"
  ]
}
```

`paymentMethod` mapping:
1. `0` = `COD`
2. `1` = `BankTransfer`
3. `2` = `EWallet`
4. `3` = `OnlineGateway`

Response thanh cong:
- HTTP `201`.
- `data.discountAmount` la so tien giam cuoi cung backend chot.
- Neu `paymentMethod = BankTransfer` va chua thanh toan, co `paymentQrUrl`.

Luu y bat buoc:
1. FE chi dung so tien giam o UI de preview.
2. So tien chinh thuc phai lay tu response checkout.
3. Backend phai tinh lai voucher tai checkout (khong trust FE).

## 6) FE state machine de de implement

Khuyen nghi state:
1. `voucherInput`: string user dang nhap.
2. `voucherStatus`: `idle | checking | applied | invalid | stale`.
3. `voucherPreview`: object response tu `POST /api/coupons/preview`.
4. `selectedCartItemIds`: danh sach item user dang checkout.

Khi nao set `stale`:
1. User doi so luong item.
2. User xoa/them item.
3. User doi item duoc tick checkout.

Neu `stale`:
1. UI can thong bao "Voucher can ap dung lai".
2. Disable nut dat hang hoac tu dong preview lai truoc submit.

## 7) Trinh tu UX de xuat tai FE

1. Vao trang checkout -> goi `GET /api/cart`.
2. User nhap voucher -> bam `Ap dung`.
3. Goi `POST /api/coupons/preview`.
4. Neu hop le:
   - Hien discount amount.
   - Hien thong tin item duoc ap dung.
5. Neu khong hop le:
   - Hien message loi API.
   - Khong thay doi tong tien.
6. User bam `Dat hang`:
   - Goi `POST /api/orders/checkout` kem `couponCode`.
7. Neu checkout loi voucher:
   - Hien message backend.
   - Goi lai preview de user thay so tien moi.

## 8) Edge cases FE bat buoc xu ly

1. Voucher hop le luc preview nhung checkout bi fail do:
   - het luot su dung,
   - voucher het han trong khoang thoi gian user cho,
   - gio hang thay doi.
2. User submit checkout nhieu lan:
   - disable nut submit khi request dang chay.
3. Token het han (`401`):
   - redirect login, giu lai du lieu form checkout neu co.

## 9) Checklist handoff cho FE

1. Da luu va attach JWT token vao tat ca endpoint auth-required.
2. Da goi `GET /api/cart` de lay `cartItemIds` chinh xac.
3. Da goi `POST /api/coupons/preview` khi bam `Ap dung`.
4. Da gui `couponCode` vao `POST /api/orders/checkout`.
5. Da dung `discountAmount` trong response checkout lam gia tri cuoi cung.
6. Da xu ly day du cac message loi voucher.
7. Da xu ly case `paymentQrUrl` voi `BankTransfer`.

## 10) Ghi chu hien tai

Da implement:
1. `POST /api/coupons/preview`.
2. `CouponProducts` + `Scope` + `MaxDiscountAmount`.
3. Dung chung coupon calculation cho `preview` va `checkout`.

## 11) API Admin Voucher (chi tiet)

Auth:
1. Bat buoc `Bearer token`.
2. Role `Admin`.

### 11.1 Danh sach voucher

Endpoint:
- `GET /api/admin/coupons`

Query params:
1. `isActive` (bool, optional)
2. `scope` (int, optional) `0=Order, 1=Product, 2=Category`
3. `type` (int, optional) `0=Percentage, 1=FixedAmount, 2=FreeShipping`
4. `keyword` (string, optional, search theo `code`)

### 11.2 Chi tiet voucher

Endpoint:
- `GET /api/admin/coupons/{id}`

Response `data` co:
1. Thong tin voucher (`code`, `type`, `scope`, `value`, `maxDiscountAmount`, ...)
2. `applicableProducts` (list product duoc ap dung)

### 11.3 Tao voucher

Endpoint:
- `POST /api/admin/coupons`

Request mau:
```json
{
  "code": "RECAFE2026",
  "type": 0,
  "scope": 1,
  "value": 20,
  "maxDiscountAmount": 30000,
  "minimumOrderAmount": null,
  "usageLimit": 0,
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2027-12-31T23:59:59Z",
  "isActive": true,
  "productIds": [
    "11111111-1111-1111-1111-111111111111"
  ]
}
```

Rule:
1. `scope=1 (Product)` bat buoc co `productIds`.
2. `maxDiscountAmount` chi dung cho `type=0 (Percentage)`.
3. `endDate` phai lon hon `startDate`.

### 11.4 Cap nhat voucher

Endpoint:
- `PUT /api/admin/coupons/{id}`

Request body: giong `POST`.

Luu y:
1. Khi update, danh sach `productIds` se duoc dong bo lai theo request moi.

### 11.5 Xoa voucher

Endpoint:
- `DELETE /api/admin/coupons/{id}`

Hanh vi:
1. Soft delete (`IsActive=false`), khong xoa vat ly record.

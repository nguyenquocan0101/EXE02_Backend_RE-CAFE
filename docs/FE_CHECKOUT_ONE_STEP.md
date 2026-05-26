# FE Guide: One-Step Checkout API

## Muc tieu
- Cho phep user dat hang ngay tai trang checkout, khong can vao profile de tao dia chi truoc.

## Endpoint moi
- `POST /api/orders/checkout`
- Auth: `Bearer token` (bat buoc)
- Content-Type: `application/json`

## Request body
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
  "couponCode": "SALE10",
  "paymentMethod": 0,
  "cartItemIds": [
    "11111111-1111-1111-1111-111111111111",
    "22222222-2222-2222-2222-222222222222"
  ]
}
```

## PaymentMethod mapping
- `0`: `COD`
- `1`: `BankTransfer`
- `2`: `EWallet`
- `3`: `OnlineGateway`

## Response
- Status `201 Created` neu thanh cong.
- `data` tra ve `OrderDto` giong endpoint `POST /api/orders`.
- Co `shippingAddressId` va `shippingAddress` trong order response.
- Neu payment la `BankTransfer` va chua thanh toan, response co them `paymentQrUrl`.

## Flow FE de xuat
1. User nhap dia chi va thong tin checkout trong cung 1 form.
2. FE goi `POST /api/orders/checkout`.
3. Thanh cong:
   - Chuyen sang trang order success/detail.
   - Hien QR neu co `paymentQrUrl`.
4. That bai:
   - Hien message loi tu API.

## Luu y quan trong
- Dia chi duoc tao va gan vao user dang login (lay tu JWT), FE khong truyen `userId`.
- API duoc bao transaction cho luong checkout: tao dia chi + tao order la cung 1 khoi.
- Truong `isDefault` trong `shippingAddress` duoc xu ly nhu API tao dia chi hien tai.

# Hướng dẫn Test Certificate Snapshot Feature

## Bước 1: Restart Backend

```bash
# Trong terminal backend
# Nhấn Ctrl+C để dừng
# Chạy lại:
dotnet run
```

## Bước 2: Test cấp chứng nhận MỚI (có snapshot)

### Test 1: Cấp đơn lẻ

1. Đăng nhập tài khoản tổ chức
2. Vào sự kiện → Tab tình nguyện viên
3. Chọn TNV chưa có chứng nhận → Cấp chứng nhận
4. Kiểm tra DB:

```sql
SELECT
    MaGiayChungNhan,
    MaTNV,
    MaSuKien,
    LEN(CertificateData) AS DataLength,
    LEFT(CertificateData, 100) AS DataPreview
FROM GiayChungNhan
WHERE MaGiayChungNhan = [ID_vừa_cấp]
```

✅ **Kỳ vọng**: `CertificateData` NOT NULL, có JSON data

### Test 2: Cấp hàng loạt

1. Tương tự, nhưng chọn nhiều TNV và cấp hàng loạt
2. Kiểm tra DB xem tất cả đều có `CertificateData`

## Bước 3: Test XEM chứng nhận

### Test 3a: Xem chứng nhận MỚI (có snapshot)

1. Đăng nhập bằng tài khoản TNV vừa được cấp
2. Vào "Đăng ký của tôi" → Tab "Giấy chứng nhận"
3. Click "Xem" hoặc "Tải về"
4. Kiểm tra console backend:

```
Generating certificate X:
- TNV: [Tên]
- SuKien: [Tên sự kiện]
- ToChuc: [Tên tổ chức]
- Has CertificateData: True
- TemplateConfig: EXISTS
Using saved CertificateData snapshot for certificate X
```

✅ **Kỳ vọng**: Hiển thị đầy đủ thông tin, log "Using saved CertificateData snapshot"

### Test 3b: Xem chứng nhận CŨ (chưa có snapshot)

1. Dùng chứng nhận đã cấp trước đây (MaGiayChungNhan = 4 trong screenshot)
2. Click "Xem"
3. Kiểm tra console backend:

```
Generating certificate data dynamically for certificate 4
```

✅ **Kỳ vọng**: Vẫn hiển thị (fallback động), log "Generating... dynamically"

## Bước 4: Test tính bất biến

### Test 4: Đổi tên TNV/sự kiện

1. Xem chứng nhận của TNV A (có snapshot)
2. Lưu lại nội dung (tên, sự kiện, v.v.)
3. Vào DB đổi tên TNV hoặc tên sự kiện:

```sql
UPDATE TinhNguyenVien SET HoTen = 'TÊN MỚI' WHERE MaTNV = X
-- hoặc
UPDATE SuKien SET TenSuKien = 'SỰ KIỆN MỚI' WHERE MaSuKien = Y
```

4. Xem lại chứng nhận

✅ **Kỳ vọng**: Chứng nhận VẪN hiển thị tên CŨ (từ snapshot), không thay đổi

## Bước 5: Kiểm tra logs chi tiết

Trong console backend, bạn sẽ thấy:

- Khi cấp: JSON snapshot được tạo
- Khi xem: "Using saved CertificateData snapshot" hoặc "Generating... dynamically"
- Các field được draw: "Drawing field TenTNV: 'Nguyễn Văn A' at (...)"

## Kết quả mong đợi

✅ Chứng nhận mới: Có CertificateData, dùng snapshot  
✅ Chứng nhận cũ: Không có CertificateData, dùng dynamic generation  
✅ Dữ liệu bất biến: Thay đổi DB không ảnh hưởng chứng nhận đã cấp  
✅ Không có lỗi: Tất cả flow hoạt động bình thường

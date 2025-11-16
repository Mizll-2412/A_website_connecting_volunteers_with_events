# Hệ thống Chứng nhận Động (Dynamic Certificate System)

## Tổng quan
Hệ thống cho phép tổ chức tạo và quản lý mẫu chứng nhận với template có thể tùy chỉnh hoàn toàn. Chứng nhận được generate động dưới dạng ảnh PNG hoặc PDF.

## Tính năng chính

### 1. Template Editor (Tổ chức)
- Kéo thả các trường thông tin lên canvas
- Chỉnh sửa style: font, size, màu sắc, căn chỉnh
- Upload ảnh nền tùy chỉnh
- Preview real-time
- Lưu cấu hình template

### 2. Certificate Generation (Tự động)
- Generate ảnh PNG (300 DPI)
- Generate PDF
- Tự động fill dữ liệu từ database
- Hỗ trợ font tiếng Việt

### 3. Certificate Viewing (Tình nguyện viên)
- Xem preview chứng nhận
- Tải về dạng PNG hoặc PDF
- Chia sẻ chứng nhận

## Cài đặt & Triển khai

### Bước 1: Cài đặt Packages (Đã hoàn thành)
```bash
cd git_2-tulan88/A_website_connecting_volunteers_with_events
dotnet restore
```

Packages đã cài:
- SixLabors.ImageSharp 3.1.6
- SixLabors.ImageSharp.Drawing 2.1.5
- QuestPDF 2024.10.3

### Bước 2: Apply Migration
```bash
cd git_2-tulan88/A_website_connecting_volunteers_with_events
dotnet ef database update
```

### Bước 3: Seed Default Template
Chạy SQL script để thêm mẫu mặc định:
```bash
# Mở SQL Server Management Studio hoặc Azure Data Studio
# Kết nối tới database: tinhnguyen_db
# Mở và chạy file: Data/SeedDefaultCertificateTemplate.sql
```

### Bước 4: Chuẩn bị Font (QUAN TRỌNG)
Tạo thư mục fonts và thêm font:
```bash
# Trong thư mục wwwroot của backend
mkdir wwwroot/fonts
```

Tải và copy các file font vào `wwwroot/fonts/`:
- `Roboto-Regular.ttf`
- `Roboto-Bold.ttf`

Link tải font Roboto: https://fonts.google.com/specimen/Roboto

**LƯU Ý**: Nếu không có font, hệ thống sẽ báo lỗi khi generate chứng nhận.

### Bước 5: Khởi động ứng dụng
```bash
# Backend
cd git_2-tulan88/A_website_connecting_volunteers_with_events
dotnet run

# Frontend (terminal khác)
cd git_1-phanngochan18/A_website_connecting_volunteers_with_events_font_end
ng serve
```

## Hướng dẫn sử dụng

### Cho Tổ chức:

#### 1. Tạo/Chỉnh sửa mẫu chứng nhận
1. Đăng nhập với tài khoản tổ chức
2. Vào "Quản lý sự kiện" → Tab "Mẫu chứng nhận"
3. Click "Chỉnh sửa mẫu" hoặc "Tạo mẫu mới"
4. Sử dụng Template Editor:
   - Upload ảnh nền (khuyến nghị 1200x800px)
   - Click "Thêm trường" để thêm các trường thông tin
   - Kéo thả trường để di chuyển
   - Sử dụng panel bên phải để chỉnh style
5. Click "Lưu" để lưu cấu hình

#### 2. Cấp chứng nhận
**Cách 1: Cấp từng cái**
1. Vào chi tiết sự kiện
2. Tab "Tình nguyện viên"
3. Chọn TNV → Click "Cấp chứng nhận"
4. Chọn mẫu → Xem preview → Xác nhận

**Cách 2: Cấp hàng loạt**
1. Vào chi tiết sự kiện
2. Tab "Chứng nhận"
3. Chọn mẫu
4. Click "Cấp cho tất cả TNV đã duyệt"

### Cho Tình nguyện viên:

#### Xem chứng nhận đã nhận
1. Vào "Lịch sử sự kiện"
2. Tìm sự kiện có badge "Có chứng nhận"
3. Click "Xem chứng nhận"
4. Tải về PNG hoặc PDF

## API Endpoints

### Certificate Management
- `GET /api/certificate/{id}` - Lấy thông tin chứng nhận
- `GET /api/certificate/{id}/preview` - Preview chứng nhận (base64)
- `GET /api/certificate/{id}/download?format=image|pdf` - Tải về
- `POST /api/certificate` - Cấp chứng nhận
- `POST /api/certificate/events/{eventId}/issue-bulk/{templateId}` - Cấp hàng loạt

### Template Management
- `GET /api/certificate/samples` - Danh sách mẫu
- `GET /api/certificate/samples/{id}` - Chi tiết mẫu
- `POST /api/certificate/samples` - Tạo mẫu mới
- `POST /api/certificate/samples/{id}/config` - Lưu cấu hình
- `GET /api/certificate/samples/{id}/config` - Lấy cấu hình

## Cấu trúc Database

### Bảng MauGiayChungNhan (Certificate Templates)
- `MaMau` (int, PK): ID mẫu
- `TenMau` (nvarchar): Tên mẫu
- `MoTa` (nvarchar): Mô tả
- `IsDefault` (bit): Mẫu mặc định
- `TemplateConfig` (text): JSON config các trường
- `BackgroundImage` (nvarchar): Tên file ảnh nền
- `Width` (int): Chiều rộng (px)
- `Height` (int): Chiều cao (px)

### Bảng GiayChungNhan (Certificates)
- `MaGiayChungNhan` (int, PK): ID chứng nhận
- `MaMau` (int, FK): ID mẫu sử dụng
- `MaTNV` (int, FK): ID tình nguyện viên
- `MaSuKien` (int, FK): ID sự kiện
- `NgayCap` (datetime): Ngày cấp

## Template Config Format

```json
{
  "fields": [
    {
      "key": "TenTNV",           // Key của trường (predefined)
      "label": "Tên TNV",        // Label hiển thị
      "x": 600,                  // Tọa độ X
      "y": 300,                  // Tọa độ Y
      "fontSize": 24,            // Cỡ chữ
      "fontFamily": "Arial",     // Font chữ
      "color": "#000000",        // Màu (hex)
      "align": "center",         // Căn chỉnh: left|center|right
      "fontWeight": "bold"       // Độ đậm: normal|bold
    }
  ]
}
```

### Available Field Keys
- `TenTNV`: Tên tình nguyện viên
- `TenSuKien`: Tên sự kiện
- `TenToChuc`: Tên tổ chức
- `NgayCap`: Ngày cấp (dd/MM/yyyy)
- `ThoiGian`: Thời gian sự kiện (dd/MM/yyyy - dd/MM/yyyy)
- `DiaChi`: Địa điểm
- `SoGioThamGia`: Số giờ tham gia
- `MaChungNhan`: Mã chứng nhận (CERT-ID-YEAR)

## Troubleshooting

### 1. Lỗi "Font not found"
**Nguyên nhân**: Thiếu file font trong `wwwroot/fonts/`
**Giải pháp**: Tải và copy Roboto-Regular.ttf và Roboto-Bold.ttf vào thư mục

### 2. Ảnh nền không hiển thị
**Nguyên nhân**: File ảnh nền không tồn tại trong `wwwroot/uploads/`
**Giải pháp**: Upload lại ảnh nền hoặc kiểm tra đường dẫn

### 3. Preview trống
**Nguyên nhân**: Template config chưa được lưu hoặc sai format
**Giải pháp**: Mở Template Editor và lưu lại cấu hình

### 4. Cannot generate PDF
**Nguyên nhân**: QuestPDF license chưa được config
**Giải pháp**: Đã config Community license trong code, không cần thêm action

## Performance Notes

- Generate ảnh: ~1-2 giây/chứng nhận
- Generate PDF: ~2-3 giây/chứng nhận
- Cấp hàng loạt: Sequential processing (có thể cải thiện bằng parallel)
- Preview cache: Không cache (generate mỗi lần xem)

## Security Considerations

- Upload ảnh nền: Validate file type và size
- Template config: Validate JSON format
- Download: Kiểm tra quyền sở hữu chứng nhận
- Font files: Chỉ admin có quyền upload font mới

## Future Improvements

1. **Performance**:
   - Cache preview images
   - Parallel processing cho bulk issue
   - Background job cho generation

2. **Features**:
   - QR code trên chứng nhận
   - Watermark/security features
   - Multiple languages
   - Email delivery
   - Blockchain verification

3. **UI/UX**:
   - Undo/Redo trong editor
   - Layer management
   - Template marketplace
   - Mobile-friendly editor

## Liên hệ & Hỗ trợ

Nếu gặp vấn đề hoặc cần hỗ trợ, vui lòng:
1. Kiểm tra lại các bước cài đặt
2. Xem phần Troubleshooting
3. Liên hệ team phát triển

---
**Version**: 1.0.0  
**Last Updated**: 08/11/2024  
**Developed by**: Volunteer Management System Team


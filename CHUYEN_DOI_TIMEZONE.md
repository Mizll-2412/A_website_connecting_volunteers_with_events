# Hướng dẫn chuyển đổi múi giờ từ UTC sang Vietnam Time (UTC+7)

## Tổng quan

Để chuyển đổi múi giờ từ UTC sang Vietnam Time (UTC+7) mà không mất nhiều công code, chúng ta đã tạo `DateTimeHelper` class và cấu hình trong `Program.cs`.

## Cách sử dụng

### 1. Thay thế `DateTime.Now` bằng `DateTimeHelper.Now`

**Trước:**
```csharp
NgayTao = DateTime.Now
```

**Sau:**
```csharp
using khoaluantotnghiep.Helpers;

NgayTao = DateTimeHelper.Now
```

### 2. Thay thế `DateTime.UtcNow` bằng `DateTimeHelper.Now`

**Trước:**
```csharp
if (suKien.NgayKetThuc > DateTime.UtcNow)
```

**Sau:**
```csharp
using khoaluantotnghiep.Helpers;

if (suKien.NgayKetThuc > DateTimeHelper.Now)
```

### 3. Chuyển đổi DateTime từ UTC sang Vietnam Time

```csharp
DateTime vietnamTime = DateTimeHelper.ToVietnamTime(utcDateTime);
```

### 4. Chuyển đổi DateTime từ Vietnam Time sang UTC

```csharp
DateTime utcTime = DateTimeHelper.ToUtc(vietnamDateTime);
```

## Các file đã được cập nhật

1. ✅ `Helpers/DateTimeHelper.cs` - Helper class mới
2. ✅ `Program.cs` - Cấu hình timezone và culture
3. ✅ `Services/NotificationService.cs` - Đã thay thế `DateTime.Now`
4. ✅ `Services/RegistrationFormService.cs` - Đã thay thế `DateTime.Now`

## Các file cần cập nhật tiếp

Để tìm tất cả các chỗ cần thay thế, sử dụng lệnh sau trong terminal:

```powershell
# Tìm tất cả DateTime.Now
Select-String -Path "Services\*.cs" -Pattern "DateTime\.Now" | Select-Object Path, LineNumber, Line

# Tìm tất cả DateTime.UtcNow
Select-String -Path "Services\*.cs" -Pattern "DateTime\.UtcNow" | Select-Object Path, LineNumber, Line
```

Sau đó thay thế từng file:

1. Thêm `using khoaluantotnghiep.Helpers;` vào đầu file
2. Thay `DateTime.Now` → `DateTimeHelper.Now`
3. Thay `DateTime.UtcNow` → `DateTimeHelper.Now`

## Lưu ý quan trọng

⚠️ **Database vẫn lưu UTC**: `DateTimeHelper.Now` trả về Vietnam time, nhưng khi lưu vào database, Entity Framework sẽ tự động convert về UTC nếu cấu hình đúng. Đảm bảo database column có type `datetime` hoặc `datetime2`.

⚠️ **So sánh với database**: Khi so sánh với datetime từ database, cần chú ý:
- Database lưu UTC
- `DateTimeHelper.Now` trả về Vietnam time
- Cần convert về cùng timezone trước khi so sánh

**Ví dụ:**
```csharp
// ❌ Sai - so sánh trực tiếp
if (suKien.NgayKetThuc > DateTimeHelper.Now)

// ✅ Đúng - convert về cùng timezone
if (suKien.NgayKetThuc > DateTimeHelper.ToUtc(DateTimeHelper.Now))
// Hoặc
if (DateTimeHelper.ToVietnamTime(suKien.NgayKetThuc) > DateTimeHelper.Now)
```

## Tự động hóa (Tùy chọn)

Nếu muốn tự động thay thế tất cả, có thể sử dụng PowerShell script:

```powershell
# Script thay thế DateTime.Now
Get-ChildItem -Path "Services" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -match "DateTime\.Now|DateTime\.UtcNow") {
        $newContent = $content -replace "DateTime\.Now", "DateTimeHelper.Now"
        $newContent = $newContent -replace "DateTime\.UtcNow", "DateTimeHelper.Now"
        
        # Thêm using nếu chưa có
        if ($newContent -notmatch "using khoaluantotnghiep\.Helpers;") {
            $newContent = $newContent -replace "(using.*?;)", "`$1`nusing khoaluantotnghiep.Helpers;"
        }
        
        Set-Content -Path $_.FullName -Value $newContent -NoNewline
        Write-Host "Updated: $($_.FullName)"
    }
}
```

⚠️ **Lưu ý**: Chạy script trên sau khi đã backup code!


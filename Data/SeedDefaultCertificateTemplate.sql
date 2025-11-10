-- Script để thêm mẫu chứng nhận mặc định
-- Chạy script này sau khi đã apply migration AddCertificateTemplateFields

-- Kiểm tra nếu chưa có mẫu mặc định thì thêm vào
IF NOT EXISTS (SELECT 1 FROM MauGiayChungNhan WHERE IsDefault = 1)
BEGIN
    INSERT INTO MauGiayChungNhan (
        MaSuKien,
        TenMau,
        MoTa,
        IsDefault,
        NgayGui,
        [File],
        TemplateConfig,
        BackgroundImage,
        Width,
        Height
    )
    VALUES (
        NULL, -- Mẫu mặc định không gắn với sự kiện cụ thể
        N'Mẫu chứng nhận mặc định',
        N'Mẫu chứng nhận tình nguyện cơ bản với các trường thông tin chuẩn',
        1, -- IsDefault = true
        GETDATE(),
        NULL,
        N'{
            "fields": [
                {
                    "key": "TenSuKien",
                    "label": "Tên sự kiện",
                    "x": 600,
                    "y": 200,
                    "fontSize": 36,
                    "fontFamily": "Arial",
                    "color": "#1a5490",
                    "align": "center",
                    "fontWeight": "bold"
                },
                {
                    "key": "TenTNV",
                    "label": "Tên tình nguyện viên",
                    "x": 600,
                    "y": 320,
                    "fontSize": 32,
                    "fontFamily": "Arial",
                    "color": "#000000",
                    "align": "center",
                    "fontWeight": "bold"
                },
                {
                    "key": "TenToChuc",
                    "label": "Tên tổ chức",
                    "x": 600,
                    "y": 450,
                    "fontSize": 20,
                    "fontFamily": "Arial",
                    "color": "#333333",
                    "align": "center",
                    "fontWeight": "normal"
                },
                {
                    "key": "ThoiGian",
                    "label": "Thời gian sự kiện",
                    "x": 600,
                    "y": 500,
                    "fontSize": 18,
                    "fontFamily": "Arial",
                    "color": "#666666",
                    "align": "center",
                    "fontWeight": "normal"
                },
                {
                    "key": "DiaChi",
                    "label": "Địa điểm",
                    "x": 600,
                    "y": 540,
                    "fontSize": 18,
                    "fontFamily": "Arial",
                    "color": "#666666",
                    "align": "center",
                    "fontWeight": "normal"
                },
                {
                    "key": "NgayCap",
                    "label": "Ngày cấp",
                    "x": 900,
                    "y": 680,
                    "fontSize": 16,
                    "fontFamily": "Arial",
                    "color": "#000000",
                    "align": "right",
                    "fontWeight": "normal"
                },
                {
                    "key": "MaChungNhan",
                    "label": "Mã chứng nhận",
                    "x": 100,
                    "y": 750,
                    "fontSize": 14,
                    "fontFamily": "Courier New",
                    "color": "#999999",
                    "align": "left",
                    "fontWeight": "normal"
                }
            ]
        }',
        NULL, -- BackgroundImage - có thể để null hoặc thêm tên file ảnh nền mặc định
        1200, -- Width
        800  -- Height
    );
    
    PRINT 'Đã thêm mẫu chứng nhận mặc định thành công';
END
ELSE
BEGIN
    PRINT 'Mẫu chứng nhận mặc định đã tồn tại';
END
GO


-- Migration script để thêm cột MoTa vào bảng LinhVuc và KyNang
-- Chạy script này trực tiếp trên database nếu migration không hoạt động

-- Thêm cột MoTa vào bảng LinhVuc
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LinhVuc]') AND name = 'MoTa')
BEGIN
    ALTER TABLE [LinhVuc] ADD [MoTa] NVARCHAR(500) NULL;
    PRINT 'Đã thêm cột MoTa vào bảng LinhVuc';
END
ELSE
BEGIN
    PRINT 'Cột MoTa đã tồn tại trong bảng LinhVuc';
END

-- Thêm cột MoTa vào bảng KyNang
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[KyNang]') AND name = 'MoTa')
BEGIN
    ALTER TABLE [KyNang] ADD [MoTa] NVARCHAR(500) NULL;
    PRINT 'Đã thêm cột MoTa vào bảng KyNang';
END
ELSE
BEGIN
    PRINT 'Cột MoTa đã tồn tại trong bảng KyNang';
END

-- Đánh dấu migration đã được áp dụng (chỉ chạy nếu migration chưa có trong __EFMigrationsHistory)
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251110103233_AddMoTaToLinhVucAndKyNang')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251110103233_AddMoTaToLinhVucAndKyNang', '9.0.10');
    PRINT 'Đã đánh dấu migration đã được áp dụng';
END
ELSE
BEGIN
    PRINT 'Migration đã được đánh dấu trong __EFMigrationsHistory';
END


using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using SixLaborsImage = SixLabors.ImageSharp.Image;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ImageSharpColor = SixLabors.ImageSharp.Color;
using ImageSharpHorizontalAlignment = SixLabors.Fonts.HorizontalAlignment;

namespace khoaluantotnghiep.Services
{
    public class CertificateGeneratorService : ICertificateGeneratorService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CertificateGeneratorService> _logger;
        private Dictionary<string, string>? _snapshotData; // Lưu snapshot data tạm thời

        public CertificateGeneratorService(
            AppDbContext context,
            IWebHostEnvironment env,
            ILogger<CertificateGeneratorService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
            
            // Configure QuestPDF license (Community license)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<CertificateDataDto> GetCertificateDataAsync(int certificateId)
        {
            var certificate = await _context.GiayChungNhan
                .Include(c => c.TinhNguyenVien)
                    .ThenInclude(t => t!.TaiKhoan)
                .Include(c => c.SuKien)
                    .ThenInclude(s => s!.Organization)
                        .ThenInclude(tc => tc!.TaiKhoan)
                .FirstOrDefaultAsync(c => c.MaGiayChungNhan == certificateId);

            if (certificate == null)
                throw new Exception("Không tìm thấy giấy chứng nhận");

            // ƯU TIÊN: Nếu có CertificateData đã lưu, dùng snapshot đó
            if (!string.IsNullOrEmpty(certificate.CertificateData))
            {
                try
                {
                    var savedData = JsonSerializer.Deserialize<Dictionary<string, string>>(certificate.CertificateData);
                    if (savedData != null)
                    {
                        _logger.LogInformation($"Using saved CertificateData snapshot for certificate {certificateId}");
                        
                        // Tạo DTO đặc biệt: Các trường string đã được format sẵn trong snapshot
                        // Lưu vào các field tương ứng để GetFieldValue có thể dùng
                        var dto = new CertificateDataDto
                        {
                            TenTNV = savedData.GetValueOrDefault("tenTNV", "Tình nguyện viên"),
                            TenSuKien = savedData.GetValueOrDefault("tenSuKien", "Sự kiện"),
                            TenToChuc = savedData.GetValueOrDefault("tenToChuc", "Tổ chức"),
                            NgayCap = certificate.NgayCap ?? DateTime.Now,
                            DiaChi = savedData.GetValueOrDefault("diaChi", ""),
                            MaChungNhan = savedData.GetValueOrDefault("maChungNhan", $"CERT-{certificateId}"),
                            // Lưu các trường đã format sẵn vào NgayBatDau/NgayKetThuc (hack để GetFieldValue dùng)
                            NgayBatDau = null,
                            NgayKetThuc = null,
                            SoGioThamGia = null
                        };
                        
                        // Lưu các giá trị đã format vào một dictionary riêng để GetFieldValue dùng
                        // (Tạm thời dùng cách này, sau có thể refactor)
                        _snapshotData = savedData;
                        
                        return dto;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Không thể phân tích CertificateData, sẽ sử dụng phương thức tạo động: {ex.Message}");
                }
            }
            
            _snapshotData = null; // Clear snapshot data nếu không có

            // FALLBACK: Generate động từ dữ liệu hiện tại (cho chứng nhận cũ chưa có snapshot)
            _logger.LogInformation($"Generating certificate data dynamically for certificate {certificateId}");
            
            var ngayBatDau = certificate.SuKien?.NgayBatDau;
            var ngayKetThuc = certificate.SuKien?.NgayKetThuc ?? ngayBatDau;
            int? soGioThamGia = null;
            
            if (ngayBatDau != null && ngayKetThuc != null)
            {
                var timeSpan = ngayKetThuc.Value - ngayBatDau.Value;
                soGioThamGia = (int)Math.Ceiling(timeSpan.TotalHours);
            }

            return new CertificateDataDto
            {
                TenTNV = certificate.TinhNguyenVien?.HoTen ?? "Tình nguyện viên",
                TenSuKien = certificate.SuKien?.TenSuKien ?? "Sự kiện",
                TenToChuc = certificate.SuKien?.Organization?.TenToChuc ?? "Tổ chức",
                NgayCap = certificate.NgayCap ?? DateTime.Now,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                DiaChi = certificate.SuKien?.DiaChi,
                SoGioThamGia = soGioThamGia,
                MaChungNhan = $"CERT-{certificate.MaGiayChungNhan}-{certificate.NgayCap?.Year ?? DateTime.Now.Year}",
                LogoToChuc = null // Logo will be handled separately if needed
            };
        }

        public async Task<string> GeneratePreviewImageAsync(int certificateId)
        {
            var imageBytes = await GenerateCertificateImageAsync(certificateId);
            return Convert.ToBase64String(imageBytes);
        }

        public async Task<byte[]> GenerateCertificateImageAsync(int certificateId)
        {
            // Query certificate - KHÔNG CẦN query MauGiayChungNhan nữa!
            var certificate = await _context.GiayChungNhan
                .FirstOrDefaultAsync(c => c.MaGiayChungNhan == certificateId);

            if (certificate == null)
                throw new Exception("Không tìm thấy giấy chứng nhận");

            // Parse CertificateData (TemplateConfig đã điền data)
            if (string.IsNullOrEmpty(certificate.CertificateData))
                throw new Exception("Giấy chứng nhận chưa có dữ liệu");

            Dictionary<string, object>? filledConfig = null;
            List<Dictionary<string, object>>? fields = null;
            int width = 1200; // Default
            int height = 800; // Default

            try
            {
                filledConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(certificate.CertificateData);
                if (filledConfig != null && filledConfig.ContainsKey("fields"))
                {
                    var fieldsJson = JsonSerializer.Serialize(filledConfig["fields"]);
                    fields = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(fieldsJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi parse CertificateData: {ex.Message}");
                throw new Exception("Không thể parse dữ liệu chứng nhận");
            }

            if (fields == null || fields.Count == 0)
            {
                _logger.LogWarning($"Certificate {certificateId} has no fields in CertificateData");
                throw new Exception("Chứng nhận chưa có cấu hình trường dữ liệu");
            }

            _logger.LogInformation($"Generating certificate {certificateId} with {fields.Count} fields");

            // Load background image từ certificate
            Image<Rgba32> image;
            var backgroundPath = Path.Combine(_env.WebRootPath ?? "", "uploads", certificate.BackgroundImage ?? "");
            
            if (!string.IsNullOrEmpty(certificate.BackgroundImage) && File.Exists(backgroundPath))
            {
                image = await SixLaborsImage.LoadAsync<Rgba32>(backgroundPath);
                width = image.Width;
                height = image.Height;
                _logger.LogInformation($"Loaded background image: {certificate.BackgroundImage} ({width}x{height})");
            }
            else
            {
                // Fallback: Lấy từ mẫu nếu không có trong certificate
                var template = await _context.MauGiayChungNhan
                    .FirstOrDefaultAsync(m => m.MaMau == certificate.MaMau);
                
                if (template != null)
                {
                    width = template.Width;
                    height = template.Height;
                    backgroundPath = Path.Combine(_env.WebRootPath ?? "", "uploads", template.BackgroundImage ?? "");
                    
                    if (!string.IsNullOrEmpty(template.BackgroundImage) && File.Exists(backgroundPath))
                    {
                        image = await SixLaborsImage.LoadAsync<Rgba32>(backgroundPath);
                        if (image.Width != width || image.Height != height)
                        {
                            image.Mutate(x => x.Resize(width, height));
                        }
                        _logger.LogInformation($"Loaded background from template: {template.BackgroundImage}");
                    }
                    else
                    {
                        image = new Image<Rgba32>(width, height);
                        image.Mutate(x => x.BackgroundColor(ImageSharpColor.White));
                        _logger.LogWarning("No background image found, using white background");
                    }
                }
                else
                {
                    image = new Image<Rgba32>(width, height);
                    image.Mutate(x => x.BackgroundColor(ImageSharpColor.White));
                    _logger.LogWarning("No template found, using default white background");
                }
            }

            // Draw text fields từ CertificateData (đã có value sẵn)
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add(Path.Combine(_env.WebRootPath ?? "", "fonts", "Roboto-Regular.ttf"));
            var fontFamilyBold = fontCollection.Add(Path.Combine(_env.WebRootPath ?? "", "fonts", "Roboto-Bold.ttf"));

            image.Mutate(ctx =>
            {
                foreach (var fieldDict in fields)
                {
                    // Lấy các thuộc tính từ field
                    if (!fieldDict.ContainsKey("key") || !fieldDict.ContainsKey("value"))
                        continue;

                    var key = "";
                    if (fieldDict["key"] != null)
                    {
                        key = fieldDict["key"] is System.Text.Json.JsonElement keyElement
                            ? keyElement.GetString() ?? ""
                            : fieldDict["key"].ToString() ?? "";
                    }

                    var value = "";
                    if (fieldDict["value"] != null)
                    {
                        value = fieldDict["value"] is System.Text.Json.JsonElement valueElement
                            ? valueElement.GetString() ?? ""
                            : fieldDict["value"].ToString() ?? "";
                    }
                    
                    if (string.IsNullOrEmpty(value))
                    {
                        _logger.LogWarning($"Field {key} has empty value, skipping");
                        continue;
                    }

                    // Parse các thuộc tính (xử lý JsonElement)
                    float x = 0f;
                    if (fieldDict.ContainsKey("x") && fieldDict["x"] != null)
                    {
                        if (fieldDict["x"] is System.Text.Json.JsonElement xElement)
                            x = xElement.ValueKind == System.Text.Json.JsonValueKind.Number 
                                ? (float)xElement.GetSingle() 
                                : float.TryParse(xElement.GetString(), out var xVal) ? xVal : 0f;
                        else
                            float.TryParse(fieldDict["x"].ToString(), out x);
                    }

                    float y = 0f;
                    if (fieldDict.ContainsKey("y") && fieldDict["y"] != null)
                    {
                        if (fieldDict["y"] is System.Text.Json.JsonElement yElement)
                            y = yElement.ValueKind == System.Text.Json.JsonValueKind.Number 
                                ? (float)yElement.GetSingle() 
                                : float.TryParse(yElement.GetString(), out var yVal) ? yVal : 0f;
                        else
                            float.TryParse(fieldDict["y"].ToString(), out y);
                    }

                    int fontSize = 24;
                    if (fieldDict.ContainsKey("fontSize") && fieldDict["fontSize"] != null)
                    {
                        if (fieldDict["fontSize"] is System.Text.Json.JsonElement sizeElement)
                            fontSize = sizeElement.ValueKind == System.Text.Json.JsonValueKind.Number 
                                ? sizeElement.GetInt32() 
                                : int.TryParse(sizeElement.GetString(), out var sizeVal) ? sizeVal : 24;
                        else
                            int.TryParse(fieldDict["fontSize"].ToString(), out fontSize);
                    }

                    var fontFamilyName = "Arial";
                    if (fieldDict.ContainsKey("fontFamily") && fieldDict["fontFamily"] != null)
                    {
                        fontFamilyName = fieldDict["fontFamily"] is System.Text.Json.JsonElement fontElement
                            ? fontElement.GetString() ?? "Arial"
                            : fieldDict["fontFamily"].ToString() ?? "Arial";
                    }

                    var colorHex = "#000000";
                    if (fieldDict.ContainsKey("color") && fieldDict["color"] != null)
                    {
                        colorHex = fieldDict["color"] is System.Text.Json.JsonElement colorElement
                            ? colorElement.GetString() ?? "#000000"
                            : fieldDict["color"].ToString() ?? "#000000";
                    }

                    var align = "center";
                    if (fieldDict.ContainsKey("align") && fieldDict["align"] != null)
                    {
                        align = fieldDict["align"] is System.Text.Json.JsonElement alignElement
                            ? alignElement.GetString() ?? "center"
                            : fieldDict["align"].ToString() ?? "center";
                    }

                    var fontWeight = "normal";
                    if (fieldDict.ContainsKey("fontWeight") && fieldDict["fontWeight"] != null)
                    {
                        fontWeight = fieldDict["fontWeight"] is System.Text.Json.JsonElement weightElement
                            ? weightElement.GetString() ?? "normal"
                            : fieldDict["fontWeight"].ToString() ?? "normal";
                    }

                    var font = fontWeight == "bold" 
                        ? fontFamilyBold.CreateFont(fontSize, FontStyle.Bold)
                        : fontFamily.CreateFont(fontSize, FontStyle.Regular);

                    // Điều chỉnh Y coordinate để khớp với HTML Canvas textBaseline='top'
                    // HTML Canvas: textBaseline='top' -> Y là top của text
                    // ImageSharp: Origin là baseline (bottom của text)
                    // Dùng fontSize với hệ số điều chỉnh (có thể cần fine-tune)
                    // Hệ số 0.75-0.85 thường hoạt động tốt cho hầu hết font
                    var adjustedY = y + fontSize * 0.8f;
                    
                    // Log để debug và điều chỉnh nếu cần
                    _logger.LogInformation($"Y adjustment for {key}: fontSize={fontSize}, y={y} -> adjustedY={adjustedY:F2} (offset={fontSize * 0.8f:F2})");

                    var color = ParseColor(colorHex);
                    var textOptions = new RichTextOptions(font)
                    {
                        Origin = new PointF(x, adjustedY),
                        HorizontalAlignment = align switch
                        {
                            "left" => ImageSharpHorizontalAlignment.Left,
                            "right" => ImageSharpHorizontalAlignment.Right,
                            _ => ImageSharpHorizontalAlignment.Center
                        }
                    };

                    ctx.DrawText(textOptions, value, color);
                    _logger.LogInformation($"Drawing field {key}: '{value}' at ({x}, {y}) -> adjusted to ({x}, {adjustedY})");
                }
            });
            
            _logger.LogInformation($"Finished drawing {fields.Count} fields");

            // Convert to byte array
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> GenerateCertificatePdfAsync(int certificateId)
        {
            var imageBytes = await GenerateCertificateImageAsync(certificateId);
            
            // Lấy kích thước từ image đã generate
            using var imageStream = new MemoryStream(imageBytes);
            var imageInfo = await SixLaborsImage.IdentifyAsync(imageStream);
            
            if (imageInfo == null)
                throw new Exception("Không thể đọc thông tin ảnh");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Set page size based on image dimensions
                    float pageWidth = imageInfo.Width * 72f / 96f; // Convert pixels to points
                    float pageHeight = imageInfo.Height * 72f / 96f;
                    
                    page.Size(new PageSize(pageWidth, pageHeight));
                    page.Margin(0);
                    
                    page.Content().Image(imageBytes);
                });
            });

            return document.GeneratePdf();
        }

        private string GetFieldValue(CertificateDataDto data, string key)
        {
            // Nếu có snapshot data, ưu tiên dùng (đã format sẵn)
            if (_snapshotData != null)
            {
                var snapshotKey = key switch
                {
                    "TenTNV" => "tenTNV",
                    "TenSuKien" => "tenSuKien",
                    "TenToChuc" => "tenToChuc",
                    "NgayCap" => "ngayCap",
                    "NgayBatDau" => "ngayBatDau",
                    "NgayKetThuc" => "ngayKetThuc",
                    "ThoiGian" => "thoiGian",
                    "DiaChi" => "diaChi",
                    "SoGioThamGia" => "soGioThamGia",
                    "MaChungNhan" => "maChungNhan",
                    _ => null
                };
                
                if (snapshotKey != null && _snapshotData.ContainsKey(snapshotKey))
                {
                    return _snapshotData[snapshotKey];
                }
            }
            
            // Fallback: Generate động (cho chứng nhận cũ)
            return key switch
            {
                "TenTNV" => data.TenTNV,
                "TenSuKien" => data.TenSuKien,
                "TenToChuc" => data.TenToChuc,
                "NgayCap" => data.NgayCap.ToString("dd/MM/yyyy"),
                "NgayBatDau" => data.NgayBatDau?.ToString("dd/MM/yyyy") ?? "",
                "NgayKetThuc" => data.NgayKetThuc?.ToString("dd/MM/yyyy") ?? "",
                "ThoiGian" => data.NgayBatDau != null && data.NgayKetThuc != null 
                    ? $"{data.NgayBatDau.Value:dd/MM/yyyy} - {data.NgayKetThuc.Value:dd/MM/yyyy}" 
                    : "",
                "DiaChi" => data.DiaChi ?? "",
                "SoGioThamGia" => data.SoGioThamGia?.ToString() ?? "",
                "MaChungNhan" => data.MaChungNhan,
                _ => ""
            };
        }

        private ImageSharpColor ParseColor(string colorHex)
        {
            try
            {
                colorHex = colorHex.TrimStart('#');
                if (colorHex.Length == 6)
                {
                    byte r = Convert.ToByte(colorHex.Substring(0, 2), 16);
                    byte g = Convert.ToByte(colorHex.Substring(2, 2), 16);
                    byte b = Convert.ToByte(colorHex.Substring(4, 2), 16);
                    return ImageSharpColor.FromRgb(r, g, b);
                }
            }
            catch
            {
                _logger.LogWarning($"Không thể parse màu: {colorHex}");
            }
            return ImageSharpColor.Black;
        }
    }
}


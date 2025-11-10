using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CertificateService> _logger;
        private readonly string _uploadsFolder;

        public CertificateService(AppDbContext context, ILogger<CertificateService> logger)
        {
            _context = context;
            _logger = logger;
            // Folder lưu file chứng nhận
            _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "certificates");
            
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        // Các phương thức cho mẫu giấy chứng nhận
        public async Task<CertificateSampleDto> CreateCertificateSampleAsync(CreateCertificateSampleDto createDto)
        {
            try
            {
                SuKien? suKien = null;
                if (createDto.MaSuKien.HasValue)
                {
                    // Kiểm tra sự kiện có tồn tại không
                    suKien = await _context.Event.FindAsync(createDto.MaSuKien.Value);
                    if (suKien == null)
                    {
                        throw new KeyNotFoundException("Sự kiện không tồn tại");
                    }
                }

                // Upload file (optional)
                string? filePath = null;
                string? backgroundImageFileName = createDto.BackgroundImage;
                if (createDto.File != null)
                {
                    var ext = Path.GetExtension(createDto.File.FileName).ToLowerInvariant();
                    var imageExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                    if (imageExts.Contains(ext))
                    {
                        // Trường 'file' đang là ảnh nền -> lưu vào /wwwroot/uploads và set BackgroundImage
                        backgroundImageFileName = await UploadBackgroundImageAsync(createDto.File);
                    }
                    else
                    {
                        // Các định dạng khác (vd: PDF) được coi là file chứng nhận tĩnh
                        filePath = await UploadCertificateSampleFileAsync(createDto.File);
                    }
                }

                // Nếu đây là mẫu mặc định, bỏ default của mẫu khác
                if (createDto.IsDefault)
                {
                    var existingDefault = await _context.MauGiayChungNhan
                        .FirstOrDefaultAsync(m => m.IsDefault);
                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                    }
                }

                // Tạo mẫu giấy chứng nhận
                var certificateSample = new MauGiayChungNhan
                {
                    MaSuKien = createDto.MaSuKien,
                    TenMau = createDto.TenMau ?? "Mẫu giấy chứng nhận",
                    MoTa = createDto.MoTa,
                    IsDefault = createDto.IsDefault,
                    NgayGui = DateTime.Now,
                    File = filePath,
                    BackgroundImage = backgroundImageFileName
                };

                _context.MauGiayChungNhan.Add(certificateSample);
                await _context.SaveChangesAsync();

                return new CertificateSampleDto
                {
                    MaMau = certificateSample.MaMau,
                    MaSuKien = certificateSample.MaSuKien,
                    TenSuKien = suKien?.TenSuKien,
                    TenMau = certificateSample.TenMau,
                    MoTa = certificateSample.MoTa,
                    IsDefault = certificateSample.IsDefault,
                    NgayGui = certificateSample.NgayGui,
                    FilePath = certificateSample.File,
                    TemplateConfig = certificateSample.TemplateConfig,
                    BackgroundImage = certificateSample.BackgroundImage,
                    Width = certificateSample.Width,
                    Height = certificateSample.Height
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo mẫu giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        private async Task<string> UploadBackgroundImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Ảnh nền chỉ chấp nhận JPG, JPEG, PNG, GIF, BMP");
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Lưu BackgroundImage là tên file để phù hợp GeneratorService và FE
            return fileName;
        }

        public async Task<CertificateSampleDto> GetCertificateSampleByIdAsync(int maMau)
        {
            try
            {
                var certificateSample = await _context.MauGiayChungNhan
                    .Include(m => m.SuKien)
                    .FirstOrDefaultAsync(m => m.MaMau == maMau);

                if (certificateSample == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy mẫu giấy chứng nhận");
                }

                return new CertificateSampleDto
                {
                    MaMau = certificateSample.MaMau,
                    MaSuKien = certificateSample.MaSuKien,
                    TenSuKien = certificateSample.SuKien?.TenSuKien,
                    TenMau = certificateSample.TenMau,
                    MoTa = certificateSample.MoTa,
                    IsDefault = certificateSample.IsDefault,
                    NgayGui = certificateSample.NgayGui,
                    FilePath = certificateSample.File,
                    TemplateConfig = certificateSample.TemplateConfig,
                    BackgroundImage = certificateSample.BackgroundImage,
                    Width = certificateSample.Width,
                    Height = certificateSample.Height
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy mẫu giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CertificateSampleDto>> GetAllCertificateSamplesAsync()
        {
            try
            {
                var certificateSamples = await _context.MauGiayChungNhan
                    .Include(m => m.SuKien)
                    .OrderByDescending(m => m.IsDefault)
                    .ThenByDescending(m => m.NgayGui)
                    .ToListAsync();

                return certificateSamples.Select(m => new CertificateSampleDto
                {
                    MaMau = m.MaMau,
                    MaSuKien = m.MaSuKien,
                    TenSuKien = m.SuKien?.TenSuKien,
                    TenMau = m.TenMau,
                    MoTa = m.MoTa,
                    IsDefault = m.IsDefault,
                    NgayGui = m.NgayGui,
                    FilePath = m.File,
                    TemplateConfig = m.TemplateConfig,
                    BackgroundImage = m.BackgroundImage,
                    Width = m.Width,
                    Height = m.Height
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách tất cả mẫu giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CertificateSampleDto>> GetCertificateSamplesByEventAsync(int maSuKien)
        {
            try
            {
                var certificateSamples = await _context.MauGiayChungNhan
                    .Include(m => m.SuKien)
                    .Where(m => m.MaSuKien == maSuKien || m.IsDefault)
                    .OrderByDescending(m => m.IsDefault)
                    .ToListAsync();

                return certificateSamples.Select(m => new CertificateSampleDto
                {
                    MaMau = m.MaMau,
                    MaSuKien = m.MaSuKien,
                    TenSuKien = m.SuKien?.TenSuKien,
                    TenMau = m.TenMau,
                    MoTa = m.MoTa,
                    IsDefault = m.IsDefault,
                    NgayGui = m.NgayGui,
                    FilePath = m.File,
                    TemplateConfig = m.TemplateConfig,
                    BackgroundImage = m.BackgroundImage,
                    Width = m.Width,
                    Height = m.Height
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách mẫu giấy chứng nhận theo sự kiện: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteCertificateSampleAsync(int maMau)
        {
            try
            {
                var certificateSample = await _context.MauGiayChungNhan.FindAsync(maMau);
                if (certificateSample == null)
                {
                    return false;
                }

                // Cho phép xóa mẫu đã được sử dụng; ràng buộc FK sẽ SET NULL cho các chứng nhận cũ

                // Xóa file nếu tồn tại
                if (!string.IsNullOrEmpty(certificateSample.File))
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", certificateSample.File);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _context.MauGiayChungNhan.Remove(certificateSample);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa mẫu giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        public async Task<CertificateSampleDto> SetDefaultCertificateSampleAsync(int maMau)
        {
            try
            {
                var certificateSample = await _context.MauGiayChungNhan.FindAsync(maMau);
                if (certificateSample == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy mẫu giấy chứng nhận");
                }

                // Bỏ mặc định mẫu hiện tại (nếu có)
                var existingDefault = await _context.MauGiayChungNhan
                    .Where(m => m.IsDefault && m.MaMau != maMau)
                    .ToListAsync();
                foreach (var sample in existingDefault)
                {
                    sample.IsDefault = false;
                }

                certificateSample.IsDefault = true;
                await _context.SaveChangesAsync();

                return new CertificateSampleDto
                {
                    MaMau = certificateSample.MaMau,
                    MaSuKien = certificateSample.MaSuKien,
                    TenMau = certificateSample.TenMau,
                    MoTa = certificateSample.MoTa,
                    IsDefault = certificateSample.IsDefault,
                    NgayGui = certificateSample.NgayGui,
                    FilePath = certificateSample.File,
                    TemplateConfig = certificateSample.TemplateConfig,
                    BackgroundImage = certificateSample.BackgroundImage,
                    Width = certificateSample.Width,
                    Height = certificateSample.Height
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đặt mẫu chứng nhận mặc định: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadCertificateSampleFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File không hợp lệ");
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("Chỉ chấp nhận file PDF, JPG, JPEG hoặc PNG");
                }

                // Tạo tên file mới để tránh trùng lặp
                var fileName = $"certificate_sample_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối để lưu vào database
                return $"/uploads/certificates/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload file mẫu giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        // Các phương thức cho giấy chứng nhận cụ thể
        public async Task<CertificateDto> IssueCertificateAsync(IssueCertificateDto issueDto)
        {
            try
            {
                // Kiểm tra tình nguyện viên có tồn tại không
                var tinhNguyenVien = await _context.Volunteer
                    .Include(v => v.TaiKhoan)
                    .FirstOrDefaultAsync(v => v.MaTNV == issueDto.MaTNV);

                if (tinhNguyenVien == null)
                {
                    throw new KeyNotFoundException("Tình nguyện viên không tồn tại");
                }

                // Kiểm tra sự kiện có tồn tại không
                var suKien = await _context.Event
                    .Include(e => e.Organization)
                    .FirstOrDefaultAsync(e => e.MaSuKien == issueDto.MaSuKien);

                if (suKien == null)
                {
                    throw new KeyNotFoundException("Sự kiện không tồn tại");
                }

                // Kiểm tra mẫu giấy chứng nhận có tồn tại không
                var certificateSample = await _context.MauGiayChungNhan
                    .Include(m => m.SuKien)
                    .FirstOrDefaultAsync(m => m.MaMau == issueDto.MaMau);

                if (certificateSample == null)
                {
                    throw new KeyNotFoundException("Mẫu giấy chứng nhận không tồn tại");
                }

                // Kiểm tra TNV có tham gia sự kiện không
                var donDangKy = await _context.DonDangKy
                    .FirstOrDefaultAsync(d => d.MaTNV == issueDto.MaTNV && 
                                             d.MaSuKien == issueDto.MaSuKien &&
                                             d.TrangThai == 1); // Đã duyệt

                if (donDangKy == null)
                {
                    throw new InvalidOperationException("Tình nguyện viên chưa tham gia sự kiện này hoặc chưa được duyệt");
                }

                // Kiểm tra xem đã có giấy chứng nhận cho TNV và sự kiện này chưa
                var existingCertificate = await _context.GiayChungNhan
                    .Include(c => c.MauGiayChungNhan)
                    .FirstOrDefaultAsync(c => c.MaTNV == issueDto.MaTNV && 
                                             c.MaSuKien == issueDto.MaSuKien);

                if (existingCertificate != null)
                {
                    throw new InvalidOperationException("Đã tồn tại giấy chứng nhận cho tình nguyện viên này");
                }

                string filePath = null;
                if (issueDto.File != null)
                {
                    // Nếu có file riêng, upload file đó
                    filePath = await UploadCertificateFileAsync(issueDto.File);
                }
                else if (!string.IsNullOrEmpty(certificateSample.TemplateConfig))
                {
                    // Mẫu động (có TemplateConfig) - sẽ generate khi cần
                    // Không cần file tĩnh, set null và generate on-demand
                    filePath = null;
                }
                else
                {
                    // Mẫu tĩnh - cần file
                    filePath = certificateSample.File
                        ?? throw new InvalidOperationException("Mẫu giấy chứng nhận chưa có file đính kèm");
                }

                // Tạo snapshot dữ liệu tại thời điểm cấp
                var ngayCap = DateTime.Now;
                var ngayBatDau = suKien.NgayBatDau;
                var ngayKetThuc = suKien.NgayKetThuc ?? ngayBatDau;
                int? soGioThamGia = null;
                
                if (ngayBatDau != null && ngayKetThuc != null)
                {
                    var timeSpan = ngayKetThuc.Value - ngayBatDau.Value;
                    soGioThamGia = (int)Math.Ceiling(timeSpan.TotalHours);
                }

                // Chuẩn bị dữ liệu để điền vào template
                var dataValues = new Dictionary<string, string>
                {
                    { "TenTNV", tinhNguyenVien.HoTen ?? "Tình nguyện viên" },
                    { "TenSuKien", suKien.TenSuKien ?? "Sự kiện" },
                    { "TenToChuc", suKien.Organization?.TenToChuc ?? "Tổ chức" },
                    { "NgayCap", ngayCap.ToString("dd/MM/yyyy") },
                    { "NgayBatDau", ngayBatDau?.ToString("dd/MM/yyyy") ?? "" },
                    { "NgayKetThuc", ngayKetThuc?.ToString("dd/MM/yyyy") ?? "" },
                    { "ThoiGian", ngayBatDau != null && ngayKetThuc != null 
                        ? $"{ngayBatDau.Value:dd/MM/yyyy} - {ngayKetThuc.Value:dd/MM/yyyy}" 
                        : "" },
                    { "DiaChi", suKien.DiaChi ?? "" },
                    { "SoGioThamGia", soGioThamGia?.ToString() ?? "" },
                    { "MaChungNhan", "" } // Sẽ được set sau khi có ID
                };

                // Tạo giấy chứng nhận mới
                var certificate = new GiayChungNhan
                {
                    MaMau = issueDto.MaMau,
                    MaTNV = issueDto.MaTNV,
                    MaSuKien = issueDto.MaSuKien,
                    NgayCap = ngayCap,
                    File = filePath,
                    BackgroundImage = certificateSample.BackgroundImage, // Lưu ảnh nền
                    CertificateData = null // Sẽ update sau
                };

                _context.GiayChungNhan.Add(certificate);
                await _context.SaveChangesAsync();

                // Update mã chứng nhận
                var maChungNhan = $"CERT-{certificate.MaGiayChungNhan}-{ngayCap.Year}";
                dataValues["MaChungNhan"] = maChungNhan;

                // Tạo TemplateConfig đã điền data từ mẫu
                string? filledTemplateConfig = null;
                if (!string.IsNullOrEmpty(certificateSample.TemplateConfig))
                {
                    try
                    {
                        var templateConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(certificateSample.TemplateConfig);
                        if (templateConfig != null && templateConfig.ContainsKey("fields"))
                        {
                            var fieldsJson = System.Text.Json.JsonSerializer.Serialize(templateConfig["fields"]);
                            var fields = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(fieldsJson);
                            
                            if (fields != null)
                            {
                                // Điền value vào mỗi field
                                foreach (var field in fields)
                                {
                                    if (field.ContainsKey("key") && field["key"] != null)
                                    {
                                        var key = field["key"].ToString();
                                        if (dataValues.ContainsKey(key))
                                        {
                                            field["value"] = dataValues[key];
                                        }
                                    }
                                }
                                
                                // Tạo lại TemplateConfig với fields đã điền data
                                var filledConfig = new Dictionary<string, object>
                                {
                                    { "fields", fields }
                                };
                                
                                filledTemplateConfig = System.Text.Json.JsonSerializer.Serialize(filledConfig);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Cannot parse template config, will use simple data format: {ex.Message}");
                    }
                }

                // Nếu không có TemplateConfig hoặc parse lỗi, dùng format đơn giản
                if (string.IsNullOrEmpty(filledTemplateConfig))
                {
                    filledTemplateConfig = System.Text.Json.JsonSerializer.Serialize(new { fields = new List<object>() });
                }

                certificate.CertificateData = filledTemplateConfig;
                await _context.SaveChangesAsync();

                return new CertificateDto
                {
                    MaGiayChungNhan = certificate.MaGiayChungNhan,
                    MaMau = certificate.MaMau,
                    TenMau = certificateSample.TenMau,
                    MaTNV = certificate.MaTNV,
                    TenTNV = tinhNguyenVien.HoTen ?? "Tình nguyện viên",
                    MaSuKien = certificate.MaSuKien,
                    TenSuKien = suKien.TenSuKien,
                    TenToChuc = suKien.Organization?.TenToChuc,
                    NgayCap = certificate.NgayCap,
                    FilePath = certificate.File
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi phát hành giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        public async Task<CertificateDto> GetCertificateByIdAsync(int maGiayChungNhan)
        {
            try
            {
                var certificate = await _context.GiayChungNhan
                    .Include(c => c.TinhNguyenVien)
                    .Include(c => c.MauGiayChungNhan)
                    .ThenInclude(m => m.SuKien)
                    .Include(c => c.SuKien)
                    .ThenInclude(s => s.Organization)
                    .FirstOrDefaultAsync(c => c.MaGiayChungNhan == maGiayChungNhan);

                if (certificate == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy giấy chứng nhận");
                }

                // Lấy width/height từ template (fallback về default nếu không có)
                var width = certificate.MauGiayChungNhan?.Width ?? 1200;
                var height = certificate.MauGiayChungNhan?.Height ?? 800;

                return new CertificateDto
                {
                    MaGiayChungNhan = certificate.MaGiayChungNhan,
                    MaMau = certificate.MaMau,
                    MaTNV = certificate.MaTNV,
                    TenTNV = certificate.TinhNguyenVien?.HoTen,
                    MaSuKien = certificate.MaSuKien,
                    TenSuKien = certificate.SuKien?.TenSuKien ?? certificate.MauGiayChungNhan?.SuKien?.TenSuKien,
                    TenToChuc = certificate.SuKien?.Organization?.TenToChuc,
                    NgayCap = certificate.NgayCap,
                    FilePath = certificate.File,
                    CertificateData = certificate.CertificateData, // TemplateConfig đã điền data
                    BackgroundImage = certificate.BackgroundImage, // Ảnh nền
                    Width = width,
                    Height = height
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CertificateDto>> GetCertificatesByVolunteerAsync(int maTNV)
        {
            try
            {
                var certificates = await _context.GiayChungNhan
                    .Include(c => c.TinhNguyenVien)
                    .Include(c => c.MauGiayChungNhan)
                    .ThenInclude(m => m.SuKien)
                    .Include(c => c.SuKien)
                    .ThenInclude(s => s.Organization)
                    .Where(c => c.MaTNV == maTNV)
                    .ToListAsync();

                return certificates.Select(c => new CertificateDto
                {
                    MaGiayChungNhan = c.MaGiayChungNhan,
                    MaMau = c.MaMau,
                    MaTNV = c.MaTNV,
                    TenTNV = c.TinhNguyenVien?.HoTen,
                    MaSuKien = c.MaSuKien,
                    TenSuKien = c.SuKien?.TenSuKien ?? c.MauGiayChungNhan?.SuKien?.TenSuKien,
                    TenToChuc = c.SuKien?.Organization?.TenToChuc,
                    NgayCap = c.NgayCap,
                    FilePath = c.File
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách giấy chứng nhận theo tình nguyện viên: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CertificateDto>> GetCertificatesByEventAsync(int maSuKien)
        {
            try
            {
                var certificates = await _context.GiayChungNhan
                    .Include(c => c.TinhNguyenVien)
                    .Include(c => c.MauGiayChungNhan)
                    .ThenInclude(m => m.SuKien)
                    .Include(c => c.SuKien)
                    .ThenInclude(s => s.Organization)
                    .Where(c => c.MaSuKien == maSuKien)
                    .ToListAsync();

                return certificates.Select(c => new CertificateDto
                {
                    MaGiayChungNhan = c.MaGiayChungNhan,
                    MaMau = c.MaMau,
                    MaTNV = c.MaTNV,
                    TenTNV = c.TinhNguyenVien?.HoTen,
                    MaSuKien = c.MaSuKien,
                    TenSuKien = c.SuKien?.TenSuKien ?? c.MauGiayChungNhan?.SuKien?.TenSuKien,
                    TenToChuc = c.SuKien?.Organization?.TenToChuc,
                    NgayCap = c.NgayCap,
                    FilePath = c.File
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách giấy chứng nhận theo sự kiện: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CertificateDto>> GetCertificatesWithFilterAsync(CertificateFilterDto filter)
        {
            try
            {
                var query = _context.GiayChungNhan
                    .Include(c => c.TinhNguyenVien)
                    .Include(c => c.MauGiayChungNhan)
                    .ThenInclude(m => m.SuKien)
                    .Include(c => c.SuKien)
                    .ThenInclude(s => s.Organization)
                    .AsQueryable();

                // Áp dụng filter
                if (filter.MaSuKien.HasValue)
                {
                    query = query.Where(c => c.MaSuKien == filter.MaSuKien);
                }

                if (filter.MaTNV.HasValue)
                {
                    query = query.Where(c => c.MaTNV == filter.MaTNV);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(c => c.NgayCap >= filter.FromDate);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(c => c.NgayCap <= filter.ToDate);
                }

                var certificates = await query.ToListAsync();

                return certificates.Select(c => new CertificateDto
                {
                    MaGiayChungNhan = c.MaGiayChungNhan,
                    MaMau = c.MaMau,
                    MaTNV = c.MaTNV,
                    TenTNV = c.TinhNguyenVien?.HoTen,
                    MaSuKien = c.MaSuKien,
                    TenSuKien = c.SuKien?.TenSuKien ?? c.MauGiayChungNhan?.SuKien?.TenSuKien,
                    TenToChuc = c.SuKien?.Organization?.TenToChuc,
                    NgayCap = c.NgayCap,
                    FilePath = c.File
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách giấy chứng nhận theo filter: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteCertificateAsync(int maGiayChungNhan)
        {
            try
            {
                var certificate = await _context.GiayChungNhan.FindAsync(maGiayChungNhan);
                if (certificate == null)
                {
                    return false;
                }

                // Xóa file nếu tồn tại và không phải là file mẫu
                var mauFile = await _context.MauGiayChungNhan
                    .Where(m => m.MaMau == certificate.MaMau)
                    .Select(m => m.File)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(certificate.File) && certificate.File != mauFile)
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", certificate.File);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _context.GiayChungNhan.Remove(certificate);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadCertificateFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File không hợp lệ");
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("Chỉ chấp nhận file PDF, JPG, JPEG hoặc PNG");
                }

                // Tạo tên file mới để tránh trùng lặp
                var fileName = $"certificate_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối để lưu vào database
                return $"/uploads/certificates/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload file giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        // Phát hành hàng loạt
        public async Task<List<CertificateDto>> IssueCertificatesToEventParticipantsAsync(int maSuKien, int maMau)
        {
            try
            {
                // Kiểm tra sự kiện có tồn tại không
                var suKien = await _context.Event
                    .Include(e => e.Organization)
                    .FirstOrDefaultAsync(e => e.MaSuKien == maSuKien);

                if (suKien == null)
                {
                    throw new KeyNotFoundException("Sự kiện không tồn tại");
                }

                // Kiểm tra mẫu giấy chứng nhận có tồn tại không
                var certificateSample = await _context.MauGiayChungNhan
                    .Include(m => m.SuKien)
                    .FirstOrDefaultAsync(m => m.MaMau == maMau);

                if (certificateSample == null)
                {
                    throw new KeyNotFoundException("Mẫu giấy chứng nhận không tồn tại");
                }

                // Lấy danh sách TNV đã được duyệt tham gia sự kiện
                var approvedVolunteers = await _context.DonDangKy
                    .Include(d => d.TinhNguyenVien)
                    .ThenInclude(v => v.TaiKhoan)
                    .Where(d => d.MaSuKien == maSuKien && d.TrangThai == 1) // Đã duyệt
                    .ToListAsync();

                if (!approvedVolunteers.Any())
                {
                    throw new InvalidOperationException("Không có tình nguyện viên nào được duyệt tham gia sự kiện này");
                }

                var issuedCertificates = new List<CertificateDto>();
                var now = DateTime.Now;

                // Lấy danh sách TNV đã có giấy chứng nhận cho sự kiện này
                var existingCertificates = await _context.GiayChungNhan
                    .Where(c => c.MaSuKien == maSuKien)
                    .Select(c => c.MaTNV)
                    .ToListAsync();

                // Tính toán dữ liệu chung cho tất cả chứng nhận
                var ngayBatDau = suKien.NgayBatDau;
                var ngayKetThuc = suKien.NgayKetThuc ?? ngayBatDau;
                int? soGioThamGia = null;
                
                if (ngayBatDau != null && ngayKetThuc != null)
                {
                    var timeSpan = ngayKetThuc.Value - ngayBatDau.Value;
                    soGioThamGia = (int)Math.Ceiling(timeSpan.TotalHours);
                }

                foreach (var volunteer in approvedVolunteers)
                {
                    // Bỏ qua TNV đã có giấy chứng nhận
                    if (existingCertificates.Contains(volunteer.MaTNV))
                    {
                        continue;
                    }

                    // Chuẩn bị dữ liệu cho TNV này
                    var dataValues = new Dictionary<string, string>
                    {
                        { "TenTNV", volunteer.TinhNguyenVien?.HoTen ?? "Tình nguyện viên" },
                        { "TenSuKien", suKien.TenSuKien ?? "Sự kiện" },
                        { "TenToChuc", suKien.Organization?.TenToChuc ?? "Tổ chức" },
                        { "NgayCap", now.ToString("dd/MM/yyyy") },
                        { "NgayBatDau", ngayBatDau?.ToString("dd/MM/yyyy") ?? "" },
                        { "NgayKetThuc", ngayKetThuc?.ToString("dd/MM/yyyy") ?? "" },
                        { "ThoiGian", ngayBatDau != null && ngayKetThuc != null 
                            ? $"{ngayBatDau.Value:dd/MM/yyyy} - {ngayKetThuc.Value:dd/MM/yyyy}" 
                            : "" },
                        { "DiaChi", suKien.DiaChi ?? "" },
                        { "SoGioThamGia", soGioThamGia?.ToString() ?? "" },
                        { "MaChungNhan", "" } // Sẽ được set sau
                    };

                    // Tạo giấy chứng nhận mới
                    var certificate = new GiayChungNhan
                    {
                        MaMau = maMau,
                        MaTNV = volunteer.MaTNV,
                        MaSuKien = maSuKien,
                        NgayCap = now,
                        File = !string.IsNullOrEmpty(certificateSample.TemplateConfig) ? null : certificateSample.File,
                        BackgroundImage = certificateSample.BackgroundImage,
                        CertificateData = null // Sẽ update sau
                    };

                    _context.GiayChungNhan.Add(certificate);
                    await _context.SaveChangesAsync();

                    // Update mã chứng nhận
                    var maChungNhan = $"CERT-{certificate.MaGiayChungNhan}-{now.Year}";
                    dataValues["MaChungNhan"] = maChungNhan;

                    // Tạo TemplateConfig đã điền data
                    string? filledTemplateConfig = null;
                    if (!string.IsNullOrEmpty(certificateSample.TemplateConfig))
                    {
                        try
                        {
                            var templateConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(certificateSample.TemplateConfig);
                            if (templateConfig != null && templateConfig.ContainsKey("fields"))
                            {
                                var fieldsJson = System.Text.Json.JsonSerializer.Serialize(templateConfig["fields"]);
                                var fields = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(fieldsJson);
                                
                                if (fields != null)
                                {
                                    foreach (var field in fields)
                                    {
                                        if (field.ContainsKey("key") && field["key"] != null)
                                        {
                                            var key = field["key"].ToString();
                                            if (dataValues.ContainsKey(key))
                                            {
                                                field["value"] = dataValues[key];
                                            }
                                        }
                                    }
                                    
                                    var filledConfig = new Dictionary<string, object>
                                    {
                                        { "fields", fields }
                                    };
                                    
                                    filledTemplateConfig = System.Text.Json.JsonSerializer.Serialize(filledConfig);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Cannot parse template config for certificate {certificate.MaGiayChungNhan}: {ex.Message}");
                        }
                    }

                    if (string.IsNullOrEmpty(filledTemplateConfig))
                    {
                        filledTemplateConfig = System.Text.Json.JsonSerializer.Serialize(new { fields = new List<object>() });
                    }

                    certificate.CertificateData = filledTemplateConfig;
                    await _context.SaveChangesAsync();

                    issuedCertificates.Add(new CertificateDto
                    {
                        MaGiayChungNhan = certificate.MaGiayChungNhan,
                        MaMau = certificate.MaMau,
                        TenMau = certificateSample.TenMau,
                        MaTNV = certificate.MaTNV,
                        TenTNV = volunteer.TinhNguyenVien?.HoTen ?? "Tình nguyện viên",
                        MaSuKien = maSuKien,
                        TenSuKien = suKien.TenSuKien,
                        TenToChuc = suKien.Organization?.TenToChuc,
                        NgayCap = certificate.NgayCap,
                        FilePath = certificate.File
                    });
                }

                return issuedCertificates;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi phát hành hàng loạt giấy chứng nhận: {ex.Message}");
                throw;
            }
        }

        public async Task<CertificateSampleDto> UpdateTemplateConfigAsync(int maMau, TemplateConfigDto configDto)
        {
            try
            {
                var template = await _context.MauGiayChungNhan.FindAsync(maMau);
                if (template == null)
                    throw new KeyNotFoundException("Mẫu chứng nhận không tồn tại");

                template.TemplateConfig = configDto.TemplateConfig;
                template.BackgroundImage = configDto.BackgroundImage ?? template.BackgroundImage;
                template.Width = configDto.Width > 0 ? configDto.Width : template.Width;
                template.Height = configDto.Height > 0 ? configDto.Height : template.Height;

                await _context.SaveChangesAsync();

                return new CertificateSampleDto
                {
                    MaMau = template.MaMau,
                    MaSuKien = template.MaSuKien,
                    TenMau = template.TenMau,
                    MoTa = template.MoTa,
                    IsDefault = template.IsDefault,
                    NgayGui = template.NgayGui,
                    FilePath = template.File,
                    TemplateConfig = template.TemplateConfig,
                    BackgroundImage = template.BackgroundImage,
                    Width = template.Width,
                    Height = template.Height
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật cấu hình template: {ex.Message}");
                throw;
            }
        }

        public async Task<TemplateConfigDto> GetTemplateConfigAsync(int maMau)
        {
            try
            {
                var template = await _context.MauGiayChungNhan.FindAsync(maMau);
                if (template == null)
                    throw new KeyNotFoundException("Mẫu chứng nhận không tồn tại");

                return new TemplateConfigDto
                {
                    TemplateConfig = template.TemplateConfig,
                    BackgroundImage = template.BackgroundImage,
                    Width = template.Width,
                    Height = template.Height
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy cấu hình template: {ex.Message}");
                throw;
            }
        }
    }
}

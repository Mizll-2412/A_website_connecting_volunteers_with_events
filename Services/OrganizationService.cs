using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrganizationService> _logger;
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 5242880; // 5MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public OrganizationService(AppDbContext context, ILogger<OrganizationService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        public async Task<ToChucResponseDto> GetToChucAsync(int maToChuc)
        {
            try
            {
                var toChuc = await _context.Organization
                    .FirstOrDefaultAsync(t => t.MaToChuc == maToChuc);

                if (toChuc == null)
                {
                    throw new Exception("Tổ chức không tồn tại");
                }

                return new ToChucResponseDto
                {
                    MaToChuc = toChuc.MaToChuc,
                    MaTaiKhoan = toChuc.MaTaiKhoan,
                    TenToChuc = toChuc.TenToChuc,
                    Email = toChuc.Email,
                    SoDienThoai = toChuc.SoDienThoai,
                    DiaChi = toChuc.DiaChi,
                    NgayTao = toChuc.NgayTao,
                    GioiThieu = toChuc.GioiThieu,
                    DiemTrungBinh = toChuc.DiemTrungBinh,
                    AnhDaiDien = toChuc.AnhDaiDien,
                    TrangThaiXacMinh = toChuc.TrangThaiXacMinh,
                    LyDoTuChoi = toChuc.LyDoTuChoi,
                    GiayToPhapLyIds = toChuc.GiayToPhapLys?.Select(g => g.MaGiayTo).ToList() ?? new List<int>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy tổ chức: {ex.Message}");
                throw;
            }
        }

        public async Task<ToChucResponseDto> UpdateToChucAsync(int maToChuc, UpdateToChucDto updateDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var toChuc = await _context.Organization.FindAsync(maToChuc);
                    if (toChuc == null)
                    {
                        throw new Exception("Tổ chức không tồn tại");
                    }

                    toChuc.TenToChuc = updateDto.TenToChuc ?? toChuc.TenToChuc;
                    toChuc.Email = updateDto.Email; // Email bắt buộc
                    toChuc.SoDienThoai = updateDto.SoDienThoai ?? toChuc.SoDienThoai;
                    toChuc.DiaChi = updateDto.DiaChi ?? toChuc.DiaChi;
                    toChuc.GioiThieu = updateDto.GioiThieu ?? toChuc.GioiThieu;
                    toChuc.AnhDaiDien = updateDto.AnhDaiDien ?? toChuc.AnhDaiDien;

                    var taiKhoan = await _context.User
                        .FirstOrDefaultAsync(t => t.MaTaiKhoan == toChuc.MaTaiKhoan);

                    if (taiKhoan != null)
                    {
                        taiKhoan.Email = updateDto.Email;
                        _context.User.Update(taiKhoan);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Trả về đúng dữ liệu
                    return new ToChucResponseDto
                    {
                        MaToChuc = toChuc.MaToChuc,
                        MaTaiKhoan = toChuc.MaTaiKhoan,
                        TenToChuc = toChuc.TenToChuc,
                        Email = toChuc.Email,
                        SoDienThoai = toChuc.SoDienThoai,
                        DiaChi = toChuc.DiaChi,
                        NgayTao = toChuc.NgayTao,
                        GioiThieu = toChuc.GioiThieu,
                        DiemTrungBinh = toChuc.DiemTrungBinh,
                        AnhDaiDien = toChuc.AnhDaiDien,
                        GiayToPhapLyIds = toChuc.GiayToPhapLys?.Select(g => g.MaGiayTo).ToList() ?? new List<int>()
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi cập nhật tổ chức: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<string> UploadAnhDaiDienAsync(int maToChuc, IFormFile anhFile)
        {
            try
            {
                if (anhFile == null || anhFile.Length == 0)
                    throw new Exception("Vui lòng chọn file ảnh");

                if (anhFile.Length > MaxFileSize)
                    throw new Exception("File ảnh quá lớn (tối đa 5MB)");

                var fileExtension = Path.GetExtension(anhFile.FileName).ToLower();
                if (!AllowedExtensions.Contains(fileExtension))
                    throw new Exception("Định dạng file không hỗ trợ (chỉ JPG, PNG, GIF)");

                var toChuc = await _context.Organization.FindAsync(maToChuc);
                if (toChuc == null)
                    throw new Exception("Tổ chức không tồn tại");

                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "organizations"); if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                _logger.LogInformation($"UploadPath: {uploadPath}");
                if (!string.IsNullOrEmpty(toChuc.AnhDaiDien))
                {
                    var oldFilePath = Path.Combine(webRootPath, toChuc.AnhDaiDien.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }
                var fileName = $"{maToChuc}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await anhFile.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/organizations/{fileName}";
                toChuc.AnhDaiDien = imageUrl;

                await _context.SaveChangesAsync();

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload ảnh: {ex.Message}");
                throw;
            }
        }

        public async Task<ToChucResponseDto> CreateToChucAsync(CreateToChucDto createDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var taiKhoan = await _context.User.FindAsync(createDto.MaTaiKhoan);
                    if (taiKhoan == null)
                        throw new Exception("Tài khoản không tồn tại");
                    var existingTNV = await _context.Volunteer
                        .FirstOrDefaultAsync(t => t.MaTaiKhoan == createDto.MaTaiKhoan);

                    if (existingTNV != null)
                        throw new Exception("Tài khoản này đã có hồ sơ tình nguyện viên");
                    var toChuc = new ToChuc
                    {
                        MaTaiKhoan = createDto.MaTaiKhoan,
                        TenToChuc = createDto.TenToChuc,
                        Email = createDto.Email,
                        DiaChi = createDto.DiaChi,
                        GioiThieu = createDto.GioiThieu,
                        AnhDaiDien = createDto.AnhDaiDien
                    };

                    _context.Organization.Add(toChuc);
                    await _context.SaveChangesAsync();

                    if (createDto.GiayToPhapLyIds != null && createDto.GiayToPhapLyIds.Count > 0)
                    {
                        var giayTos = await _context.GiayToPhapLy
                            .Where(g => createDto.GiayToPhapLyIds.Contains(g.MaGiayTo))
                            .ToListAsync();

                        foreach (var g in giayTos)
                        {
                            toChuc.GiayToPhapLys.Add(g);
                        }

                        await _context.SaveChangesAsync();
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetToChucAsync(toChuc.MaToChuc);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi tạo tình nguyện viên: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<List<ToChucResponseDto>> GetAllToChucAsync()
        {
            try
            {
                var toChucs = await _context.Organization
                    .Include(t => t.GiayToPhapLys)

                    .ToListAsync();

                return toChucs.Select(t => new ToChucResponseDto
                {
                    MaToChuc = t.MaToChuc,
                    MaTaiKhoan = t.MaTaiKhoan,
                    TenToChuc = t.TenToChuc,
                    Email = t.Email,
                    SoDienThoai = t.SoDienThoai,
                    DiaChi = t.DiaChi,
                    NgayTao = t.NgayTao,
                    GioiThieu = t.GioiThieu,
                    AnhDaiDien = t.AnhDaiDien,
                    DiemTrungBinh = t.DiemTrungBinh,
                    TrangThaiXacMinh = t.TrangThaiXacMinh,
                    LyDoTuChoi = t.LyDoTuChoi,
                    GiayToPhapLyIds = t.GiayToPhapLys?.Select(l => l.MaGiayTo).ToList() ?? new List<int>()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách tổ chức: {ex.Message}");
                throw;
            }
        }

        public async Task<ToChucResponseDto> GetToChucByMaTaiKhoanAsync(int maTaiKhoan)
        {
            try
            {
                var toChuc = await _context.Organization
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

                if (toChuc == null)
                {
                    throw new Exception("Không tìm thấy tổ chức với mã tài khoản này");
                }

                return new ToChucResponseDto
                {
                    MaToChuc = toChuc.MaToChuc,
                    MaTaiKhoan = toChuc.MaTaiKhoan,
                    TenToChuc = toChuc.TenToChuc,
                    Email = toChuc.Email,
                    SoDienThoai = toChuc.SoDienThoai,
                    DiaChi = toChuc.DiaChi,
                    NgayTao = toChuc.NgayTao,
                    GioiThieu = toChuc.GioiThieu,
                    DiemTrungBinh = toChuc.DiemTrungBinh,
                    AnhDaiDien = toChuc.AnhDaiDien,
                    TrangThaiXacMinh = toChuc.TrangThaiXacMinh,
                    LyDoTuChoi = toChuc.LyDoTuChoi,
                    GiayToPhapLyIds = toChuc.GiayToPhapLys?.Select(g => g.MaGiayTo).ToList() ?? new List<int>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy tổ chức theo mã tài khoản: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteToChucAsync(int maToChuc)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var tochuc = await _context.Organization
                        .Include(t => t.GiayToPhapLys)
                        .FirstOrDefaultAsync(t => t.MaToChuc == maToChuc);

                    if (tochuc == null)
                        throw new Exception("Tổ chức không tồn tại");
                    _context.GiayToPhapLy.RemoveRange(tochuc.GiayToPhapLys);

                    if (!string.IsNullOrEmpty(tochuc.AnhDaiDien))
                    {
                        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var filePath = Path.Combine(webRootPath, tochuc.AnhDaiDien.TrimStart('/'));
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    _context.Organization.Remove(tochuc);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi xóa tổ chức: {ex.Message}");
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Tổ chức gửi yêu cầu xác minh
        /// </summary>
        public async Task<VerificationStatusResponseDto> RequestVerificationAsync(RequestVerificationDto requestDto)
        {
            try
            {
                var toChuc = await _context.Organization
                    .Include(t => t.GiayToPhapLys)
                    .FirstOrDefaultAsync(t => t.MaToChuc == requestDto.MaToChuc);
                    
                if (toChuc == null)
                {
                    throw new Exception("Tổ chức không tồn tại");
                }
                
                // Kiểm tra xem có giấy tờ pháp lý không
                if (toChuc.GiayToPhapLys == null || !toChuc.GiayToPhapLys.Any())
                {
                    throw new Exception("Cần tải lên ít nhất một giấy tờ pháp lý để yêu cầu xác minh");
                }
                
                // Nếu đã xác minh rồi
                if (toChuc.TrangThaiXacMinh == 1)
                {
                    throw new Exception("Tổ chức đã được xác minh");
                }
                
                // Cập nhật trạng thái xác minh
                toChuc.TrangThaiXacMinh = 2; // 2 = Đang chờ xác minh
                
                await _context.SaveChangesAsync();
                
                return await GetVerificationStatusAsync(requestDto.MaToChuc);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi yêu cầu xác minh: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Lấy thông tin trạng thái xác minh của tổ chức
        /// </summary>
        public async Task<VerificationStatusResponseDto> GetVerificationStatusAsync(int maToChuc)
        {
            try
            {
                var toChuc = await _context.Organization
                    .FirstOrDefaultAsync(t => t.MaToChuc == maToChuc);
                    
                if (toChuc == null)
                {
                    throw new Exception("Tổ chức không tồn tại");
                }
                
                string trangThaiText = "Chưa xác minh";
                switch (toChuc.TrangThaiXacMinh)
                {
                    case 1:
                        trangThaiText = "Đã xác minh";
                        break;
                    case 2:
                        trangThaiText = "Đang chờ xác minh";
                        break;
                    case 3:
                        trangThaiText = "Đã từ chối";
                        break;
                    default:
                        trangThaiText = "Chưa xác minh";
                        break;
                }
                
                return new VerificationStatusResponseDto
                {
                    MaToChuc = toChuc.MaToChuc,
                    TenToChuc = toChuc.TenToChuc,
                    TrangThaiXacMinh = toChuc.TrangThaiXacMinh,
                    TrangThaiXacMinhText = trangThaiText,
                    LyDoTuChoi = toChuc.LyDoTuChoi,
                    DaGuiYeuCauXacMinh = toChuc.TrangThaiXacMinh > 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thông tin xác minh: {ex.Message}");
                throw;
            }
        }
    }
}
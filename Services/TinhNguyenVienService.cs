using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;

namespace khoaluantotnghiep.Services
{
    public class TinhNguyenVienService : ITinhNguyenVienService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TinhNguyenVienService> _logger;
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 5242880;
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public TinhNguyenVienService(AppDbContext context, ILogger<TinhNguyenVienService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        // Helper method để format DateTime thành string yyyy-MM-dd
        private string? FormatDateForResponse(DateTime? date)
        {
            if (date == null) return null;
            return date.Value.ToString("yyyy-MM-dd");
        }

        public async Task<TinhNguyenVienResponseDto> GetTinhNguyenVienAsync(int maTNV)
        {
            try
            {
                var tinhNguyenVien = await _context.Volunteer
                    .Include(t => t.TinhNguyenVien_LinhVucs)
                    .Include(t => t.TinhNguyenVien_KyNangs)
                    .FirstOrDefaultAsync(t => t.MaTNV == maTNV);

                if (tinhNguyenVien == null)
                {
                    throw new Exception("Tình nguyện viên không tồn tại");
                }

                var linhVucIds = tinhNguyenVien.TinhNguyenVien_LinhVucs?
                    .Select(t => t.MaLinhVuc).ToList();

                var kyNangIds = tinhNguyenVien.TinhNguyenVien_KyNangs?
                    .Select(t => t.MaKyNang).ToList();

                return new TinhNguyenVienResponseDto
                {
                    MaTNV = tinhNguyenVien.MaTNV,
                    MaTaiKhoan = tinhNguyenVien.MaTaiKhoan,
                    HoTen = tinhNguyenVien.HoTen ?? string.Empty,
                    NgaySinh = FormatDateForResponse(tinhNguyenVien.NgaySinh),
                    GioiTinh = tinhNguyenVien.GioiTinh,
                    Email = tinhNguyenVien.Email,
                    CCCD = tinhNguyenVien.CCCD,
                    SoDienThoai = tinhNguyenVien.SoDienThoai,
                    DiaChi = tinhNguyenVien.DiaChi,
                    GioiThieu = tinhNguyenVien.GioiThieu,
                    AnhDaiDien = tinhNguyenVien.AnhDaiDien,
                    DiemTrungBinh = tinhNguyenVien.DiemTrungBinh,
                    CapBac = tinhNguyenVien.CapBac,
                    TongSuKienThamGia = tinhNguyenVien.TongSuKienThamGia,
                    LinhVucIds = linhVucIds,
                    KyNangIds = kyNangIds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy tình nguyện viên: {ex.Message}");
                throw;
            }
        }

        public async Task<TinhNguyenVienResponseDto> GetTinhNguyenVienByMaTaiKhoanAsync(int maTaiKhoan)
        {
            try
            {
                var tinhNguyenVien = await _context.Volunteer
                    .Include(t => t.TinhNguyenVien_LinhVucs)
                    .Include(t => t.TinhNguyenVien_KyNangs)
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

                if (tinhNguyenVien == null)
                {
                    throw new Exception("Không tìm thấy thông tin tình nguyện viên cho tài khoản này");
                }

                var linhVucIds = tinhNguyenVien.TinhNguyenVien_LinhVucs?
                    .Select(t => t.MaLinhVuc).ToList();

                var kyNangIds = tinhNguyenVien.TinhNguyenVien_KyNangs?
                    .Select(t => t.MaKyNang).ToList();

                return new TinhNguyenVienResponseDto
                {
                    MaTNV = tinhNguyenVien.MaTNV,
                    MaTaiKhoan = tinhNguyenVien.MaTaiKhoan,
                    HoTen = tinhNguyenVien.HoTen ?? string.Empty,
                    NgaySinh = FormatDateForResponse(tinhNguyenVien.NgaySinh),
                    GioiTinh = tinhNguyenVien.GioiTinh,
                    Email = tinhNguyenVien.Email,
                    CCCD = tinhNguyenVien.CCCD,
                    SoDienThoai = tinhNguyenVien.SoDienThoai,
                    DiaChi = tinhNguyenVien.DiaChi,
                    GioiThieu = tinhNguyenVien.GioiThieu,
                    AnhDaiDien = tinhNguyenVien.AnhDaiDien,
                    DiemTrungBinh = tinhNguyenVien.DiemTrungBinh,
                    CapBac = tinhNguyenVien.CapBac,
                    TongSuKienThamGia = tinhNguyenVien.TongSuKienThamGia,
                    LinhVucIds = linhVucIds,
                    KyNangIds = kyNangIds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy tình nguyện viên theo tài khoản: {ex.Message}");
                throw;
            }
        }

        public async Task<List<TinhNguyenVienResponseDto>> GetFeaturedTinhNguyenVienAsync()
        {
            try
            {
                // Lấy tình nguyện viên có điểm đánh giá cao nhất
                var tinhNguyenViens = await _context.Volunteer
                    .Include(t => t.TinhNguyenVien_LinhVucs)
                        .ThenInclude(tl => tl.LinhVuc)
                    .Include(t => t.TinhNguyenVien_KyNangs)
                        .ThenInclude(tk => tk.KyNang)
                    .OrderByDescending(t => t.DiemTrungBinh)
                    .Take(50)  // Lấy top 50 tình nguyện viên có điểm cao nhất
                    .ToListAsync();

                var result = new List<TinhNguyenVienResponseDto>();
                
                foreach (var tnv in tinhNguyenViens)
                {
                    var kyNangs = tnv.TinhNguyenVien_KyNangs?
                        .Select(k => new KyNangDto 
                        { 
                            MaKyNang = k.MaKyNang, 
                            TenKyNang = k.KyNang?.TenKyNang 
                        })
                        .ToList() ?? new List<KyNangDto>();
                    
                    var linhVucs = tnv.TinhNguyenVien_LinhVucs?
                        .Select(l => new LinhVucDto 
                        { 
                            MaLinhVuc = l.MaLinhVuc, 
                            TenLinhVuc = l.LinhVuc?.TenLinhVuc 
                        })
                        .ToList() ?? new List<LinhVucDto>();
                    
                    result.Add(new TinhNguyenVienResponseDto
                    {
                        MaTNV = tnv.MaTNV,
                        MaTaiKhoan = tnv.MaTaiKhoan,
                        HoTen = tnv.HoTen ?? string.Empty,
                        NgaySinh = FormatDateForResponse(tnv.NgaySinh),
                        GioiTinh = tnv.GioiTinh,
                        Email = tnv.Email,
                        CCCD = tnv.CCCD,
                    SoDienThoai = tnv.SoDienThoai,
                    DiaChi = tnv.DiaChi,
                    GioiThieu = tnv.GioiThieu,
                    AnhDaiDien = tnv.AnhDaiDien,
                    DiemTrungBinh = tnv.DiemTrungBinh,
                    CapBac = tnv.CapBac,
                    TongSuKienThamGia = tnv.TongSuKienThamGia,
                    KyNangs = kyNangs,
                    LinhVucs = linhVucs
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách tình nguyện viên nổi bật: {ex.Message}");
                throw;
            }
        }

        public async Task<List<KyNangDto>> GetVolunteerSkillsAsync(int maTNV)
        {
            try
            {
                var skills = await _context.TinhNguyenVien_KyNang
                    .Where(t => t.MaTNV == maTNV)
                    .Include(t => t.KyNang)
                    .Select(t => new KyNangDto
                    {
                        MaKyNang = t.MaKyNang,
                        TenKyNang = t.KyNang.TenKyNang
                    })
                    .ToListAsync();

                return skills;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy kỹ năng của tình nguyện viên: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LinhVucDto>> GetVolunteerFieldsAsync(int maTNV)
        {
            try
            {
                var fields = await _context.TinhNguyenVien_LinhVuc
                    .Where(t => t.MaTNV == maTNV)
                    .Include(t => t.LinhVuc)
                    .Select(t => new LinhVucDto
                    {
                        MaLinhVuc = t.MaLinhVuc,
                        TenLinhVuc = t.LinhVuc.TenLinhVuc
                    })
                    .ToListAsync();

                return fields;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy lĩnh vực của tình nguyện viên: {ex.Message}");
                throw;
            }
        }

        public async Task<TinhNguyenVienResponseDto> UpdateTinhNguyenVienAsync(int maTNV, UpdateTNVDto updateDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var tinhNguyenVien = await _context.Volunteer
                        .Include(t => t.TinhNguyenVien_LinhVucs)
                        .Include(t => t.TinhNguyenVien_KyNangs)
                        .FirstOrDefaultAsync(t => t.MaTNV == maTNV);

                    if (tinhNguyenVien == null)
                    {
                        throw new Exception("Tình nguyện viên không tồn tại");
                    }

                    // Cập nhật thông tin cơ bản
                    tinhNguyenVien.HoTen = updateDto.HoTen ?? tinhNguyenVien.HoTen;
                    tinhNguyenVien.NgaySinh = updateDto.NgaySinh ?? tinhNguyenVien.NgaySinh;
                    tinhNguyenVien.GioiTinh = updateDto.GioiTinh ?? tinhNguyenVien.GioiTinh;
                    tinhNguyenVien.Email = updateDto.Email;
                    tinhNguyenVien.CCCD = updateDto.CCCD?? tinhNguyenVien.CCCD;
                    tinhNguyenVien.SoDienThoai = updateDto.SoDienThoai ?? tinhNguyenVien.SoDienThoai;
                    tinhNguyenVien.DiaChi = updateDto.DiaChi??tinhNguyenVien.DiaChi;
                    tinhNguyenVien.GioiThieu = updateDto.GioiThieu ?? tinhNguyenVien.GioiThieu;
                    tinhNguyenVien.AnhDaiDien = updateDto.AnhDaiDien ?? tinhNguyenVien.AnhDaiDien;

                    var taiKhoan = await _context.User
                        .FirstOrDefaultAsync(t => t.MaTaiKhoan == tinhNguyenVien.MaTaiKhoan);

                    if (taiKhoan != null)
                    {
                        taiKhoan.Email = updateDto.Email;
                        _context.User.Update(taiKhoan);
                    }
                    
                    // Cập nhật Lĩnh vực
                    if (updateDto.LinhVucIds != null)
                    {
                        _context.TinhNguyenVien_LinhVuc.RemoveRange(tinhNguyenVien.TinhNguyenVien_LinhVucs);

                        foreach (var linhVucId in updateDto.LinhVucIds)
                        {
                            var linhVuc = await _context.LinhVuc.FirstOrDefaultAsync(l => l.MaLinhVuc == linhVucId);
                            if (linhVuc != null)
                            {
                                _context.TinhNguyenVien_LinhVuc.Add(new TinhNguyenVien_LinhVuc
                                {
                                    MaTNV = maTNV,
                                    MaLinhVuc = linhVucId
                                });
                            }
                        }
                    }

                    // Cập nhật Kỹ năng
                    if (updateDto.KyNangIds != null)
                    {
                        _context.TinhNguyenVien_KyNang.RemoveRange(tinhNguyenVien.TinhNguyenVien_KyNangs);

                        foreach (var kyNangId in updateDto.KyNangIds)
                        {
                            var kyNang = await _context.KyNang.FirstOrDefaultAsync(k => k.MaKyNang == kyNangId);
                            if (kyNang != null)
                            {
                                _context.TinhNguyenVien_KyNang.Add(new TinhNguyenVien_KyNang
                                {
                                    MaTNV = maTNV,
                                    MaKyNang = kyNangId
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetTinhNguyenVienAsync(maTNV);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi cập nhật tình nguyện viên: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<string> UploadAnhDaiDienAsync(int maTNV, IFormFile anhFile)
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

                var tinhNguyenVien = await _context.Volunteer.FindAsync(maTNV);
                if (tinhNguyenVien == null)
                    throw new Exception("Tình nguyện viên không tồn tại");

                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                if (!string.IsNullOrEmpty(tinhNguyenVien.AnhDaiDien))
                {
                    var oldFilePath = Path.Combine(webRootPath, tinhNguyenVien.AnhDaiDien.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }

                var fileName = $"{maTNV}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await anhFile.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/avatars/{fileName}";
                tinhNguyenVien.AnhDaiDien = imageUrl;

                await _context.SaveChangesAsync();

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload ảnh: {ex.Message}");
                throw;
            }
        }

        public async Task<TinhNguyenVienResponseDto> CreateTinhNguyenVienAsync(CreateTNVDto createDto)
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
                    
                    var tinhNguyenVien = new TinhNguyenVien
                    {
                        MaTaiKhoan = createDto.MaTaiKhoan,
                        HoTen = createDto.HoTen ?? string.Empty,
                        NgaySinh = createDto.NgaySinh,
                        GioiTinh = createDto.GioiTinh,
                        Email = createDto.Email,
                        CCCD = createDto.CCCD,
                        SoDienThoai = createDto.SoDienThoai,
                        DiaChi = createDto.DiaChi,
                        GioiThieu = createDto.GioiThieu,
                        AnhDaiDien = createDto.AnhDaiDien
                    };

                    _context.Volunteer.Add(tinhNguyenVien);
                    await _context.SaveChangesAsync();

                    if (createDto.LinhVucIds != null && createDto.LinhVucIds.Count > 0)
                    {
                        foreach (var linhVucId in createDto.LinhVucIds)
                        {
                            var linhVuc = await _context.LinhVuc.FindAsync(linhVucId);
                            if (linhVuc != null)
                            {
                                _context.TinhNguyenVien_LinhVuc.Add(new TinhNguyenVien_LinhVuc
                                {
                                    MaTNV = tinhNguyenVien.MaTNV,
                                    MaLinhVuc = linhVucId
                                });
                            }
                        }
                    }

                    if (createDto.KyNangIds != null && createDto.KyNangIds.Count > 0)
                    {
                        foreach (var kyNangId in createDto.KyNangIds)
                        {
                            var kyNang = await _context.KyNang.FindAsync(kyNangId);
                            if (kyNang != null)
                            {
                                _context.TinhNguyenVien_KyNang.Add(new TinhNguyenVien_KyNang
                                {
                                    MaTNV = tinhNguyenVien.MaTNV,
                                    MaKyNang = kyNangId
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetTinhNguyenVienAsync(tinhNguyenVien.MaTNV);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi tạo tình nguyện viên: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<List<TinhNguyenVienResponseDto>> GetAllTinhNguyenVienAsync()
        {
            try
            {
                var tinhNguyenViens = await _context.Volunteer
                    .Include(t => t.TinhNguyenVien_LinhVucs)
                    .Include(t => t.TinhNguyenVien_KyNangs)
                    .ToListAsync();

                return tinhNguyenViens.Select(t => new TinhNguyenVienResponseDto
                {
                    MaTNV = t.MaTNV,
                    MaTaiKhoan = t.MaTaiKhoan,
                    HoTen = t.HoTen ?? string.Empty,
                    NgaySinh = FormatDateForResponse(t.NgaySinh),
                    GioiTinh = t.GioiTinh,
                    Email = t.Email,
                    CCCD = t.CCCD,
                    SoDienThoai = t.SoDienThoai,
                    DiaChi = t.DiaChi,
                    GioiThieu = t.GioiThieu,
                    AnhDaiDien = t.AnhDaiDien,
                    DiemTrungBinh = t.DiemTrungBinh,
                    CapBac = t.CapBac,
                    TongSuKienThamGia = t.TongSuKienThamGia,
                    LinhVucIds = t.TinhNguyenVien_LinhVucs?.Select(l => l.MaLinhVuc).ToList(),
                    KyNangIds = t.TinhNguyenVien_KyNangs?.Select(k => k.MaKyNang).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách tình nguyện viên: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteTinhNguyenVienAsync(int maTNV)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var tinhNguyenVien = await _context.Volunteer
                        .Include(t => t.TinhNguyenVien_LinhVucs)
                        .Include(t => t.TinhNguyenVien_KyNangs)
                        .Include(t => t.TaiKhoan)
                        .FirstOrDefaultAsync(t => t.MaTNV == maTNV);

                    if (tinhNguyenVien == null)
                        throw new Exception("Tình nguyện viên không tồn tại");
                    
                    // Lưu MaTaiKhoan trước khi xóa
                    var maTaiKhoan = tinhNguyenVien.MaTaiKhoan;
                    
                    // Xóa các bản ghi liên quan
                    _context.TinhNguyenVien_LinhVuc.RemoveRange(tinhNguyenVien.TinhNguyenVien_LinhVucs);
                    _context.TinhNguyenVien_KyNang.RemoveRange(tinhNguyenVien.TinhNguyenVien_KyNangs);

                    // Xóa file ảnh đại diện
                    if (!string.IsNullOrEmpty(tinhNguyenVien.AnhDaiDien))
                    {
                        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var filePath = Path.Combine(webRootPath, tinhNguyenVien.AnhDaiDien.TrimStart('/'));
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    
                    // Xóa tình nguyện viên
                    _context.Volunteer.Remove(tinhNguyenVien);
                    await _context.SaveChangesAsync();

                    // Xóa tài khoản liên quan
                    var taiKhoan = await _context.User.FindAsync(maTaiKhoan);
                    if (taiKhoan != null)
                    {
                        // Xóa các bản ghi liên quan đến tài khoản trước
                        // Xóa đơn đăng ký
                        var donDangKys = await _context.DonDangKy.Where(d => d.MaTNV == maTNV).ToListAsync();
                        _context.DonDangKy.RemoveRange(donDangKys);

                        // Xóa đánh giá (nếu có)
                        var danhGias = await _context.DanhGia
                            .Where(d => d.MaNguoiDanhGia == maTaiKhoan || d.MaNguoiDuocDanhGia == maTaiKhoan)
                            .ToListAsync();
                        _context.DanhGia.RemoveRange(danhGias);

                        // Xóa thông báo (nếu có)
                        var thongBaos = await _context.ThongBao.Where(t => t.MaNguoiTao == maTaiKhoan).ToListAsync();
                        _context.ThongBao.RemoveRange(thongBaos);

                        // Xóa người nhận thông báo
                        var nguoiNhanThongBaos = await _context.NguoiNhanThongBao
                            .Where(n => n.MaNguoiNhanThongBao == maTaiKhoan)
                            .ToListAsync();
                        _context.NguoiNhanThongBao.RemoveRange(nguoiNhanThongBaos);

                        // Xóa token reset mật khẩu
                        var tokenResetMatKhaus = await _context.TokenResetMatKhau
                            .Where(t => t.MaTaiKhoan == maTaiKhoan)
                            .ToListAsync();
                        _context.TokenResetMatKhau.RemoveRange(tokenResetMatKhaus);

                        // Xóa token đổi email
                        var tokenDoiEmails = await _context.TokenDoiEmail
                            .Where(t => t.MaTaiKhoan == maTaiKhoan)
                            .ToListAsync();
                        _context.TokenDoiEmail.RemoveRange(tokenDoiEmails);

                        // Xóa tài khoản
                        _context.User.Remove(taiKhoan);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi xóa tình nguyện viên: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
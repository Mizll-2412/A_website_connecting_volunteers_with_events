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
    public class EventSerVice : IEventService
    {
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 5242880;
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };


        private readonly AppDbContext _context;
        private readonly ILogger<EventSerVice> _logger;

        public EventSerVice(AppDbContext context, ILogger<EventSerVice> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;

        }

        public async Task<SuKienResponseDto> CreateSuKienAsync(CreateSuKienDto createDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var toChuc = await _context.Organization.FindAsync(createDto.MaToChuc);
                    if (toChuc == null)
                    {
                        throw new Exception("Tổ chức không tồn tại");
                    }
                    var suKien = new SuKien
                    {
                        MaToChuc = createDto.MaToChuc,
                        TenSuKien = createDto.TenSuKien,
                        NoiDung = createDto.NoiDung,
                        DiaChi = createDto.DiaChi,
                        SoLuong = createDto.SoLuong,
                        NgayBatDau = createDto.NgayBatDau,
                        NgayKetThuc = createDto.NgayKetThuc,
                        NgayTao = DateTime.Now,
                        TuyenBatDau = createDto.TuyenBatDau,
                        TuyenKetThuc = createDto.TuyenKetThuc,
                        HinhAnh = createDto.HinhAnh,
                        TrangThai = createDto.TrangThai ?? "Đang tuyển"
                    };
                    _context.Event.Add(suKien);
                    await _context.SaveChangesAsync();

                    if (createDto.LinhVucIds != null && createDto.LinhVucIds.Count > 0)
                    {
                        foreach (var linhVucId in createDto.LinhVucIds)
                        {
                            var linhVuc = await _context.LinhVuc.FindAsync(linhVucId);
                            if (linhVuc != null)
                            {
                                _context.SuKien_LinhVuc.Add(new SuKien_LinhVuc
                                {
                                    MaSuKien = suKien.MaSuKien,
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
                                _context.SuKien_KyNang.Add(new SuKien_KyNang
                                {
                                    MaSuKien = suKien.MaSuKien,
                                    MaKyNang = kyNangId
                                });
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetSuKienAsync(suKien.MaSuKien);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi tạo sự kiện: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteSuKienAsync(int maSuKien)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var sukien = await _context.Event
                    .Include(s => s.SuKien_KyNangs)
                    .Include(s => s.SuKien_LinhVucs)
                    .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);
                    if (sukien == null)
                    {
                        throw new Exception("Sự kiện không tồn tại");
                    }
                    _context.SuKien_KyNang.RemoveRange(sukien.SuKien_KyNangs);
                    _context.SuKien_LinhVuc.RemoveRange(sukien.SuKien_LinhVucs);
                    _context.Event.Remove(sukien);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi xóa sự kiện: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<List<SuKienResponseDto>> GetAllSuKienAsync()
        {
            try
            {
                var suKiens = await _context.Event
                    .Include(s => s.SuKien_LinhVucs)
                    .Include(s => s.SuKien_KyNangs)
                    .ToListAsync();

                return suKiens.Select(s => new SuKienResponseDto
                {
                    MaSuKien = s.MaSuKien,
                    MaToChuc = s.MaToChuc,
                    TenSuKien = s.TenSuKien,
                    NoiDung = s.NoiDung,
                    SoLuong = s.SoLuong,
                    DiaChi = s.DiaChi,
                    NgayBatDau = s.NgayBatDau,
                    NgayKetThuc = s.NgayKetThuc,
                    NgayTao = s.NgayTao,
                    TuyenBatDau = s.TuyenBatDau,
                    TuyenKetThuc = s.TuyenKetThuc,
                    TrangThai = s.TrangThai,
                    LinhVucIds = s.SuKien_LinhVucs?.Select(l => l.MaLinhVuc).ToList(),
                    KyNangIds = s.SuKien_KyNangs?.Select(k => k.MaKyNang).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách sự kiện: {ex.Message}");
                throw;
            }
        }


        public async Task<SuKienResponseDto> GetSuKienAsync(int maSuKien)
        {
            try
            {
                var suKien = await _context.Event
                .Include(s => s.SuKien_LinhVucs)
                .Include(s => s.SuKien_KyNangs)
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);
                if (suKien == null)
                {
                    throw new Exception("Sự kiện không tồn tại");
                }
                return new SuKienResponseDto
                {
                    MaSuKien = suKien.MaSuKien,
                    MaToChuc = suKien.MaToChuc,
                    TenSuKien = suKien.TenSuKien,
                    NoiDung = suKien.NoiDung,
                    SoLuong = suKien.SoLuong,
                    DiaChi = suKien.DiaChi,
                    NgayBatDau = suKien.NgayBatDau,
                    NgayKetThuc = suKien.NgayKetThuc,
                    NgayTao = suKien.NgayTao,
                    TuyenBatDau = suKien.TuyenBatDau,
                    TuyenKetThuc = suKien.TuyenKetThuc,
                    TrangThai = suKien.TrangThai,
                    HinhAnh = suKien.HinhAnh,
                    LinhVucIds = suKien.SuKien_LinhVucs?.Select(l => l.MaLinhVuc).ToList(),
                    KyNangIds = suKien.SuKien_KyNangs?.Select(k => k.MaKyNang).ToList()
                };

            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy sự kiện: {ex.Message}");
                throw;
            }
        }

        public async Task<SuKienResponseDto> UpdateSuKienAsync(int maSuKien, UpdateSuKienDto updateDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var suKien = await _context.Event
                        .Include(s => s.SuKien_LinhVucs)
                        .Include(s => s.SuKien_KyNangs)
                        .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

                    if (suKien == null)
                    {
                        throw new Exception("Sự kiện không tồn tại");
                    }

                    suKien.TenSuKien = updateDto.TenSuKien;
                    suKien.NoiDung = updateDto.NoiDung;
                    suKien.SoLuong = updateDto.SoLuong;
                    suKien.DiaChi = updateDto.DiaChi;
                    suKien.NgayBatDau = updateDto.NgayBatDau;
                    suKien.NgayKetThuc = updateDto.NgayKetThuc;
                    suKien.TuyenBatDau = updateDto.TuyenBatDau;
                    suKien.TuyenKetThuc = updateDto.TuyenKetThuc;
                    suKien.TrangThai = updateDto.TrangThai;
                    suKien.HinhAnh = updateDto.HinhAnh;
                    if (updateDto.LinhVucIds != null)
                    {
                        _context.SuKien_LinhVuc.RemoveRange(suKien.SuKien_LinhVucs);

                        foreach (var linhVucId in updateDto.LinhVucIds)
                        {
                            var linhVuc = await _context.LinhVuc.FindAsync(linhVucId);
                            if (linhVuc != null)
                            {
                                _context.SuKien_LinhVuc.Add(new SuKien_LinhVuc
                                {
                                    MaSuKien = maSuKien,
                                    MaLinhVuc = linhVucId
                                });
                            }
                        }
                    }

                    // Cập nhật kỹ năng
                    if (updateDto.KyNangIds != null)
                    {
                        _context.SuKien_KyNang.RemoveRange(suKien.SuKien_KyNangs);

                        foreach (var kyNangId in updateDto.KyNangIds)
                        {
                            var kyNang = await _context.KyNang.FindAsync(kyNangId);
                            if (kyNang != null)
                            {
                                _context.SuKien_KyNang.Add(new SuKien_KyNang
                                {
                                    MaSuKien = maSuKien,
                                    MaKyNang = kyNangId
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetSuKienAsync(maSuKien);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi cập nhật sự kiện: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<string> UploadAnhAsync(int maSuKien, IFormFile anhFile)
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

                var tinhNguyenVien = await _context.Volunteer.FindAsync(maSuKien);
                if (tinhNguyenVien == null)
                    throw new Exception("Tình nguyện viên không tồn tại");

                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{maSuKien}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
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
        public async Task<string> UploadAnh(IFormFile anhFile)
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

                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await anhFile.CopyToAsync(stream);
                }
                var imageUrl = $"/uploads/avatars/{fileName}";
                await _context.SaveChangesAsync();

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload ảnh: {ex.Message}");
                throw;
            }
        }
    }
}
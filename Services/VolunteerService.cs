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
                    HoTen = tinhNguyenVien.HoTen,
                    NgaySinh = tinhNguyenVien.NgaySinh,
                    GioiTinh = tinhNguyenVien.GioiTinh,
                    Email = tinhNguyenVien.Email,
                    CCCD = tinhNguyenVien.CCCD,
                    DiaChi = tinhNguyenVien.DiaChi,
                    GioiThieu = tinhNguyenVien.GioiThieu,
                    AnhDaiDien = tinhNguyenVien.AnhDaiDien,
                    LinhVucIds = linhVucIds,
                    KyNangIds = kyNangIds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy tính nguyện viên: {ex.Message}");
                throw;
            }
        }

        public async Task<TinhNguyenVienResponseDto> UpdateTinhNguyenVienAsync(int maTNV, UpdateTinhNguyenVienDto updateDto)
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
                    tinhNguyenVien.HoTen = updateDto.HoTen;
                    tinhNguyenVien.NgaySinh = updateDto.NgaySinh;
                    tinhNguyenVien.GioiTinh = updateDto.GioiTinh;
                    tinhNguyenVien.Email = updateDto.Email;
                    tinhNguyenVien.CCCD = updateDto.CCCD;
                    tinhNguyenVien.DiaChi = updateDto.DiaChi;
                    tinhNguyenVien.GioiThieu = updateDto.GioiThieu;
                    tinhNguyenVien.AnhDaiDien = updateDto.AnhDaiDien;

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
                    _logger.LogError($"Lỗi cập nhật tính nguyện viên: {ex.Message}");
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
    }
}
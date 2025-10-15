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
                    TenToChuc = toChuc.TenToChuc,
                    Email = toChuc.Email,
                    SoDienThoai = toChuc.SoDienThoai,
                    DiaChi = toChuc.DiaChi,
                    NgayTao = toChuc.NgayTao,
                    GioiThieu = toChuc.GioiThieu,
                    AnhDaiDien = toChuc.AnhDaiDien
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
                    var toChuc = await _context.Organization
                        .FirstOrDefaultAsync(t => t.MaToChuc == maToChuc);

                    if (toChuc == null)
                    {
                        throw new Exception("Tổ chức không tồn tại");
                    }

                    // Cập nhật thông tin
                    toChuc.TenToChuc = updateDto.TenToChuc;
                    toChuc.Email = updateDto.Email;
                    toChuc.SoDienThoai = updateDto.SoDienThoai;
                    toChuc.DiaChi = updateDto.DiaChi;
                    toChuc.GioiThieu = updateDto.GioiThieu;
                    toChuc.AnhDaiDien = updateDto.AnhDaiDien;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetToChucAsync(maToChuc);
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
                var uploadPath = Path.Combine(webRootPath, "uploads", "avatars"); if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

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
    }
}
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
    public class LegalDocumentService : ILegalDocumentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LegalDocumentService> _logger;
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 10485760; // 10MB
        private readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

        public LegalDocumentService(AppDbContext context, ILogger<LegalDocumentService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        public async Task<GiayToPhapLyResponseDto> UploadGiayToAsync(UploadDocument uploadDto)
        {
            try
            {
                var toChuc = await _context.Organization.FindAsync(uploadDto.MaToChuc);
                if (toChuc == null)
                    throw new Exception("Tổ chức không tồn tại");

                if (uploadDto.File == null || uploadDto.File.Length == 0)
                    throw new Exception("File không hợp lệ");

                if (uploadDto.File.Length > MaxFileSize)
                    throw new Exception("File quá lớn (tối đa 10MB)");

                var ext = Path.GetExtension(uploadDto.File.FileName).ToLower();
                if (!AllowedExtensions.Contains(ext))
                    throw new Exception("Chỉ hỗ trợ file PDF, JPG, PNG");

                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "avatars"); if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{uploadDto.MaToChuc}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadDto.File.CopyToAsync(stream);
                }

                var giayTo = new GiayToPhapLy
                {
                    MaToChuc = uploadDto.MaToChuc,
                    TenGiayTo = uploadDto.TenGiayTo,
                    NgayTao = DateTime.Now,
                    File = $"/uploads/documents/{fileName}"
                };

                _context.GiayToPhapLy.Add(giayTo);

                toChuc.TrangThaiXacMinh = 0;
                toChuc.LyDoTuChoi = null;

                await _context.SaveChangesAsync();

                return new GiayToPhapLyResponseDto
                {
                    MaGiayTo = giayTo.MaGiayTo,
                    MaToChuc = giayTo.MaToChuc,
                    TenGiayTo = giayTo.TenGiayTo,
                    NgayTao = giayTo.NgayTao,
                    File = giayTo.File
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload giấy tờ: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GiayToPhapLyResponseDto>> GetGiayToByToChucAsync(int maToChuc)
        {
            try
            {
                var giayTos = await _context.GiayToPhapLy
                    .Where(g => g.MaToChuc == maToChuc)
                    .OrderByDescending(g => g.NgayTao)
                    .ToListAsync();

                return giayTos.Select(g => new GiayToPhapLyResponseDto
                {
                    MaGiayTo = g.MaGiayTo,
                    MaToChuc = g.MaToChuc,
                    TenGiayTo = g.TenGiayTo,
                    NgayTao = g.NgayTao,
                    File = g.File
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy giấy tờ: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteGiayToAsync(int maGiayTo)
        {
            try
            {
                var giayTo = await _context.GiayToPhapLy.FindAsync(maGiayTo);
                if (giayTo == null)
                    throw new Exception("Giấy tờ không tồn tại");

                // Xóa file vật lý
                if (!string.IsNullOrEmpty(giayTo.File))
                {
                    var filePath = Path.Combine(_env.WebRootPath, giayTo.File.TrimStart('/'));
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                _context.GiayToPhapLy.Remove(giayTo);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa giấy tờ: {ex.Message}");
                throw;
            }
        }

        public async Task<ToChucXacMinhResponseDto> XacMinhToChucAsync(int maToChuc, XacMinhToChucDto xacMinhDto)
        {
            try
            {
                var toChuc = await _context.Organization
                    .Include(t => t.GiayToPhapLys)
                    .FirstOrDefaultAsync(t => t.MaToChuc == maToChuc);

                if (toChuc == null)
                    throw new Exception("Tổ chức không tồn tại");

                // Cập nhật trạng thái
                toChuc.TrangThaiXacMinh = xacMinhDto.TrangThai;
                toChuc.LyDoTuChoi = xacMinhDto.TrangThai == 2 ? xacMinhDto.LyDoTuChoi : null;

                await _context.SaveChangesAsync();

                // TODO: Gửi thông báo cho tổ chức

                return await GetThongTinXacMinhAsync(maToChuc);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xác minh: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ToChucXacMinhResponseDto>> GetDanhSachChoXacMinhAsync()
        {
            try
            {
                var toChucs = await _context.Organization
                    .Include(t => t.GiayToPhapLys)
                    .Where(t => t.TrangThaiXacMinh == 0) // Chờ duyệt
                    .OrderBy(t => t.NgayTao)
                    .ToListAsync();

                return toChucs.Select(t => MapToDto(t)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách: {ex.Message}");
                throw;
            }
        }

        public async Task<ToChucXacMinhResponseDto> GetThongTinXacMinhAsync(int maToChuc)
        {
            try
            {
                var toChuc = await _context.Organization
                    .Include(t => t.GiayToPhapLys)
                    .FirstOrDefaultAsync(t => t.MaToChuc == maToChuc);

                if (toChuc == null)
                    throw new Exception("Tổ chức không tồn tại");

                return MapToDto(toChuc);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thông tin: {ex.Message}");
                throw;
            }
        }

        private ToChucXacMinhResponseDto MapToDto(ToChuc toChuc)
        {
            return new ToChucXacMinhResponseDto
            {
                MaToChuc = toChuc.MaToChuc,
                TenToChuc = toChuc.TenToChuc,
                Email = toChuc.Email,
                TrangThaiXacMinh = toChuc.TrangThaiXacMinh,
                TrangThaiXacMinhText = GetTrangThaiText(toChuc.TrangThaiXacMinh),
                LyDoTuChoi = toChuc.LyDoTuChoi,
                NgayTao = toChuc.NgayTao,
                GiayToPhapLys = toChuc.GiayToPhapLys?.Select(g => new GiayToPhapLyResponseDto
                {
                    MaGiayTo = g.MaGiayTo,
                    MaToChuc = g.MaToChuc,
                    TenGiayTo = g.TenGiayTo,
                    NgayTao = g.NgayTao,
                    File = g.File
                }).ToList()
            };
        }

        private string GetTrangThaiText(byte? trangThai)
        {
            return trangThai switch
            {
                0 => "Chờ duyệt",
                1 => "Đã xác minh ✓",
                2 => "Từ chối",
                _ => "Chưa xác minh"
            };
        }
    }
}
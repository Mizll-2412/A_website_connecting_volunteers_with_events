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

        public async Task<List<GiayToPhapLyResponseDto>> UploadGiayToAsync(UploadDocument uploadDto)
        {
            try
            {
                var toChuc = await _context.Organization.FindAsync(uploadDto.MaToChuc);
                if (toChuc == null)
                    throw new Exception("Tổ chức không tồn tại");

                if (uploadDto.Files == null || uploadDto.Files.Length == 0)
                    throw new Exception("Không có file được chọn");

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var uploadedFiles = new List<GiayToPhapLyResponseDto>();

                // Xử lý từng file
                foreach (var file in uploadDto.Files)
                {
                    // Kiểm tra kích thước và định dạng file
                    if (file.Length == 0)
                        continue; // Bỏ qua file rỗng
                        
                    if (file.Length > MaxFileSize)
                        throw new Exception($"File '{file.FileName}' quá lớn (tối đa 10MB)");

                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!AllowedExtensions.Contains(ext))
                        throw new Exception($"File '{file.FileName}' không được hỗ trợ (chỉ hỗ trợ PDF, JPG, PNG)");

                    // Tạo tên file duy nhất và lưu file
                    var fileName = $"{uploadDto.MaToChuc}_{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Tạo bản ghi giấy tờ pháp lý
                    var giayTo = new GiayToPhapLy
                    {
                        MaToChuc = uploadDto.MaToChuc,
                        TenGiayTo = uploadDto.TenGiayTo,
                        NgayTao = DateTime.Now,
                        File = $"/uploads/documents/{fileName}",
                        MoTa = uploadDto.MoTa
                    };

                    _context.GiayToPhapLy.Add(giayTo);
                    await _context.SaveChangesAsync(); // Lưu ngay để lấy MaGiayTo

                    // Thêm vào danh sách kết quả
                    uploadedFiles.Add(new GiayToPhapLyResponseDto
                    {
                        MaGiayTo = giayTo.MaGiayTo,
                        MaToChuc = giayTo.MaToChuc,
                        TenGiayTo = giayTo.TenGiayTo,
                        NgayTao = giayTo.NgayTao,
                        File = giayTo.File,
                        MoTa = giayTo.MoTa
                    });
                }

                // KHÔNG tự động đổi trạng thái xác minh khi chỉ tải giấy tờ
                // Trạng thái sẽ chỉ thay đổi khi tổ chức bấm "Gửi yêu cầu xác minh"

                return uploadedFiles;
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
                    File = g.File,
                    MoTa = g.MoTa
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
                    File = g.File,
                    MoTa = g.MoTa
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
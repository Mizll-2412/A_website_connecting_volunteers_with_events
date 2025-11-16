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
        private readonly INotificationService _notificationService;
        private const long MaxFileSize = 5242880; // 5MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public OrganizationService(AppDbContext context, ILogger<OrganizationService> logger, IWebHostEnvironment env, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _notificationService = notificationService;
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
                        .Include(t => t.TaiKhoan)
                        .FirstOrDefaultAsync(t => t.MaToChuc == maToChuc);

                    if (tochuc == null)
                        throw new Exception("Tổ chức không tồn tại");
                    
                    // Lưu MaTaiKhoan trước khi xóa
                    var maTaiKhoan = tochuc.MaTaiKhoan;
                    
                    // Xóa giấy tờ pháp lý
                    if (tochuc.GiayToPhapLys != null && tochuc.GiayToPhapLys.Any())
                    {
                        _context.GiayToPhapLy.RemoveRange(tochuc.GiayToPhapLys);
                        await _context.SaveChangesAsync(); // Lưu ngay để tránh lỗi
                    }

                    // Xóa các bản ghi liên quan đến sự kiện TRƯỚC khi xóa tổ chức
                    // Xóa sự kiện (nếu có)
                    var suKiens = await _context.Event.Where(s => s.MaToChuc == maToChuc).ToListAsync();
                    var suKienIds = suKiens.Select(s => s.MaSuKien).ToList();
                    
                    if (suKienIds.Any())
                    {
                        // Xóa file ảnh của sự kiện trước
                        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        foreach (var suKien in suKiens)
                        {
                            if (!string.IsNullOrEmpty(suKien.HinhAnh))
                            {
                                var filePath = Path.Combine(webRootPath, suKien.HinhAnh.TrimStart('/'));
                                if (File.Exists(filePath))
                                {
                                    try
                                    {
                                        File.Delete(filePath);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning($"Không thể xóa file ảnh sự kiện {suKien.MaSuKien}: {ex.Message}");
                                    }
                                }
                            }
                        }

                        // Xóa tất cả các bản ghi liên quan đến sự kiện cùng lúc (hiệu quả hơn)
                        // Xóa theo thứ tự để tránh foreign key constraint
                        
                        // 1. Xóa đơn đăng ký (có NoAction với SuKien)
                        var donDangKys = await _context.DonDangKy.Where(d => suKienIds.Contains(d.MaSuKien)).ToListAsync();
                        if (donDangKys.Any())
                        {
                            _context.DonDangKy.RemoveRange(donDangKys);
                            await _context.SaveChangesAsync();
                        }

                        // 2. Xóa đánh giá (có thể có foreign key đến SuKien)
                        var danhGiasSuKien = await _context.DanhGia.Where(d => suKienIds.Contains(d.MaSuKien)).ToListAsync();
                        if (danhGiasSuKien.Any())
                        {
                            _context.DanhGia.RemoveRange(danhGiasSuKien);
                            await _context.SaveChangesAsync();
                        }

                        // 3. Xóa giấy chứng nhận (có Cascade với SuKien, nhưng xóa trước để chắc chắn)
                        var giayChungNhans = await _context.GiayChungNhan.Where(g => suKienIds.Contains(g.MaSuKien)).ToListAsync();
                        if (giayChungNhans.Any())
                        {
                            _context.GiayChungNhan.RemoveRange(giayChungNhans);
                            await _context.SaveChangesAsync();
                        }

                        // 4. Xóa mẫu giấy chứng nhận
                        var mauGiayChungNhans = await _context.MauGiayChungNhan.Where(m => m.MaSuKien.HasValue && suKienIds.Contains(m.MaSuKien.Value)).ToListAsync();
                        if (mauGiayChungNhans.Any())
                        {
                            _context.MauGiayChungNhan.RemoveRange(mauGiayChungNhans);
                            await _context.SaveChangesAsync();
                        }

                        // 5. Xóa quan hệ sự kiện - kỹ năng
                        var suKienKyNangs = await _context.SuKien_KyNang.Where(s => suKienIds.Contains(s.MaSuKien)).ToListAsync();
                        if (suKienKyNangs.Any())
                        {
                            _context.SuKien_KyNang.RemoveRange(suKienKyNangs);
                            await _context.SaveChangesAsync();
                        }

                        // 6. Xóa quan hệ sự kiện - lĩnh vực
                        var suKienLinhVucs = await _context.SuKien_LinhVuc.Where(s => suKienIds.Contains(s.MaSuKien)).ToListAsync();
                        if (suKienLinhVucs.Any())
                        {
                            _context.SuKien_LinhVuc.RemoveRange(suKienLinhVucs);
                            await _context.SaveChangesAsync();
                        }

                        // 7. Xóa sự kiện (cuối cùng)
                        _context.Event.RemoveRange(suKiens);
                        await _context.SaveChangesAsync(); // Lưu thay đổi trước khi xóa tổ chức
                    }
                    
                    // Xóa file ảnh đại diện
                    if (!string.IsNullOrEmpty(tochuc.AnhDaiDien))
                    {
                        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var filePath = Path.Combine(webRootPath, tochuc.AnhDaiDien.TrimStart('/'));
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    
                    // Xóa tổ chức
                    _context.Organization.Remove(tochuc);
                    await _context.SaveChangesAsync();

                    // Xóa tài khoản liên quan
                    var taiKhoan = await _context.User.FindAsync(maTaiKhoan);
                    if (taiKhoan != null)
                    {
                        // Xóa các bản ghi liên quan đến tài khoản
                        // Lưu ý: Đánh giá liên quan đến sự kiện đã được xóa ở trên
                        // Chỉ xóa đánh giá không liên quan đến sự kiện (nếu có)
                        if (suKienIds.Any())
                        {
                            var danhGiasTaiKhoan = await _context.DanhGia
                                .Where(d => (d.MaNguoiDanhGia == maTaiKhoan || d.MaNguoiDuocDanhGia == maTaiKhoan)
                                    && !suKienIds.Contains(d.MaSuKien))
                                .ToListAsync();
                            _context.DanhGia.RemoveRange(danhGiasTaiKhoan);
                        }
                        else
                        {
                            // Nếu không có sự kiện, xóa tất cả đánh giá liên quan đến tài khoản
                            var danhGiasTaiKhoan = await _context.DanhGia
                                .Where(d => d.MaNguoiDanhGia == maTaiKhoan || d.MaNguoiDuocDanhGia == maTaiKhoan)
                                .ToListAsync();
                            _context.DanhGia.RemoveRange(danhGiasTaiKhoan);
                        }

                        // Xóa thông báo (nếu có)
                        var thongBaos = await _context.ThongBao.Where(t => t.MaNguoiTao == maTaiKhoan).ToListAsync();
                        var thongBaoIds = thongBaos.Select(t => t.MaThongBao).ToList();
                        
                        // Xóa TẤT CẢ người nhận thông báo liên quan đến các thông báo này (không chỉ của maTaiKhoan)
                        // Điều này quan trọng vì có thể có người khác cũng nhận thông báo này
                        if (thongBaoIds.Any())
                        {
                            var nguoiNhanThongBaos = await _context.NguoiNhanThongBao
                                .Where(n => thongBaoIds.Contains(n.MaThongBao))
                                .ToListAsync();
                            if (nguoiNhanThongBaos.Any())
                            {
                                _context.NguoiNhanThongBao.RemoveRange(nguoiNhanThongBaos);
                                await _context.SaveChangesAsync(); // Lưu trước khi xóa ThongBao
                            }
                        }
                        
                        // Xóa người nhận thông báo của tài khoản này (nếu có thông báo khác)
                        var nguoiNhanThongBaosCuaTaiKhoan = await _context.NguoiNhanThongBao
                            .Where(n => n.MaNguoiNhanThongBao == maTaiKhoan && !thongBaoIds.Contains(n.MaThongBao))
                            .ToListAsync();
                        if (nguoiNhanThongBaosCuaTaiKhoan.Any())
                        {
                            _context.NguoiNhanThongBao.RemoveRange(nguoiNhanThongBaosCuaTaiKhoan);
                            await _context.SaveChangesAsync();
                        }
                        
                        // Sau đó mới xóa thông báo
                        if (thongBaos.Any())
                        {
                            _context.ThongBao.RemoveRange(thongBaos);
                            await _context.SaveChangesAsync();
                        }

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

                        // Xóa Admin nếu có (mặc dù tổ chức không phải Admin, nhưng để an toàn)
                        var admin = await _context.Admin.FirstOrDefaultAsync(a => a.MaTaiKhoan == maTaiKhoan);
                        if (admin != null)
                        {
                            _context.Admin.Remove(admin);
                        }

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
                    _logger.LogError($"Lỗi xóa tổ chức (MaToChuc: {maToChuc}): {ex.Message}");
                    _logger.LogError($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw new Exception($"Không thể xóa tổ chức: {ex.Message}", ex);
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
                // 0 = Chờ xác minh, 1 = Đã xác minh, 2 = Đã từ chối
                toChuc.TrangThaiXacMinh = 0; // 0 = Chờ xác minh
                
                await _context.SaveChangesAsync();

                // Gửi thông báo tới Admin
                var adminUserIds = await _context.Admin
                    .Select(a => a.MaTaiKhoan)
                    .Where(id => id > 0)
                    .ToListAsync();

                if (adminUserIds.Any() && toChuc.MaTaiKhoan > 0)
                {
                    string tenToChuc = toChuc.TenToChuc ?? "Một tổ chức";
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        MaNguoiTao = toChuc.MaTaiKhoan,
                        PhanLoai = 1, // Thông báo hệ thống
                        NoiDung = $"Tổ chức \"{tenToChuc}\" đã gửi yêu cầu xác minh.",
                        MaNguoiNhans = adminUserIds
                    });
                }

                // Gửi thông báo xác nhận cho chính tổ chức
                if (toChuc.MaTaiKhoan > 0)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        MaNguoiTao = toChuc.MaTaiKhoan,
                        PhanLoai = 1,
                        NoiDung = "Bạn đã gửi yêu cầu xác minh. Chúng tôi sẽ xem xét trong thời gian sớm nhất.",
                        MaNguoiNhans = new List<int> { toChuc.MaTaiKhoan }
                    });
                }
                
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
                    case 0:
                        trangThaiText = "Đang chờ xác minh";
                        break;
                    case 1:
                        trangThaiText = "Đã xác minh";
                        break;
                    case 2:
                        trangThaiText = "Đã từ chối";
                        break;
                    case 3:
                        trangThaiText = "Đã thu hồi";
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
                    // DaGuiYeuCauXacMinh = true nếu đã gửi yêu cầu (0 = chờ xác minh, 1 = đã xác minh, 2 = từ chối)
                    DaGuiYeuCauXacMinh = toChuc.TrangThaiXacMinh.HasValue && toChuc.TrangThaiXacMinh >= 0
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
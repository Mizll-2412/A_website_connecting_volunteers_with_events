using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;

        public EventSerVice(AppDbContext context, ILogger<EventSerVice> logger, IWebHostEnvironment env, INotificationService notificationService, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _notificationService = notificationService;
            _configuration = configuration;
        }

        /// <summary>
        /// Format DateTime theo định dạng dd/MM/yyyy HH:mm (múi giờ Việt Nam)
        /// </summary>
        private string? FormatDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;
            
            return dateTime.Value.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>
        /// Tính toán trạng thái tuyển dụng dựa trên thời gian và số lượng
        /// </summary>
        private (string status, string cssClass, bool canRegister) GetTrangThaiTuyen(
            SuKien suKien, int soLuongDaDuyet, int tongDangKy, int gioiHanDangKy)
        {
            var now = DateTime.Now;
            
            // 1. Đóng thủ công
            if (!string.IsNullOrEmpty(suKien.TrangThaiTuyen) && suKien.TrangThaiTuyen == "Đóng")
                return ("Đóng", "secondary", false);
            
            // 2. Đã đủ người được duyệt
            if (suKien.SoLuong.HasValue && soLuongDaDuyet >= suKien.SoLuong.Value)
                return ("Đã đủ người", "warning", false);
            
            // 3. Vượt giới hạn đăng ký (1.5x)
            if (tongDangKy >= gioiHanDangKy && gioiHanDangKy > 0)
                return ("Đã đủ đơn", "warning", false);
            
            // 4. Kiểm tra thời gian tuyển
            if (suKien.TuyenBatDau.HasValue && now < suKien.TuyenBatDau.Value)
                return ("Chưa mở đăng ký", "info", false);
            
            if (suKien.TuyenBatDau.HasValue && suKien.TuyenKetThuc.HasValue &&
                now >= suKien.TuyenBatDau.Value && now <= suKien.TuyenKetThuc.Value)
                return ("Đang tuyển", "success", true);
            
            if (suKien.TuyenKetThuc.HasValue && now > suKien.TuyenKetThuc.Value)
                return ("Hết hạn tuyển", "danger", false);
            
            return ("Đang tuyển", "success", true);
        }

        /// <summary>
        /// Tính toán trạng thái sự kiện dựa trên thời gian
        /// </summary>
        private (string status, string cssClass) GetTrangThaiSuKien(SuKien suKien)
        {
            // 1. Ưu tiên trường TrangThai từ database (đóng sớm)
            if (suKien.TrangThai == "Đã kết thúc")
            {
                return ("Đã kết thúc", "secondary");
            }

            // 2. Tính toán dựa trên ngày
            var now = DateTime.Now;
            
            if (suKien.NgayKetThuc.HasValue && suKien.NgayKetThuc.Value < now)
            {
                return ("Đã kết thúc", "secondary");
            }
            
            if (suKien.NgayBatDau.HasValue && suKien.NgayBatDau.Value > now)
            {
                return ("Sắp diễn ra", "info");
            }
            
            if (suKien.NgayBatDau.HasValue && suKien.NgayBatDau.Value <= now && 
                (!suKien.NgayKetThuc.HasValue || suKien.NgayKetThuc.Value >= now))
            {
                return ("Đang diễn ra", "success");
            }
            
            return ("Sắp diễn ra", "info");
        }

        /// <summary>
        /// Tính toán trạng thái hiển thị của sự kiện (Legacy - giữ để tương thích)
        /// </summary>
        private string GetTrangThaiHienThi(SuKien suKien)
        {
            var (status, _) = GetTrangThaiSuKien(suKien);
            return status;
        }

        /// <summary>
        /// Helper method để map SuKien thành SuKienResponseDto với đầy đủ thông tin trạng thái
        /// </summary>
        private SuKienResponseDto MapToResponseDto(SuKien suKien, int soLuongDaDuyet, int soLuongChoDuyet)
        {
            var tongDangKy = soLuongDaDuyet + soLuongChoDuyet;
            var gioiHanDangKy = (int)Math.Ceiling((suKien.SoLuong ?? 0) * 1.5);
            
            // Tính 2 trạng thái độc lập với logic mới (đếm theo tổng đăng ký và giới hạn)
            var (trangThaiTuyen, trangThaiTuyenMau, choPhepDangKy) = 
                GetTrangThaiTuyen(suKien, soLuongDaDuyet, tongDangKy, gioiHanDangKy);
            
            var (trangThaiSuKien, trangThaiSuKienMau) = GetTrangThaiSuKien(suKien);
            
            // Tính số lượng còn lại
            var soLuongConLai = (suKien.SoLuong ?? 0) - soLuongDaDuyet;
            if (soLuongConLai < 0) soLuongConLai = 0;
            
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
                TrangThai = int.TryParse(suKien.TrangThai, out int trangThai) ? trangThai : 0,
                TrangThaiHienThi = GetTrangThaiHienThi(suKien),
                HinhAnh = suKien.HinhAnh,
                LinhVucIds = suKien.SuKien_LinhVucs?.Select(l => l.MaLinhVuc).ToList(),
                KyNangIds = suKien.SuKien_KyNangs?.Select(k => k.MaKyNang).ToList(),
                TenToChuc = suKien.Organization?.TenToChuc,
                MaTaiKhoanToChuc = suKien.Organization?.MaTaiKhoan,
                SoLuongDaDangKy = soLuongDaDuyet,  // Compatibility: backward compatibility field
                // Thêm các trường formatted
                DateFormat = "dd/MM/yyyy HH:mm",
                NgayBatDauFormatted = FormatDateTime(suKien.NgayBatDau),
                NgayKetThucFormatted = FormatDateTime(suKien.NgayKetThuc),
                TuyenBatDauFormatted = FormatDateTime(suKien.TuyenBatDau),
                TuyenKetThucFormatted = FormatDateTime(suKien.TuyenKetThuc),
                NgayTaoFormatted = FormatDateTime(suKien.NgayTao),
                // Thêm 2 trạng thái độc lập và thông tin chi tiết
                TrangThaiTuyen = trangThaiTuyen,
                TrangThaiTuyenMau = trangThaiTuyenMau,
                TrangThaiSuKien = trangThaiSuKien,
                TrangThaiSuKienMau = trangThaiSuKienMau,
                ChoPhepDangKy = choPhepDangKy,
                SoLuongConLai = soLuongConLai,
                
                // Thời gian diễn ra thực tế
                NgayDienRaBatDau = suKien.NgayDienRaBatDau,
                NgayDienRaKetThuc = suKien.NgayDienRaKetThuc,
                NgayDienRaBatDauFormatted = FormatDateTime(suKien.NgayDienRaBatDau),
                NgayDienRaKetThucFormatted = FormatDateTime(suKien.NgayDienRaKetThuc),
                ThoiGianKhoaHuy = suKien.ThoiGianKhoaHuy ?? 24,
                
                // Thống kê chi tiết
                SoLuongDaDuyet = soLuongDaDuyet,
                SoLuongChoDuyet = soLuongChoDuyet,
                TongSoDangKy = tongDangKy,
                GioiHanDangKy = gioiHanDangKy,
                
                // Thông tin hủy đăng ký - sẽ tính ở detail level
                CoTheHuyDangKy = false,
                SoGioConLaiDeHuy = 0
            };
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
                    
                    // Validation ngày tháng
                    if (!createDto.NgayBatDau.HasValue)
                    {
                        throw new Exception("Ngày bắt đầu sự kiện là bắt buộc");
                    }
                    
                    if (!createDto.NgayKetThuc.HasValue)
                    {
                        throw new Exception("Ngày kết thúc sự kiện là bắt buộc");
                    }
                    
                    if (createDto.NgayBatDau.Value > createDto.NgayKetThuc.Value)
                    {
                        throw new Exception("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc");
                    }
                    
                    // Validation ngày tuyển phải nằm trong khoảng ngày sự kiện
                    if (createDto.TuyenBatDau.HasValue || createDto.TuyenKetThuc.HasValue)
                    {
                        if (!createDto.TuyenBatDau.HasValue)
                        {
                            throw new Exception("Ngày bắt đầu tuyển là bắt buộc nếu có ngày kết thúc tuyển");
                        }
                        
                        if (!createDto.TuyenKetThuc.HasValue)
                        {
                            throw new Exception("Ngày kết thúc tuyển là bắt buộc nếu có ngày bắt đầu tuyển");
                        }
                        
                        if (createDto.TuyenBatDau.Value > createDto.TuyenKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu tuyển phải nhỏ hơn hoặc bằng ngày kết thúc tuyển");
                        }
                        
                        if (createDto.TuyenBatDau.Value < createDto.NgayBatDau.Value || 
                            createDto.TuyenBatDau.Value > createDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu tuyển phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }
                        
                        if (createDto.TuyenKetThuc.Value < createDto.NgayBatDau.Value || 
                            createDto.TuyenKetThuc.Value > createDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày kết thúc tuyển phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }
                    }
                    
                    // Validation thời gian diễn ra thực tế (nếu có)
                    if (createDto.NgayDienRaBatDau.HasValue)
                    {
                        if (createDto.NgayDienRaBatDau.Value < createDto.NgayBatDau.Value ||
                            createDto.NgayDienRaBatDau.Value > createDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu diễn ra phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }
                    }

                    if (createDto.NgayDienRaKetThuc.HasValue)
                    {
                        if (createDto.NgayDienRaKetThuc.Value < createDto.NgayBatDau.Value ||
                            createDto.NgayDienRaKetThuc.Value > createDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày kết thúc diễn ra phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }

                        if (createDto.NgayDienRaBatDau.HasValue &&
                            createDto.NgayDienRaBatDau.Value > createDto.NgayDienRaKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu diễn ra phải nhỏ hơn hoặc bằng ngày kết thúc diễn ra");
                        }
                    }

                    var suKien = new SuKien
                    {
                        MaToChuc = createDto.MaToChuc,
                        TenSuKien = createDto.TenSuKien,
                        NoiDung = createDto.NoiDung,
                        DiaChi = createDto.DiaChi,
                        SoLuong = createDto.SoLuong,
                        // DateTime từ frontend đã là local time (GMT+7), không cần convert
                        NgayBatDau = createDto.NgayBatDau.HasValue 
                            ? DateTime.SpecifyKind(createDto.NgayBatDau.Value, DateTimeKind.Unspecified) 
                            : null,
                        NgayKetThuc = createDto.NgayKetThuc.HasValue 
                            ? DateTime.SpecifyKind(createDto.NgayKetThuc.Value, DateTimeKind.Unspecified) 
                            : null,
                        NgayTao = DateTime.Now,
                        TuyenBatDau = createDto.TuyenBatDau.HasValue 
                            ? DateTime.SpecifyKind(createDto.TuyenBatDau.Value, DateTimeKind.Unspecified) 
                            : null,
                        TuyenKetThuc = createDto.TuyenKetThuc.HasValue 
                            ? DateTime.SpecifyKind(createDto.TuyenKetThuc.Value, DateTimeKind.Unspecified) 
                            : null,
                        NgayDienRaBatDau = createDto.NgayDienRaBatDau.HasValue 
                            ? DateTime.SpecifyKind(createDto.NgayDienRaBatDau.Value, DateTimeKind.Unspecified) 
                            : null,
                        NgayDienRaKetThuc = createDto.NgayDienRaKetThuc.HasValue 
                            ? DateTime.SpecifyKind(createDto.NgayDienRaKetThuc.Value, DateTimeKind.Unspecified) 
                            : null,
                        ThoiGianKhoaHuy = createDto.ThoiGianKhoaHuy ?? 24,
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

                    var blockingReasons = new List<string>();

                    // Chỉ chặn khi có đơn Chờ duyệt (0) hoặc Đã duyệt (1)
                    // Đơn Từ chối (2) không chặn xóa
                    if (await _context.DonDangKy.AnyAsync(d => d.MaSuKien == maSuKien && (d.TrangThai == 0 || d.TrangThai == 1)))
                    {
                        blockingReasons.Add("đơn đăng ký đang chờ duyệt hoặc đã duyệt");
                    }

                    if (await _context.GiayChungNhan.AnyAsync(g => g.MaSuKien == maSuKien))
                    {
                        blockingReasons.Add("giấy chứng nhận đã phát hành");
                    }

                    if (await _context.DanhGia.AnyAsync(dg => dg.MaSuKien == maSuKien))
                    {
                        blockingReasons.Add("đánh giá liên quan");
                    }

                    if (blockingReasons.Any())
                    {
                        var message = $"Không thể xóa sự kiện vì đang có {string.Join(", ", blockingReasons)}. Vui lòng xử lý các dữ liệu này trước khi xóa.";
                        throw new InvalidOperationException(message);
                    }

                    // Xóa các đơn đăng ký bị từ chối (TrangThai = 2) trước
                    var rejectedRegistrations = await _context.DonDangKy
                        .Where(d => d.MaSuKien == maSuKien && d.TrangThai == 2)
                        .ToListAsync();
                    
                    if (rejectedRegistrations.Any())
                    {
                        _context.DonDangKy.RemoveRange(rejectedRegistrations);
                        _logger.LogInformation($"Đã xóa {rejectedRegistrations.Count} đơn đăng ký bị từ chối của sự kiện {maSuKien}");
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
                    .Include(s => s.Organization)
                    .ToListAsync();

                // Lấy danh sách ID sự kiện
                var eventIds = suKiens.Select(s => s.MaSuKien).ToList();
                
                // Đếm 2 loại: Đã duyệt (TrangThai = 1) và Chờ duyệt (TrangThai = 0)
                var registrationStats = await _context.DonDangKy
                    .Where(d => eventIds.Contains(d.MaSuKien))
                    .GroupBy(d => d.MaSuKien)
                    .Select(g => new {
                        MaSuKien = g.Key,
                        DaDuyet = g.Count(d => d.TrangThai == 1),
                        ChoDuyet = g.Count(d => d.TrangThai == 0)
                    })
                    .ToDictionaryAsync(x => x.MaSuKien);

                return suKiens.Select(s =>
                {
                    int daDuyet = 0;
                    int choDuyet = 0;
                    
                    if (registrationStats.ContainsKey(s.MaSuKien))
                    {
                        daDuyet = registrationStats[s.MaSuKien].DaDuyet;
                        choDuyet = registrationStats[s.MaSuKien].ChoDuyet;
                    }
                    
                    return MapToResponseDto(s, daDuyet, choDuyet);
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách sự kiện: {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<SuKienResponseDto>> GetSuKiensByToChucAsync(int maToChuc)
        {
            try
            {
                var suKiens = await _context.Event
                    .Include(s => s.SuKien_LinhVucs)
                    .Include(s => s.SuKien_KyNangs)
                    .Include(s => s.Organization)
                    .Where(s => s.MaToChuc == maToChuc)
                    .ToListAsync();

                if (suKiens == null || !suKiens.Any())
                {
                    // Không tìm thấy sự kiện nào
                    return new List<SuKienResponseDto>();
                }

                // Lấy danh sách ID sự kiện
                var eventIds = suKiens.Select(s => s.MaSuKien).ToList();
                
                // Đếm 2 loại: Đã duyệt (TrangThai = 1) và Chờ duyệt (TrangThai = 0)
                var registrationStats = await _context.DonDangKy
                    .Where(d => eventIds.Contains(d.MaSuKien))
                    .GroupBy(d => d.MaSuKien)
                    .Select(g => new {
                        MaSuKien = g.Key,
                        DaDuyet = g.Count(d => d.TrangThai == 1),
                        ChoDuyet = g.Count(d => d.TrangThai == 0)
                    })
                    .ToDictionaryAsync(x => x.MaSuKien);

                return suKiens.Select(s =>
                {
                    int daDuyet = 0;
                    int choDuyet = 0;
                    
                    if (registrationStats.ContainsKey(s.MaSuKien))
                    {
                        daDuyet = registrationStats[s.MaSuKien].DaDuyet;
                        choDuyet = registrationStats[s.MaSuKien].ChoDuyet;
                    }
                    
                    return MapToResponseDto(s, daDuyet, choDuyet);
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách sự kiện theo tổ chức: {ex.Message}");
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
                .Include(s => s.Organization)
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);
                if (suKien == null)
                {
                    throw new Exception("Sự kiện không tồn tại");
                }
                
                // Đếm 2 loại: Đã duyệt (TrangThai = 1) và Chờ duyệt (TrangThai = 0)
                var soLuongDaDuyet = await _context.DonDangKy
                    .Where(d => d.MaSuKien == maSuKien && d.TrangThai == 1)
                    .CountAsync();
                
                var soLuongChoDuyet = await _context.DonDangKy
                    .Where(d => d.MaSuKien == maSuKien && d.TrangThai == 0)
                    .CountAsync();
                
                return MapToResponseDto(suKien, soLuongDaDuyet, soLuongChoDuyet);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy sự kiện: {ex.Message}");
                throw;
            }
        }

        public async Task<SuKienResponseDto> UpdateSuKienAsync(int maSuKien, UpdateSuKienDto updateDto)
        {
            _logger.LogInformation($"=== BẮT ĐẦU CẬP NHẬT SỰ KIỆN {maSuKien} tại {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ===");
            
            // Lưu dữ liệu cũ để so sánh trước khi cập nhật
            // Sử dụng AsNoTracking để tránh tracking entity và đảm bảo dữ liệu không bị thay đổi
            var suKienOld = await _context.Event
                .AsNoTracking()
                .Include(s => s.SuKien_LinhVucs)
                .Include(s => s.SuKien_KyNangs)
                .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

            if (suKienOld == null)
            {
                throw new Exception("Sự kiện không tồn tại");
            }

            // Lưu dữ liệu cũ vào biến riêng để đảm bảo không bị thay đổi
            var oldData = new
            {
                TenSuKien = suKienOld.TenSuKien ?? "",
                NoiDung = suKienOld.NoiDung ?? "",
                SoLuong = suKienOld.SoLuong,
                DiaChi = suKienOld.DiaChi ?? "",
                NgayBatDau = suKienOld.NgayBatDau,
                NgayKetThuc = suKienOld.NgayKetThuc,
                TuyenBatDau = suKienOld.TuyenBatDau,
                TuyenKetThuc = suKienOld.TuyenKetThuc,
                TrangThai = suKienOld.TrangThai ?? "",
                HinhAnh = suKienOld.HinhAnh ?? "",
                LinhVucIds = suKienOld.SuKien_LinhVucs?.Select(l => l.MaLinhVuc).OrderBy(x => x).ToList() ?? new List<int>(),
                KyNangIds = suKienOld.SuKien_KyNangs?.Select(k => k.MaKyNang).OrderBy(x => x).ToList() ?? new List<int>()
            };
            
            _logger.LogInformation($"Dữ liệu cũ - Tên: {oldData.TenSuKien}, Địa chỉ: {oldData.DiaChi}, Số lượng: {oldData.SoLuong}");
            _logger.LogInformation($"Dữ liệu mới - Tên: {updateDto.TenSuKien}, Địa chỉ: {updateDto.DiaChi}, Số lượng: {updateDto.SoLuong}");

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

                    // Validation ngày tháng
                    if (!updateDto.NgayBatDau.HasValue)
                    {
                        throw new Exception("Ngày bắt đầu sự kiện là bắt buộc");
                    }
                    
                    if (!updateDto.NgayKetThuc.HasValue)
                    {
                        throw new Exception("Ngày kết thúc sự kiện là bắt buộc");
                    }
                    
                    if (updateDto.NgayBatDau.Value > updateDto.NgayKetThuc.Value)
                    {
                        throw new Exception("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc");
                    }
                    
                    // Validation ngày tuyển phải nằm trong khoảng ngày sự kiện
                    if (updateDto.TuyenBatDau.HasValue || updateDto.TuyenKetThuc.HasValue)
                    {
                        if (!updateDto.TuyenBatDau.HasValue)
                        {
                            throw new Exception("Ngày bắt đầu tuyển là bắt buộc nếu có ngày kết thúc tuyển");
                        }
                        
                        if (!updateDto.TuyenKetThuc.HasValue)
                        {
                            throw new Exception("Ngày kết thúc tuyển là bắt buộc nếu có ngày bắt đầu tuyển");
                        }
                        
                        if (updateDto.TuyenBatDau.Value > updateDto.TuyenKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu tuyển phải nhỏ hơn hoặc bằng ngày kết thúc tuyển");
                        }
                        
                        if (updateDto.TuyenBatDau.Value < updateDto.NgayBatDau.Value || 
                            updateDto.TuyenBatDau.Value > updateDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu tuyển phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }
                        
                        if (updateDto.TuyenKetThuc.Value < updateDto.NgayBatDau.Value || 
                            updateDto.TuyenKetThuc.Value > updateDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày kết thúc tuyển phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }
                    }

                    // Validation thời gian diễn ra thực tế (nếu có)
                    if (updateDto.NgayDienRaBatDau.HasValue)
                    {
                        if (updateDto.NgayDienRaBatDau.Value < updateDto.NgayBatDau.Value ||
                            updateDto.NgayDienRaBatDau.Value > updateDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu diễn ra phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }
                    }

                    if (updateDto.NgayDienRaKetThuc.HasValue)
                    {
                        if (updateDto.NgayDienRaKetThuc.Value < updateDto.NgayBatDau.Value ||
                            updateDto.NgayDienRaKetThuc.Value > updateDto.NgayKetThuc.Value)
                        {
                            throw new Exception("Ngày kết thúc diễn ra phải nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc sự kiện");
                        }

                        if (updateDto.NgayDienRaBatDau.HasValue &&
                            updateDto.NgayDienRaBatDau.Value > updateDto.NgayDienRaKetThuc.Value)
                        {
                            throw new Exception("Ngày bắt đầu diễn ra phải nhỏ hơn hoặc bằng ngày kết thúc diễn ra");
                        }
                    }

                    suKien.TenSuKien = updateDto.TenSuKien;
                    suKien.NoiDung = updateDto.NoiDung;
                    suKien.SoLuong = updateDto.SoLuong;
                    suKien.DiaChi = updateDto.DiaChi;
                    // DateTime từ frontend đã là local time (GMT+7), không cần convert
                    suKien.NgayBatDau = updateDto.NgayBatDau.HasValue 
                        ? DateTime.SpecifyKind(updateDto.NgayBatDau.Value, DateTimeKind.Unspecified) 
                        : null;
                    suKien.NgayKetThuc = updateDto.NgayKetThuc.HasValue 
                        ? DateTime.SpecifyKind(updateDto.NgayKetThuc.Value, DateTimeKind.Unspecified) 
                        : null;
                    suKien.TuyenBatDau = updateDto.TuyenBatDau.HasValue 
                        ? DateTime.SpecifyKind(updateDto.TuyenBatDau.Value, DateTimeKind.Unspecified) 
                        : null;
                    suKien.TuyenKetThuc = updateDto.TuyenKetThuc.HasValue 
                        ? DateTime.SpecifyKind(updateDto.TuyenKetThuc.Value, DateTimeKind.Unspecified) 
                        : null;
                    suKien.NgayDienRaBatDau = updateDto.NgayDienRaBatDau.HasValue 
                        ? DateTime.SpecifyKind(updateDto.NgayDienRaBatDau.Value, DateTimeKind.Unspecified) 
                        : null;
                    suKien.NgayDienRaKetThuc = updateDto.NgayDienRaKetThuc.HasValue 
                        ? DateTime.SpecifyKind(updateDto.NgayDienRaKetThuc.Value, DateTimeKind.Unspecified) 
                        : null;
                    suKien.ThoiGianKhoaHuy = updateDto.ThoiGianKhoaHuy ?? suKien.ThoiGianKhoaHuy ?? 24;
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
                    _logger.LogInformation($"✓ Transaction đã commit thành công cho sự kiện {maSuKien}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"✗ Lỗi cập nhật sự kiện {maSuKien}: {ex.Message}");
                    _logger.LogError($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }

            // Gửi thông báo tới các user đã đăng ký sự kiện khi cập nhật
            // Luôn gửi thông báo mỗi lần cập nhật, không phụ thuộc vào lần cập nhật trước
            // Thực hiện sau khi transaction đã commit để đảm bảo dữ liệu đã được lưu
            _logger.LogInformation($"→ Bắt đầu xử lý gửi thông báo cập nhật cho sự kiện {maSuKien} (sau khi commit transaction)");
            try
            {
                var registrations = await _context.DonDangKy
                    .Include(d => d.TinhNguyenVien)
                    .Where(d => d.MaSuKien == maSuKien)
                    .ToListAsync();

                _logger.LogInformation($"Tìm thấy {registrations.Count} đăng ký cho sự kiện {maSuKien}");

                if (registrations.Any())
                {
                    // Lấy lại thông tin sự kiện sau khi đã cập nhật
                    var suKienUpdated = await _context.Event
                        .Include(s => s.SuKien_LinhVucs)
                        .Include(s => s.SuKien_KyNangs)
                        .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

                    if (suKienUpdated != null)
                    {
                        // Lấy thông tin tổ chức để làm người tạo thông báo
                        var toChuc = await _context.Organization
                            .FirstOrDefaultAsync(o => o.MaToChuc == suKienUpdated.MaToChuc);

                        if (toChuc != null)
                        {
                            // Lấy danh sách user IDs từ các đăng ký (bao gồm cả đã duyệt và chưa duyệt)
                            var userIds = registrations
                                .Where(r => r.TinhNguyenVien != null && r.TinhNguyenVien.MaTaiKhoan > 0)
                                .Select(r => r.TinhNguyenVien.MaTaiKhoan)
                                .Distinct()
                                .ToList();

                            if (userIds.Any())
                            {
                                // So sánh dữ liệu cũ và mới để xác định những gì đã thay đổi
                                var changes = new List<string>();
                                
                                // So sánh tên sự kiện (null-safe)
                                if (oldData.TenSuKien != (updateDto.TenSuKien ?? ""))
                                {
                                    changes.Add($"tên sự kiện");
                                    _logger.LogInformation($"Phát hiện thay đổi tên: '{oldData.TenSuKien}' -> '{updateDto.TenSuKien}'");
                                }
                                
                                // So sánh nội dung (null-safe)
                                if (oldData.NoiDung != (updateDto.NoiDung ?? ""))
                                {
                                    changes.Add($"nội dung");
                                    _logger.LogInformation($"Phát hiện thay đổi nội dung");
                                }
                                
                                // So sánh số lượng (nullable int)
                                if (oldData.SoLuong != updateDto.SoLuong)
                                {
                                    changes.Add($"số lượng người tham gia");
                                    _logger.LogInformation($"Phát hiện thay đổi số lượng: {oldData.SoLuong} -> {updateDto.SoLuong}");
                                }
                                
                                // So sánh địa chỉ (null-safe)
                                if (oldData.DiaChi != (updateDto.DiaChi ?? ""))
                                {
                                    changes.Add($"địa điểm");
                                    _logger.LogInformation($"Phát hiện thay đổi địa chỉ: '{oldData.DiaChi}' -> '{updateDto.DiaChi}'");
                                }
                                
                                // So sánh ngày bắt đầu (nullable DateTime - so sánh chính xác)
                                if ((oldData.NgayBatDau?.Ticks ?? 0) != (updateDto.NgayBatDau?.Ticks ?? 0))
                                {
                                    changes.Add($"ngày bắt đầu");
                                }
                                
                                // So sánh ngày kết thúc (nullable DateTime - so sánh chính xác)
                                if ((oldData.NgayKetThuc?.Ticks ?? 0) != (updateDto.NgayKetThuc?.Ticks ?? 0))
                                {
                                    changes.Add($"ngày kết thúc");
                                }
                                
                                // So sánh ngày bắt đầu tuyển (nullable DateTime - so sánh chính xác)
                                if ((oldData.TuyenBatDau?.Ticks ?? 0) != (updateDto.TuyenBatDau?.Ticks ?? 0))
                                {
                                    changes.Add($"ngày bắt đầu tuyển");
                                }
                                
                                // So sánh ngày kết thúc tuyển (nullable DateTime - so sánh chính xác)
                                if ((oldData.TuyenKetThuc?.Ticks ?? 0) != (updateDto.TuyenKetThuc?.Ticks ?? 0))
                                {
                                    changes.Add($"ngày kết thúc tuyển");
                                }
                                
                                // So sánh trạng thái (null-safe)
                                if (oldData.TrangThai != (updateDto.TrangThai ?? ""))
                                {
                                    changes.Add($"trạng thái");
                                    _logger.LogInformation($"Phát hiện thay đổi trạng thái: '{oldData.TrangThai}' -> '{updateDto.TrangThai}'");
                                }
                                
                                // So sánh hình ảnh (null-safe)
                                if (oldData.HinhAnh != (updateDto.HinhAnh ?? ""))
                                {
                                    changes.Add($"hình ảnh");
                                    _logger.LogInformation($"Phát hiện thay đổi hình ảnh");
                                }
                                
                                // So sánh lĩnh vực
                                var newLinhVucIds = updateDto.LinhVucIds?.OrderBy(x => x).ToList() ?? new List<int>();
                                if (!oldData.LinhVucIds.SequenceEqual(newLinhVucIds))
                                {
                                    changes.Add($"lĩnh vực");
                                }
                                
                                // So sánh kỹ năng
                                var newKyNangIds = updateDto.KyNangIds?.OrderBy(x => x).ToList() ?? new List<int>();
                                if (!oldData.KyNangIds.SequenceEqual(newKyNangIds))
                                {
                                    changes.Add($"kỹ năng");
                                }

                                // Tạo nội dung thông báo chi tiết
                                // LUÔN gửi thông báo mỗi lần cập nhật, bất kể có thay đổi hay không
                                string notificationContent;
                                if (changes.Any())
                                {
                                    var changesText = string.Join(", ", changes);
                                    notificationContent = $"Sự kiện \"{updateDto.TenSuKien}\" đã được cập nhật các thông tin: {changesText}. [EVENT_ID:{maSuKien}]";
                                    _logger.LogInformation($"Có {changes.Count} thay đổi được phát hiện: {changesText}");
                                }
                                else
                                {
                                    // Nếu không có thay đổi nào, vẫn gửi thông báo nhưng không liệt kê chi tiết
                                    notificationContent = $"Sự kiện \"{updateDto.TenSuKien}\" đã được cập nhật. [EVENT_ID:{maSuKien}]";
                                    _logger.LogWarning($"Không phát hiện thay đổi nào cho sự kiện {maSuKien}, nhưng vẫn gửi thông báo");
                                }
                                
                                _logger.LogInformation($"Đang tạo thông báo mới cho sự kiện {maSuKien} tại {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                                var createNotificationDto = new CreateNotificationDto
                                {
                                    MaNguoiTao = toChuc.MaTaiKhoan,
                                    PhanLoai = 2, // Thông báo sự kiện
                                    NoiDung = notificationContent,
                                    MaNguoiNhans = userIds
                                };

                                var notificationResult = await _notificationService.CreateNotificationAsync(createNotificationDto);
                                _logger.LogInformation($"✓ Đã tạo thông báo MỚI (ID: {notificationResult.MaThongBao}) cho sự kiện {maSuKien} tới {userIds.Count} người dùng. Nội dung: {notificationContent}");
                            }
                            else
                            {
                                _logger.LogWarning($"Không tìm thấy user IDs hợp lệ để gửi thông báo cho sự kiện {maSuKien}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Không tìm thấy tổ chức cho sự kiện {maSuKien}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Không tìm thấy sự kiện {maSuKien} sau khi cập nhật");
                    }
                }
                else
                {
                    _logger.LogInformation($"Sự kiện {maSuKien} chưa có người đăng ký, không cần gửi thông báo");
                }
            }
            catch (Exception notifEx)
            {
                // Log lỗi chi tiết nhưng không throw để không ảnh hưởng đến việc cập nhật sự kiện
                _logger.LogError($"✗✗✗ LỖI GỬI THÔNG BÁO cập nhật sự kiện {maSuKien}: {notifEx.Message}");
                _logger.LogError($"Stack trace: {notifEx.StackTrace}");
                if (notifEx.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {notifEx.InnerException.Message}");
                }
            }

            _logger.LogInformation($"=== KẾT THÚC CẬP NHẬT SỰ KIỆN {maSuKien} tại {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ===");
            return await GetSuKienAsync(maSuKien);
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
                    throw new ArgumentNullException(nameof(anhFile), "Vui lòng chọn file ảnh");

                if (anhFile.Length > MaxFileSize)
                    throw new InvalidOperationException($"File ảnh quá lớn (tối đa {MaxFileSize / 1024 / 1024}MB)");

                var fileExtension = Path.GetExtension(anhFile.FileName).ToLower();
                if (!AllowedExtensions.Contains(fileExtension))
                    throw new InvalidOperationException($"Định dạng file không hỗ trợ. Chỉ chấp nhận: {string.Join(", ", AllowedExtensions)}");

                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedContentTypes.Contains(anhFile.ContentType.ToLower()))
                    throw new InvalidOperationException("Content-Type không hợp lệ");

                // 3. Tạo đường dẫn
                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, "uploads", "events"); // ← Sửa thành "events" cho sự kiện

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                    _logger.LogInformation($"Đã tạo thư mục upload: {uploadPath}");
                }

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await anhFile.CopyToAsync(stream);
                }
                var imageUrl = $"/uploads/events/{fileName}";

                _logger.LogInformation($"Upload ảnh thành công: {imageUrl}");

                return imageUrl;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning($"File không hợp lệ: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Validation lỗi: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload ảnh: {ex.Message}");
                throw new Exception("Có lỗi xảy ra khi upload ảnh", ex);
            }
        }
    }
}
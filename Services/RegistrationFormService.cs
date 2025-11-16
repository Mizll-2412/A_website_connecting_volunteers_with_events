using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;
using khoaluantotnghiep.Helpers;
namespace khoaluantotnghiep.Services
{
    public class RegistrationFormService : IRegistrationFormService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IRegistrationFormService> _logger;
        public RegistrationFormService(AppDbContext context, ILogger<RegistrationFormService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DonDangKyResponseDto> DangKySuKienAsync(CreateDonDangKyDto createDto)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu đăng ký sự kiện: MaTNV={createDto.MaTNV}, MaSuKien={createDto.MaSuKien}");
                
                // Kiểm tra tình nguyện viên tồn tại
                var tnv = await _context.Volunteer.FindAsync(createDto.MaTNV);
                if (tnv == null)
                {
                    _logger.LogWarning($"Tình nguyện viên không tồn tại: MaTNV={createDto.MaTNV}");
                    throw new Exception("Tình nguyện viên không tồn tại");
                }
                
                // Kiểm tra sự kiện tồn tại
                var suKien = await _context.Event.FindAsync(createDto.MaSuKien);
                if (suKien == null)
                {
                    _logger.LogWarning($"Sự kiện không tồn tại: MaSuKien={createDto.MaSuKien}");
                    throw new Exception("Sự kiện không tồn tại");
                }
                
                // Kiểm tra đã đăng ký chưa
                var existing = await _context.DonDangKy.FirstOrDefaultAsync(d => d.MaTNV == createDto.MaTNV && d.MaSuKien == createDto.MaSuKien);
                if (existing != null)
                {
                    // Nếu đơn đăng ký đã bị từ chối (TrangThai = 2), cho phép đăng ký lại
                    if (existing.TrangThai == 2)
                    {
                        _logger.LogInformation($"Đăng ký lại sau khi bị từ chối: MaTNV={createDto.MaTNV}, MaSuKien={createDto.MaSuKien}");
                        // Cập nhật đơn đăng ký cũ thành trạng thái chờ duyệt
                        existing.TrangThai = 0; // Chờ duyệt
                        existing.NgayTao = DateTimeHelper.Now; // Cập nhật ngày đăng ký
                        existing.GhiChu = createDto.GhiChu ?? existing.GhiChu; // Cập nhật ghi chú nếu có
                        
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation($"Đăng ký lại thành công: MaTNV={createDto.MaTNV}, MaSuKien={createDto.MaSuKien}");
                        
                        return new DonDangKyResponseDto
                        {
                            MaTNV = existing.MaTNV,
                            MaSuKien = existing.MaSuKien,
                            NgayTao = existing.NgayTao,
                            GhiChu = existing.GhiChu,
                            TrangThai = existing.TrangThai,
                            TrangThaiText = GetTrangThaiText(existing.TrangThai)
                        };
                    }
                    else
                    {
                        // Nếu đơn đăng ký chưa bị từ chối (đang chờ duyệt hoặc đã duyệt), không cho đăng ký lại
                        _logger.LogWarning($"Đã tồn tại đơn đăng ký với trạng thái {existing.TrangThai}: MaTNV={createDto.MaTNV}, MaSuKien={createDto.MaSuKien}");
                        throw new Exception("Bạn đã đăng ký tham gia sự kiện này rồi");
                    }
                }
                
                // Kiểm tra thời hạn đăng ký - chỉ kiểm tra thời gian tuyển
                if (suKien.TuyenBatDau.HasValue && DateTimeHelper.Now < suKien.TuyenBatDau)
                {
                    _logger.LogWarning($"Chưa đến thời gian tuyển: MaSuKien={createDto.MaSuKien}");
                    throw new Exception("Chưa đến thời gian tuyển tình nguyện viên");
                }
                
                if (suKien.TuyenKetThuc.HasValue && DateTimeHelper.Now > suKien.TuyenKetThuc)
                {
                    _logger.LogWarning($"Sự kiện đã hết hạn đăng ký: MaSuKien={createDto.MaSuKien}");
                    throw new Exception("Sự kiện đã hết hạn đăng ký");
                }
                
                // Kiểm tra sự kiện đã kết thúc chưa (dựa trên ngày kết thúc)
                if (suKien.NgayKetThuc.HasValue && DateTimeHelper.Now > suKien.NgayKetThuc)
                {
                    _logger.LogWarning($"Sự kiện đã kết thúc: MaSuKien={createDto.MaSuKien}, NgayKetThuc={suKien.NgayKetThuc}");
                    throw new Exception("Không thể đăng ký vì sự kiện đã kết thúc");
                }
                
                // Kiểm tra số lượng
                var soLuongDaDangKy = await _context.DonDangKy
                                    .Where(d => d.MaSuKien == createDto.MaSuKien && d.TrangThai == 1)
                                    .CountAsync();
                                    
                if (suKien.SoLuong.HasValue && soLuongDaDangKy >= suKien.SoLuong)
                {
                    _logger.LogWarning($"Sự kiện đã đủ số lượng: MaSuKien={createDto.MaSuKien}");
                    throw new Exception("Sự kiện đã đủ số lượng tình nguyện viên");
                }
                
                // Tạo đơn đăng ký mới
                var donDangKy = new DonDangKy
                {
                    MaTNV = createDto.MaTNV,
                    MaSuKien = createDto.MaSuKien,
                    NgayTao = DateTimeHelper.Now,
                    GhiChu = createDto.GhiChu,
                    TrangThai = 0 // Chờ duyệt
                };
                
                _context.DonDangKy.Add(donDangKy);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Đăng ký sự kiện thành công: MaTNV={createDto.MaTNV}, MaSuKien={createDto.MaSuKien}");

                return await GetDonDangKyAsync(createDto.MaTNV, createDto.MaSuKien);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đăng ký sự kiện: {ex.Message}");
                throw;
            }
        }

        public async Task<DonDangKyResponseDto> GetDonDangKyAsync(int maTNV, int maSuKien)
        {
            try
            {
                var don = await _context.DonDangKy
                    .Include(d => d.TinhNguyenVien)
                    .Include(d => d.SuKien)
                    .FirstOrDefaultAsync(d => d.MaTNV == maTNV && d.MaSuKien == maSuKien);

                if (don == null)
                    throw new Exception("Đơn đăng ký không tồn tại");

                return MapToDto(don);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy đơn đăng ký: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DonDangKyResponseDto>> GetDonDangKyByTNVAsync(int maTNV)
        {
            try
            {
                // Auto-reject expired pending registrations first
                await AutoRejectPendingRegistrationsAsync();
                
                var dons = await _context.DonDangKy
                    .Include(d => d.TinhNguyenVien)
                    .Include(d => d.SuKien)
                    .ThenInclude(s => s.Organization)
                    .Where(d => d.MaTNV == maTNV)
                    .ToListAsync();

                return dons.Select(d => MapToDto(d)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách đơn: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DonDangKyResponseDto>> GetDonDangKyBySuKienAsync(int maSuKien)
        {
            try
            {
                // Auto-reject expired pending registrations first
                await AutoRejectPendingRegistrationsAsync();
                
                var dons = await _context.DonDangKy
                    .Include(d => d.TinhNguyenVien)
                    .Include(d => d.SuKien)
                    .Where(d => d.MaSuKien == maSuKien)
                    .ToListAsync();

                return dons.Select(d => MapToDto(d)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách đơn: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> HuyDangKyAsync(int maTNV, int maSuKien)
        {
            try
            {
                var don = await _context.DonDangKy
                    .Include(d => d.SuKien)
                    .FirstOrDefaultAsync(d => d.MaTNV == maTNV && d.MaSuKien == maSuKien);

                if (don == null)
                    throw new Exception("Đơn đăng ký không tồn tại");

                // Kiểm tra sự kiện đã kết thúc chưa
                var suKien = don.SuKien;
                if (suKien == null)
                    throw new Exception("Sự kiện không tồn tại");

                // Kiểm tra trạng thái sự kiện
                if (suKien.TrangThai == "Đã kết thúc")
                    throw new Exception("Không thể hủy đăng ký vì sự kiện đã kết thúc");

                // Kiểm tra ngày kết thúc
                if (suKien.NgayKetThuc.HasValue && suKien.NgayKetThuc.Value < DateTime.Now)
                    throw new Exception("Không thể hủy đăng ký vì sự kiện đã kết thúc");

                // Kiểm tra thời gian khóa hủy (CHỈ áp dụng cho đơn đã duyệt)
                if (don.TrangThai == 1)
                {
                    // Lấy thời gian khóa hủy (mặc định 24 giờ)
                    int thoiGianKhoaHuy = suKien.ThoiGianKhoaHuy ?? 24;
                    
                    // Xác định ngày diễn ra thực tế với fallback logic
                    DateTime ngayDienRaThucTe;
                    if (suKien.NgayDienRaBatDau.HasValue)
                    {
                        ngayDienRaThucTe = suKien.NgayDienRaBatDau.Value;
                    }
                    else if (suKien.TuyenKetThuc.HasValue)
                    {
                        // Nếu không nhập ngày diễn ra, tính = TuyenKetThuc + lock time
                        ngayDienRaThucTe = suKien.TuyenKetThuc.Value.AddHours(thoiGianKhoaHuy);
                    }
                    else if (suKien.NgayBatDau.HasValue)
                    {
                        ngayDienRaThucTe = suKien.NgayBatDau.Value;
                    }
                    else
                    {
                        // Fallback cuối: không cho hủy
                        throw new Exception("Không thể xác định thời gian sự kiện để tính thời gian khóa hủy");
                    }
                    
                    // Tính khoảng cách thời gian
                    var khoangCach = (ngayDienRaThucTe - DateTime.Now).TotalHours;
                    
                    if (khoangCach < thoiGianKhoaHuy)
                    {
                        throw new Exception($"Không thể hủy đăng ký trong vòng {thoiGianKhoaHuy} giờ trước khi sự kiện diễn ra");
                    }
                }

                _context.DonDangKy.Remove(don);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi hủy đăng ký: {ex.Message}");
                throw;
            }
        }

        public async Task<DonDangKyResponseDto> UpdateTrangThaiAsync(int maTNV, int maSuKien, UpdateDonDangKyDto updateDto)
        {
            try
            {
                var don = await _context.DonDangKy
                    .Include(d => d.SuKien)
                    .FirstOrDefaultAsync(d => d.MaTNV == maTNV && d.MaSuKien == maSuKien);
                
                if (don == null)
                {
                    throw new Exception("Đơn đăng ký không tồn tại");
                }

                var suKien = don.SuKien ?? await _context.Event.FindAsync(maSuKien);
                if (suKien == null)
                {
                    throw new Exception("Sự kiện không tồn tại");
                }

                // === UNDO APPROVAL (1 → 0) ===
                if (don.TrangThai == 1 && updateDto.TrangThai == 0)
                {
                    // Validation: Only allow if event hasn't started yet
                    if (suKien.TrangThai == "Đã kết thúc")
                    {
                        throw new Exception("Không thể hủy duyệt vì sự kiện đã kết thúc");
                    }
                    
                    // Check if event has started
                    DateTime eventStartDate = suKien.NgayDienRaBatDau 
                        ?? suKien.NgayBatDau 
                        ?? DateTime.Now;
                    
                    if (eventStartDate < DateTime.Now)
                    {
                        throw new Exception("Không thể hủy duyệt vì sự kiện đã bắt đầu");
                    }
                    
                    _logger.LogInformation($"Hủy duyệt đơn đăng ký: MaSuKien={maSuKien}, MaTNV={maTNV}");
                }

                // === REJECT APPROVED (1 → 2) ===
                if (don.TrangThai == 1 && updateDto.TrangThai == 2)
                {
                    // Require reason when rejecting approved registration
                    if (string.IsNullOrWhiteSpace(updateDto.GhiChu))
                    {
                        throw new Exception("Vui lòng nhập lý do từ chối");
                    }
                    
                    // Validation: Only allow if event hasn't started yet
                    DateTime eventStartDate = suKien.NgayDienRaBatDau 
                        ?? suKien.NgayBatDau 
                        ?? DateTime.Now;
                    
                    if (eventStartDate < DateTime.Now)
                    {
                        throw new Exception("Không thể từ chối TNV đã duyệt vì sự kiện đã bắt đầu");
                    }
                    
                    _logger.LogWarning($"Từ chối đơn đã duyệt: MaSuKien={maSuKien}, MaTNV={maTNV}, Lý do: {updateDto.GhiChu}");
                }

                // === APPROVE (0 → 1) ===
                if (updateDto.TrangThai == 1 && don.TrangThai != 1)
                {
                    // Kiểm tra số lượng đã duyệt hiện tại
                    var soLuongDaDuyet = await _context.DonDangKy
                        .Where(d => d.MaSuKien == maSuKien && d.TrangThai == 1)
                        .CountAsync();
                    
                    if (suKien.SoLuong.HasValue)
                    {
                        // Kiểm tra đã đủ số lượng chưa
                        if (soLuongDaDuyet >= suKien.SoLuong.Value)
                        {
                            _logger.LogWarning($"Không thể duyệt: Sự kiện đã đủ số lượng ({soLuongDaDuyet}/{suKien.SoLuong.Value}). MaSuKien={maSuKien}, MaTNV={maTNV}");
                            throw new Exception($"Không thể duyệt thêm. Sự kiện đã đủ số lượng tình nguyện viên ({suKien.SoLuong} người).");
                        }
                        
                        _logger.LogInformation($"Duyệt đơn đăng ký: Số lượng hiện tại {soLuongDaDuyet + 1}/{suKien.SoLuong.Value}. MaSuKien={maSuKien}, MaTNV={maTNV}");
                    }
                }
                
                // Cập nhật trạng thái
                don.TrangThai = updateDto.TrangThai;
                don.GhiChu = updateDto.GhiChu ?? don.GhiChu;

                await _context.SaveChangesAsync();

                return await GetDonDangKyAsync(maTNV, maSuKien);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật trạng thái: {ex.Message}");
                throw;
            }
        }

        public async Task<int> AutoRejectPendingRegistrationsAsync()
        {
            try
            {
                var now = DateTime.Now;
                
                // Query pending registrations that should be auto-rejected:
                // 1. Sự kiện đã kết thúc (NgayKetThuc < now)
                // 2. Hoặc hết hạn tuyển (TuyenKetThuc < now)
                var pendingRegistrations = await _context.DonDangKy
                    .Include(d => d.SuKien)
                    .Where(d => d.TrangThai == 0 && // Chờ duyệt
                                d.SuKien != null &&
                                (
                                    // Sự kiện đã kết thúc
                                    (d.SuKien.NgayKetThuc.HasValue && d.SuKien.NgayKetThuc.Value < now) ||
                                    // Hoặc hết hạn tuyển
                                    (d.SuKien.TuyenKetThuc.HasValue && d.SuKien.TuyenKetThuc.Value < now)
                                ))
                    .ToListAsync();
                
                if (!pendingRegistrations.Any())
                {
                    return 0;
                }
                
                // Auto-reject expired registrations
                foreach (var don in pendingRegistrations)
                {
                    don.TrangThai = 2; // Từ chối
                    
                    // Xác định lý do từ chối
                    string rejectionReason = string.Empty;
                    if (don.SuKien != null)
                    {
                        if (don.SuKien.NgayKetThuc.HasValue && don.SuKien.NgayKetThuc.Value < now)
                        {
                            rejectionReason = "Sự kiện đã kết thúc - Tự động từ chối";
                        }
                        else if (don.SuKien.TuyenKetThuc.HasValue && don.SuKien.TuyenKetThuc.Value < now)
                        {
                            rejectionReason = "Hết hạn tuyển - Tự động từ chối";
                        }
                    }
                    
                    don.GhiChu = string.IsNullOrEmpty(don.GhiChu) 
                        ? rejectionReason
                        : don.GhiChu + " | " + rejectionReason;
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Auto-rejected {pendingRegistrations.Count} pending registrations");
                
                return pendingRegistrations.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi auto-reject pending registrations: {ex.Message}");
                throw;
            }
        }

        private DonDangKyResponseDto MapToDto(DonDangKy don)
        {
            return new DonDangKyResponseDto
            {
                MaTNV = don.MaTNV,
                MaTaiKhoan = don.TinhNguyenVien?.MaTaiKhoan ?? 0,
                MaSuKien = don.MaSuKien,
                NgayTao = don.NgayTao,
                GhiChu = don.GhiChu,
                TrangThai = don.TrangThai,
                TrangThaiText = GetTrangThaiText(don.TrangThai),
                TenTNV = don.TinhNguyenVien?.HoTen,
                TenSuKien = don.SuKien?.TenSuKien,
                Email = don.TinhNguyenVien?.Email,
                SoDienThoai = don.TinhNguyenVien?.SoDienThoai,
                Event = don.SuKien != null ? new EventBasicInfo
                {
                    MaSuKien = don.SuKien.MaSuKien,
                    TenSuKien = don.SuKien.TenSuKien,
                    NgayBatDau = don.SuKien.NgayBatDau,
                    NgayKetThuc = don.SuKien.NgayKetThuc,
                    DiaChi = don.SuKien.DiaChi,
                    MaToChuc = don.SuKien.MaToChuc,
                    MaTaiKhoanToChuc = don.SuKien.Organization?.MaTaiKhoan,
                    TrangThai = don.SuKien.TrangThai,
                    TrangThaiHienThi = GetEventStatusText(don.SuKien)
                } : null
            };
        }
        private string GetTrangThaiText(int? trangThai)
        {
            return trangThai switch
            {
                0 => "Chờ duyệt",
                1 => "Đã duyệt",
                2 => "Từ chối",
                _ => "Không xác định"
            };
        }
        
        private string GetEventStatusText(SuKien suKien)
        {
            var now = DateTime.Now;
            
            // Kiểm tra ngày kết thúc
            if (suKien.NgayKetThuc.HasValue && suKien.NgayKetThuc.Value < now)
            {
                return "Đã kết thúc";
            }
            
            // Kiểm tra ngày bắt đầu
            if (suKien.NgayBatDau.HasValue && suKien.NgayBatDau.Value <= now && 
                suKien.NgayKetThuc.HasValue && suKien.NgayKetThuc.Value >= now)
            {
                return "Đang diễn ra";
            }
            
            // Kiểm tra thời gian tuyển
            if (suKien.TuyenBatDau.HasValue && suKien.TuyenKetThuc.HasValue &&
                suKien.TuyenBatDau.Value <= now && suKien.TuyenKetThuc.Value >= now)
            {
                return "Đang tuyển";
            }
            
            return "Sắp diễn ra";
        }
        
        /// <summary>
        /// Lấy lịch sử tham gia sự kiện của tình nguyện viên
        /// </summary>
        public async Task<List<EventHistoryDto>> GetEventHistoryAsync(int maTNV, EventHistoryFilterDto? filter = null)
        {
            try
            {
                var query = _context.DonDangKy
                    .Where(d => d.MaTNV == maTNV)
                    .Include(d => d.SuKien)
                    .ThenInclude(s => s.Organization)
                    .Include(d => d.TinhNguyenVien)
                    .AsQueryable();

                // Áp dụng filter nếu có
                if (filter != null)
                {
                    if (filter.Nam.HasValue)
                    {
                        query = query.Where(d => d.SuKien.NgayBatDau.HasValue && 
                                               d.SuKien.NgayBatDau.Value.Year == filter.Nam.Value);
                    }

                    if (filter.Thang.HasValue)
                    {
                        query = query.Where(d => d.SuKien.NgayBatDau.HasValue && 
                                               d.SuKien.NgayBatDau.Value.Month == filter.Thang.Value);
                    }

                    if (filter.HoanThanh.HasValue)
                    {
                        bool isCompleted = filter.HoanThanh.Value;
                        var currentTime = DateTimeHelper.Now;
                        
                        if (isCompleted)
                        {
                            query = query.Where(d => d.SuKien.NgayKetThuc.HasValue && 
                                                   d.SuKien.NgayKetThuc.Value < currentTime &&
                                                   d.TrangThai == 1); // Đã duyệt
                        }
                        else
                        {
                            query = query.Where(d => !d.SuKien.NgayKetThuc.HasValue || 
                                                   d.SuKien.NgayKetThuc.Value >= currentTime);
                        }
                    }
                }

                var donDangKys = await query.OrderByDescending(d => d.NgayTao).ToListAsync();
                
                // Kiểm tra các giấy chứng nhận và đánh giá
                var suKienIds = donDangKys.Select(d => d.MaSuKien).ToList();
                
                // Lấy danh sách đánh giá
                var danhGias = await _context.DanhGia
                    .Where(d => d.MaNguoiDanhGia == maTNV && suKienIds.Contains(d.MaSuKien))
                    .ToListAsync();
                
                // Lấy danh sách giấy chứng nhận
                var giayChungNhans = await _context.GiayChungNhan
                    .Where(g => g.MaTNV == maTNV)
                    .ToListAsync();
                
                DateTime now = DateTimeHelper.Now;
                
                return donDangKys.Select(d =>
                {
                    var suKien = d.SuKien;
                    var organization = suKien?.Organization;
                    var ngayKetThuc = suKien?.NgayKetThuc;
                    var daHoanThanh = ngayKetThuc.HasValue && ngayKetThuc.Value < now && d.TrangThai == 1;
                    var hasCertificate = giayChungNhans.Any(g => g.MaSuKien == d.MaSuKien);

                    return new EventHistoryDto
                    {
                        MaSuKien = d.MaSuKien,
                        TenSuKien = suKien?.TenSuKien ?? string.Empty,
                        NgayBatDau = suKien?.NgayBatDau,
                        NgayKetThuc = ngayKetThuc,
                        DiaChi = suKien?.DiaChi ?? string.Empty,
                        HinhAnh = suKien?.HinhAnh ?? string.Empty,
                        TrangThaiDangKy = d.TrangThai,
                        TrangThaiDangKyText = GetTrangThaiText(d.TrangThai),
                        NgayDangKy = d.NgayTao,
                        DaHoanThanh = daHoanThanh,
                        DaDanhGia = danhGias.Any(dg => dg.MaSuKien == d.MaSuKien),
                        CoGiayChungNhan = hasCertificate,
                        TenToChuc = organization?.TenToChuc ?? string.Empty,
                        MaToChuc = suKien?.MaToChuc ?? 0
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy lịch sử tham gia sự kiện: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy thống kê tham gia sự kiện của tình nguyện viên
        /// </summary>
        public async Task<EventHistoryStatsDto> GetEventHistoryStatsAsync(int maTNV)
        {
            try
            {
                var donDangKys = await _context.DonDangKy
                    .Where(d => d.MaTNV == maTNV)
                    .Include(d => d.SuKien)
                    .ToListAsync();
                
                // Lấy danh sách giấy chứng nhận
                var giayChungNhans = await _context.GiayChungNhan
                    .Where(g => g.MaTNV == maTNV)
                    .CountAsync();
                
                DateTime now = DateTimeHelper.Now;
                
                var stats = new EventHistoryStatsDto
                {
                    TongSuKien = donDangKys.Count,
                    SuKienDaHoanThanh = donDangKys.Count(d => d.TrangThai == 1 && d.SuKien?.NgayKetThuc.HasValue == true && d.SuKien.NgayKetThuc.Value < now),
                    SuKienDangCho = donDangKys.Count(d => d.TrangThai == 0),
                    SuKienDaHuy = donDangKys.Count(d => d.TrangThai == 2),
                    SoGiayChungNhan = giayChungNhans
                };

                // Thống kê theo tháng trong năm hiện tại
                var currentYear = DateTimeHelper.Now.Year;
                var suKienTheoThang = donDangKys
                    .Select(d => d.SuKien)
                    .Where(s => s?.NgayBatDau.HasValue == true && s.NgayBatDau.Value.Year == currentYear)
                    .GroupBy(s => s!.NgayBatDau!.Value.Month)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                // Đảm bảo có đủ 12 tháng
                for (int i = 1; i <= 12; i++)
                {
                    if (!suKienTheoThang.ContainsKey(i))
                    {
                        suKienTheoThang[i] = 0;
                    }
                }
                
                stats.ThongKeSuKienTheoThang = suKienTheoThang;
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê tham gia sự kiện: {ex.Message}");
                throw;
            }
        }
    }
}
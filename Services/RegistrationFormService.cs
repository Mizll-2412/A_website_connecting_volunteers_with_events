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
                    _logger.LogWarning($"Đã tồn tại đơn đăng ký: MaTNV={createDto.MaTNV}, MaSuKien={createDto.MaSuKien}");
                    throw new Exception("Bạn đã đăng ký tham gia sự kiện này rồi");
                }
                
                // Kiểm tra thời hạn đăng ký
                if (suKien.NgayBatDau.HasValue && DateTime.Now > suKien.NgayBatDau)
                {
                    _logger.LogWarning($"Sự kiện đã bắt đầu: MaSuKien={createDto.MaSuKien}");
                    throw new Exception("Sự kiện đã bắt đầu, không thể đăng ký");
                }
                
                if (suKien.TuyenKetThuc.HasValue && DateTime.Now > suKien.TuyenKetThuc)
                {
                    _logger.LogWarning($"Sự kiện đã hết hạn đăng ký: MaSuKien={createDto.MaSuKien}");
                    throw new Exception("Sự kiện đã hết hạn đăng ký");
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
                    NgayTao = DateTime.Now,
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
                var dons = await _context.DonDangKy
                    .Include(d => d.TinhNguyenVien)
                    .Include(d => d.SuKien)
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
                    .FirstOrDefaultAsync(d => d.MaTNV == maTNV && d.MaSuKien == maSuKien);

                if (don == null)
                    throw new Exception("Đơn đăng ký không tồn tại");

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
                .FirstOrDefaultAsync(d => d.MaTNV == maTNV && d.MaSuKien == maSuKien);
                if (don == null)
                {
                    throw new Exception("Đơn đăng ký không tồn tại");
                }
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
        private DonDangKyResponseDto MapToDto(DonDangKy don)
        {
            return new DonDangKyResponseDto
            {
                MaTNV = don.MaTNV,
                MaSuKien = don.MaSuKien,
                NgayTao = don.NgayTao,
                GhiChu = don.GhiChu,
                TrangThai = don.TrangThai,
                TrangThaiText = GetTrangThaiText(don.TrangThai),
                TenTNV = don.TinhNguyenVien?.HoTen,
                TenSuKien = don.SuKien?.TenSuKien
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
        
        /// <summary>
        /// Lấy lịch sử tham gia sự kiện của tình nguyện viên
        /// </summary>
        public async Task<List<EventHistoryDto>> GetEventHistoryAsync(int maTNV, EventHistoryFilterDto filter = null)
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
                        var currentTime = DateTime.Now;
                        
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
                
                DateTime now = DateTime.Now;
                
                return donDangKys.Select(d => new EventHistoryDto
                {
                    MaSuKien = d.MaSuKien,
                    TenSuKien = d.SuKien?.TenSuKien,
                    NgayBatDau = d.SuKien?.NgayBatDau,
                    NgayKetThuc = d.SuKien?.NgayKetThuc,
                    DiaChi = d.SuKien?.DiaChi,
                    HinhAnh = d.SuKien?.HinhAnh,
                    TrangThaiDangKy = d.TrangThai,
                    TrangThaiDangKyText = GetTrangThaiText(d.TrangThai),
                    NgayDangKy = d.NgayTao,
                    DaHoanThanh = d.SuKien?.NgayKetThuc < now && d.TrangThai == 1, // Đã duyệt và đã kết thúc
                    DaDanhGia = danhGias.Any(dg => dg.MaSuKien == d.MaSuKien),
                    CoGiayChungNhan = giayChungNhans.Any(g => g.MaGiayChungNhan > 0), // Kiểm tra xem có giấy chứng nhận hay không
                    TenToChuc = d.SuKien?.Organization?.TenToChuc,
                    MaToChuc = d.SuKien?.MaToChuc ?? 0
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
                
                DateTime now = DateTime.Now;
                
                var stats = new EventHistoryStatsDto
                {
                    TongSuKien = donDangKys.Count,
                    SuKienDaHoanThanh = donDangKys.Count(d => d.TrangThai == 1 && d.SuKien?.NgayKetThuc < now),
                    SuKienDangCho = donDangKys.Count(d => d.TrangThai == 0),
                    SuKienDaHuy = donDangKys.Count(d => d.TrangThai == 2),
                    SoGiayChungNhan = giayChungNhans
                };

                // Thống kê theo tháng trong năm hiện tại
                var currentYear = DateTime.Now.Year;
                var suKienTheoThang = donDangKys
                    .Where(d => d.SuKien?.NgayBatDau.HasValue == true && 
                              d.SuKien.NgayBatDau.Value.Year == currentYear)
                    .GroupBy(d => d.SuKien.NgayBatDau.Value.Month)
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
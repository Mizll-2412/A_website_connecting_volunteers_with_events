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
                var tnv = await _context.Volunteer.FindAsync(createDto.MaTNV);
                if (tnv == null)
                {
                    throw new Exception("Tình nguyện viên không tồn tại");
                }
                var suKien = await _context.Event.FindAsync(createDto.MaSuKien);
                if (suKien == null)
                {
                    throw new Exception("Sự kiện không tồn tại");
                }
                var existing = await _context.DonDangKy.FirstOrDefaultAsync(d => d.MaTNV == createDto.MaTNV && d.MaSuKien == createDto.MaSuKien);
                if (existing != null)
                {
                    throw new Exception("Bạn đã tham gia sự kiện này rồi");
                }
                if (suKien.TuyenKetThuc.HasValue && DateTime.Now > suKien.TuyenKetThuc)
                    throw new Exception("Sự kiện đã hết hạn đăng ký");
                var soLuongDaDangKy = await _context.DonDangKy
                                    .Where(d => d.MaSuKien == createDto.MaSuKien && d.TrangThai == 1)
                                    .CountAsync();
                if (suKien.SoLuong.HasValue && soLuongDaDangKy >= suKien.SoLuong)
                    throw new Exception("Sự kiện đã đủ số lượng tình nguyện viên");
                var donDangKy = new DonDangKy
                {
                    MaTNV = createDto.MaTNV,
                    MaSuKien = createDto.MaSuKien,
                    NgayTao = DateTime.Now,
                    GhiChu = createDto.GhiChu,
                    TrangThai = 0
                };
                _context.DonDangKy.Add(donDangKy);
                await _context.SaveChangesAsync();

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
    }
}
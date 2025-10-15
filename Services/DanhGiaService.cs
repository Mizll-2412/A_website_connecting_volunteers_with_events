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
    public class DanhGiaService : IDanhGiaService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DanhGiaService> _logger;

        public DanhGiaService(AppDbContext context, ILogger<DanhGiaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DanhGiaResponseDto> TaoMoiDanhGiaAsync(CreateDanhGiaDto createDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var suKien = await _context.Event.FindAsync(createDto.MaSuKien);
                    if (suKien == null)
                        throw new Exception("Sự kiện không tồn tại");

                    if (suKien.NgayKetThuc > DateTime.Now)
                        throw new Exception("Sự kiện chưa kết thúc");

                    // 2. Kiểm tra người đánh giá có tham gia sự kiện không
                    var taiKhoanDanhGia = await _context.User.FindAsync(createDto.MaNguoiDanhGia);
                    var taiKhoanDuocDanhGia = await _context.User.FindAsync(createDto.MaNguoiDuocDanhGia);

                    if (taiKhoanDanhGia == null || taiKhoanDuocDanhGia == null)
                        throw new Exception("Tài khoản không tồn tại");

                    // 3. Xác định ai đánh giá ai
                    bool isTNVDanhGiaToChuc = taiKhoanDanhGia.VaiTro == "User" && taiKhoanDuocDanhGia.VaiTro == "Organization";
                    bool isToChucDanhGiaTNV = taiKhoanDanhGia.VaiTro == "Organization" && taiKhoanDuocDanhGia.VaiTro == "User";

                    if (!isTNVDanhGiaToChuc && !isToChucDanhGiaTNV)
                        throw new Exception("Chỉ tình nguyện viên đánh giá tổ chức hoặc tổ chức đánh giá tình nguyện viên");

                    if (isTNVDanhGiaToChuc)
                    {
                        var tnv = await _context.Volunteer.FirstOrDefaultAsync(t => t.MaTaiKhoan == createDto.MaNguoiDanhGia);
                        var donDangKy = await _context.DonDangKy
                            .FirstOrDefaultAsync(d => d.MaTNV == tnv.MaTNV && 
                                                     d.MaSuKien == createDto.MaSuKien && 
                                                     d.TrangThai == 1);
                        
                        if (donDangKy == null)
                            throw new Exception("Bạn chưa tham gia sự kiện này");

                        if (suKien.MaToChuc != await GetMaToChucFromTaiKhoanAsync(createDto.MaNguoiDuocDanhGia))
                            throw new Exception("Sự kiện không thuộc tổ chức này");
                    }
                    else
                    {
                        var toChuc = await _context.Organization.FirstOrDefaultAsync(t => t.MaTaiKhoan == createDto.MaNguoiDanhGia);
                        if (suKien.MaToChuc != toChuc.MaToChuc)
                            throw new Exception("Bạn không phải chủ sự kiện");

                        var tnv = await _context.Volunteer.FirstOrDefaultAsync(t => t.MaTaiKhoan == createDto.MaNguoiDuocDanhGia);
                        var donDangKy = await _context.DonDangKy
                            .FirstOrDefaultAsync(d => d.MaTNV == tnv.MaTNV && 
                                                     d.MaSuKien == createDto.MaSuKien && 
                                                     d.TrangThai == 1);
                        
                        if (donDangKy == null)
                            throw new Exception("Tình nguyện viên chưa tham gia sự kiện");
                    }

                    var daDanhGia = await _context.DanhGia
                        .AnyAsync(d => d.MaNguoiDanhGia == createDto.MaNguoiDanhGia && 
                                      d.MaNguoiDuocDanhGia == createDto.MaNguoiDuocDanhGia && 
                                      d.MaSuKien == createDto.MaSuKien);
                    
                    if (daDanhGia)
                        throw new Exception("Bạn đã đánh giá rồi");

                    var danhGia = new DanhGia
                    {
                        MaNguoiDanhGia = createDto.MaNguoiDanhGia,
                        MaNguoiDuocDanhGia = createDto.MaNguoiDuocDanhGia,
                        MaSuKien = createDto.MaSuKien,
                        DiemSo = createDto.DiemSo,
                        NoiDung = createDto.NoiDung,
                        NgayTao = DateTime.Now
                    };

                    _context.DanhGia.Add(danhGia);
                    await _context.SaveChangesAsync();

                    await CapNhatDiemTrungBinhAsync(createDto.MaNguoiDuocDanhGia);

                    await transaction.CommitAsync();

                    return await GetDanhGiaAsync(danhGia.MaDanhGia);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi tạo đánh giá: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<DanhGiaResponseDto> CapNhatDanhGiaAsync(int maDanhGia, UpdateDanhGiaDto updateDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var danhGia = await _context.DanhGia.FindAsync(maDanhGia);
                    if (danhGia == null)
                        throw new Exception("Đánh giá không tồn tại");

                    danhGia.DiemSo = updateDto.DiemSo;
                    danhGia.NoiDung = updateDto.NoiDung;

                    await _context.SaveChangesAsync();

                    await CapNhatDiemTrungBinhAsync(danhGia.MaNguoiDuocDanhGia);

                    await transaction.CommitAsync();

                    return await GetDanhGiaAsync(maDanhGia);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi cập nhật đánh giá: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<DanhGiaResponseDto> GetDanhGiaAsync(int maDanhGia)
        {
            try
            {
                var danhGia = await _context.DanhGia
                    .Include(d => d.NguoiDanhGia)
                    .Include(d => d.NguoiDuocDanhGia)
                    .Include(d => d.Event)
                    .FirstOrDefaultAsync(d => d.MaDanhGia == maDanhGia);

                if (danhGia == null)
                    throw new Exception("Đánh giá không tồn tại");

                return MapToDto(danhGia);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy đánh giá: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DanhGiaResponseDto>> GetDanhGiaCuaNguoiAsync(int maNguoi)
        {
            try
            {
                var danhGias = await _context.DanhGia
                    .Include(d => d.NguoiDanhGia)
                    .Include(d => d.NguoiDuocDanhGia)
                    .Include(d => d.Event)
                    .Where(d => d.MaNguoiDuocDanhGia == maNguoi)
                    .OrderByDescending(d => d.NgayTao)
                    .ToListAsync();

                return danhGias.Select(d => MapToDto(d)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách đánh giá: {ex.Message}");
                throw;
            }
        }

        public async Task<ThongKeDanhGiaDto> GetThongKeDanhGiaAsync(int maNguoi)
        {
            try
            {
                var taiKhoan = await _context.User.FindAsync(maNguoi);
                if (taiKhoan == null)
                    throw new Exception("Tài khoản không tồn tại");

                var danhGias = await GetDanhGiaCuaNguoiAsync(maNguoi);

                decimal diemTB = 0;
                string tenNguoi = "";

                if (taiKhoan.VaiTro == "User")
                {
                    var tnv = await _context.Volunteer.FirstOrDefaultAsync(t => t.MaTaiKhoan == maNguoi);
                    diemTB = tnv?.DiemTrungBinh ?? 0;
                    tenNguoi = tnv?.HoTen ?? "";
                }
                else if (taiKhoan.VaiTro == "Organization")
                {
                    var toChuc = await _context.Organization.FirstOrDefaultAsync(t => t.MaTaiKhoan == maNguoi);
                    diemTB = toChuc?.DiemTrungBinh ?? 0;
                    tenNguoi = toChuc?.TenToChuc ?? "";
                }

                return new ThongKeDanhGiaDto
                {
                    MaNguoi = maNguoi,
                    TenNguoi = tenNguoi,
                    DiemTrungBinh = diemTB,
                    TongSoDanhGia = danhGias.Count,
                    DanhSachs = danhGias
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi thống kê: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> XoaDanhGiaAsync(int maDanhGia)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var danhGia = await _context.DanhGia.FindAsync(maDanhGia);
                    if (danhGia == null)
                        throw new Exception("Đánh giá không tồn tại");

                    var maNguoiDuocDanhGia = danhGia.MaNguoiDuocDanhGia;

                    _context.DanhGia.Remove(danhGia);
                    await _context.SaveChangesAsync();

                    await CapNhatDiemTrungBinhAsync(maNguoiDuocDanhGia);

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi xóa đánh giá: {ex.Message}");
                    throw;
                }
            }
        }

        private async Task CapNhatDiemTrungBinhAsync(int maTaiKhoan)
        {
            var danhGias = await _context.DanhGia
                .Where(d => d.MaNguoiDuocDanhGia == maTaiKhoan)
                .ToListAsync();

            if (!danhGias.Any())
                return;

            decimal diemTB = Math.Round((decimal)danhGias.Average(d => d.DiemSo), 1);

            var taiKhoan = await _context.User.FindAsync(maTaiKhoan);
            
            if (taiKhoan.VaiTro == "User")
            {
                var tnv = await _context.Volunteer.FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);
                if (tnv != null)
                {
                    tnv.DiemTrungBinh = diemTB;
                }
            }
            else if (taiKhoan.VaiTro == "Organization")
            {
                var toChuc = await _context.Organization.FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);
                if (toChuc != null)
                {
                    toChuc.DiemTrungBinh = diemTB;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetMaToChucFromTaiKhoanAsync(int maTaiKhoan)
        {
            var toChuc = await _context.Organization.FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);
            return toChuc?.MaToChuc ?? 0;
        }

        private DanhGiaResponseDto MapToDto(DanhGia danhGia)
        {
            return new DanhGiaResponseDto
            {
                MaDanhGia = danhGia.MaDanhGia,
                MaNguoiDanhGia = danhGia.MaNguoiDanhGia,
                TenNguoiDanhGia = GetTenFromTaiKhoan(danhGia.NguoiDanhGia),
                MaNguoiDuocDanhGia = danhGia.MaNguoiDuocDanhGia,
                TenNguoiDuocDanhGia = GetTenFromTaiKhoan(danhGia.NguoiDuocDanhGia),
                MaSuKien = danhGia.MaSuKien,
                TenSuKien = danhGia.Event?.TenSuKien,
                DiemSo = danhGia.DiemSo,
                NoiDung = danhGia.NoiDung,
                NgayTao = danhGia.NgayTao
            };
        }

        private string GetTenFromTaiKhoan(TaiKhoan taiKhoan)
        {
            if (taiKhoan.VaiTro == "User")
            {
                var tnv = _context.Volunteer.FirstOrDefault(t => t.MaTaiKhoan == taiKhoan.MaTaiKhoan);
                return tnv?.HoTen ?? "";
            }
            else if (taiKhoan.VaiTro == "Organization")
            {
                var toChuc = _context.Organization.FirstOrDefault(t => t.MaTaiKhoan == taiKhoan.MaTaiKhoan);
                return toChuc?.TenToChuc ?? "";
            }
            return "";
        }
    }
}
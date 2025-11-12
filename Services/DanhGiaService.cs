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
                    // 1. Kiểm tra sự kiện
                    var suKien = await _context.Event.FindAsync(createDto.MaSuKien);
                    if (suKien == null)
                        throw new KeyNotFoundException("Sự kiện không tồn tại");

                    // Sử dụng DateTime.Now thay vì DateTime.UtcNow vì database lưu theo giờ địa phương
                    if (suKien.NgayKetThuc > DateTime.Now)
                        throw new InvalidOperationException("Sự kiện chưa kết thúc, chưa thể đánh giá");

                    // 2. Kiểm tra tài khoản
                    var taiKhoanDanhGia = await _context.User.FindAsync(createDto.MaNguoiDanhGia);
                    var taiKhoanDuocDanhGia = await _context.User.FindAsync(createDto.MaNguoiDuocDanhGia);

                    if (taiKhoanDanhGia == null || taiKhoanDuocDanhGia == null)
                        throw new KeyNotFoundException("Tài khoản không tồn tại");

                    // Log để debug
                    _logger.LogInformation($"Đánh giá: MaNguoiDanhGia={createDto.MaNguoiDanhGia} (VaiTro={taiKhoanDanhGia.VaiTro}), MaNguoiDuocDanhGia={createDto.MaNguoiDuocDanhGia} (VaiTro={taiKhoanDuocDanhGia.VaiTro})");

                    // 3. Validate vai trò
                    bool isTNVDanhGiaToChuc = taiKhoanDanhGia.VaiTro == "User" && taiKhoanDuocDanhGia.VaiTro == "Organization";
                    bool isToChucDanhGiaTNV = taiKhoanDanhGia.VaiTro == "Organization" && taiKhoanDuocDanhGia.VaiTro == "User";

                    if (!isTNVDanhGiaToChuc && !isToChucDanhGiaTNV)
                        throw new InvalidOperationException($"Chỉ tình nguyện viên có thể đánh giá tổ chức hoặc tổ chức đánh giá tình nguyện viên. Hiện tại: {taiKhoanDanhGia.VaiTro} đánh giá {taiKhoanDuocDanhGia.VaiTro}");

                    // 4. Validate quyền đánh giá
                    if (isTNVDanhGiaToChuc)
                    {
                        await ValidateTNVDanhGiaToChucAsync(createDto.MaNguoiDanhGia, createDto.MaNguoiDuocDanhGia, suKien, createDto.MaSuKien);
                    }
                    else
                    {
                        await ValidateToChucDanhGiaTNVAsync(createDto.MaNguoiDanhGia, createDto.MaNguoiDuocDanhGia, suKien, createDto.MaSuKien);
                    }

                    // 5. Kiểm tra đã đánh giá chưa
                    var daDanhGia = await _context.DanhGia
                        .AnyAsync(d => d.MaNguoiDanhGia == createDto.MaNguoiDanhGia && 
                                      d.MaNguoiDuocDanhGia == createDto.MaNguoiDuocDanhGia && 
                                      d.MaSuKien == createDto.MaSuKien);
                    
                    if (daDanhGia)
                        throw new InvalidOperationException("Bạn đã đánh giá người này trong sự kiện này rồi");

                    // 6. Tạo đánh giá
                    var danhGia = new DanhGia
                    {
                        MaNguoiDanhGia = createDto.MaNguoiDanhGia,
                        MaNguoiDuocDanhGia = createDto.MaNguoiDuocDanhGia,
                        MaSuKien = createDto.MaSuKien,
                        DiemSo = createDto.DiemSo,
                        NoiDung = createDto.NoiDung?.Trim(),
                        NgayTao = DateTime.UtcNow
                    };

                    _context.DanhGia.Add(danhGia);
                    await _context.SaveChangesAsync();

                    // 7. Cập nhật điểm trung bình và cấp bậc
                    await CapNhatDiemVaCapBacAsync(createDto.MaNguoiDuocDanhGia);

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

        public async Task<DanhGiaResponseDto> CapNhatDanhGiaAsync(int maDanhGia, UpdateDanhGiaDto updateDto, int currentUserId, string currentUserRole)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var danhGia = await _context.DanhGia.FindAsync(maDanhGia);
                    if (danhGia == null)
                        throw new KeyNotFoundException("Đánh giá không tồn tại");

                    // Kiểm tra quyền: Chỉ người tạo hoặc Admin mới sửa được
                    if (danhGia.MaNguoiDanhGia != currentUserId && currentUserRole != "Admin")
                        throw new UnauthorizedAccessException("Bạn không có quyền cập nhật đánh giá này");

                    danhGia.DiemSo = updateDto.DiemSo;
                    danhGia.NoiDung = updateDto.NoiDung?.Trim();

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
            var danhGia = await _context.DanhGia
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.Event)
                .FirstOrDefaultAsync(d => d.MaDanhGia == maDanhGia);

            if (danhGia == null)
                throw new KeyNotFoundException("Đánh giá không tồn tại");

            return MapToDto(danhGia);
        }

        public async Task<List<DanhGiaResponseDto>> GetDanhGiaCuaNguoiAsync(int maNguoi)
        {
            var danhGias = await _context.DanhGia
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.Event)
                .Where(d => d.MaNguoiDuocDanhGia == maNguoi)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            var result = new List<DanhGiaResponseDto>();
            foreach (var danhGia in danhGias)
            {
                result.Add(MapToDto(danhGia));
            }
            return result;
        }

        /// <summary>
        /// Lấy tất cả đánh giá theo sự kiện (cho tổ chức xem lại đánh giá đã cho)
        /// </summary>
        public async Task<List<DanhGiaResponseDto>> GetDanhGiaByEventAsync(int maSuKien)
        {
            var danhGias = await _context.DanhGia
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.Event)
                .Where(d => d.MaSuKien == maSuKien)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            var result = new List<DanhGiaResponseDto>();
            foreach (var danhGia in danhGias)
            {
                result.Add(MapToDto(danhGia));
            }
            return result;
        }

        /// <summary>
        /// Lấy các đánh giá mà user nhận được (người khác đánh giá mình)
        /// </summary>
        public async Task<List<DanhGiaResponseDto>> GetDanhGiaNhanDuocAsync(int maUser, StatisticFilterDto? filter = null)
        {
            var query = _context.DanhGia
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.Event)
                .Where(d => d.MaNguoiDuocDanhGia == maUser)
                .AsQueryable();

            // Áp dụng filter nếu có - filter theo NgayTao và DiemSo của đánh giá
            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                    query = query.Where(d => d.NgayTao >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(d => d.NgayTao <= filter.ToDate.Value);

                if (filter.Year.HasValue)
                    query = query.Where(d => d.NgayTao.Year == filter.Year.Value);

                if (filter.Month.HasValue && filter.Year.HasValue)
                    query = query.Where(d => d.NgayTao.Month == filter.Month.Value && d.NgayTao.Year == filter.Year.Value);

                if (filter.MinScore.HasValue)
                    query = query.Where(d => d.DiemSo >= filter.MinScore.Value);

                if (filter.MaxScore.HasValue)
                    query = query.Where(d => d.DiemSo <= filter.MaxScore.Value);
            }

            var danhGias = await query
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            var result = new List<DanhGiaResponseDto>();
            foreach (var danhGia in danhGias)
            {
                result.Add(MapToDto(danhGia));
            }
            return result;
        }

        /// <summary>
        /// Lấy các đánh giá mà user đã đưa ra (đánh giá người khác)
        /// </summary>
        public async Task<List<DanhGiaResponseDto>> GetDanhGiaDaDuaRaAsync(int maUser, StatisticFilterDto? filter = null)
        {
            var query = _context.DanhGia
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.Event)
                .Where(d => d.MaNguoiDanhGia == maUser)
                .AsQueryable();

            // Áp dụng filter nếu có - filter theo NgayTao và DiemSo của đánh giá
            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                    query = query.Where(d => d.NgayTao >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(d => d.NgayTao <= filter.ToDate.Value);

                if (filter.Year.HasValue)
                    query = query.Where(d => d.NgayTao.Year == filter.Year.Value);

                if (filter.Month.HasValue && filter.Year.HasValue)
                    query = query.Where(d => d.NgayTao.Month == filter.Month.Value && d.NgayTao.Year == filter.Year.Value);

                if (filter.MinScore.HasValue)
                    query = query.Where(d => d.DiemSo >= filter.MinScore.Value);

                if (filter.MaxScore.HasValue)
                    query = query.Where(d => d.DiemSo <= filter.MaxScore.Value);
            }

            var danhGias = await query
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            var result = new List<DanhGiaResponseDto>();
            foreach (var danhGia in danhGias)
            {
                result.Add(MapToDto(danhGia));
            }
            return result;
        }

        public async Task<List<DanhGiaResponseDto>> GetAllEvaluationsAsync(StatisticFilterDto? filter = null)
        {
            var query = _context.DanhGia
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Volunteer)
                .Include(d => d.NguoiDuocDanhGia).ThenInclude(u => u.Organization)
                .Include(d => d.Event)
                .AsQueryable();

            // Áp dụng filter nếu có - filter theo NgayTao và DiemSo của đánh giá
            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                    query = query.Where(d => d.NgayTao >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(d => d.NgayTao <= filter.ToDate.Value);

                if (filter.Year.HasValue)
                    query = query.Where(d => d.NgayTao.Year == filter.Year.Value);

                if (filter.Month.HasValue && filter.Year.HasValue)
                    query = query.Where(d => d.NgayTao.Month == filter.Month.Value && d.NgayTao.Year == filter.Year.Value);

                if (filter.MinScore.HasValue)
                    query = query.Where(d => d.DiemSo >= filter.MinScore.Value);

                if (filter.MaxScore.HasValue)
                    query = query.Where(d => d.DiemSo <= filter.MaxScore.Value);
            }

            var danhGias = await query
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            return danhGias.Select(MapToDto).ToList();
        }

        public async Task<ThongKeDanhGiaDto> GetThongKeDanhGiaAsync(int maNguoi)
        {
            var taiKhoan = await _context.User.FindAsync(maNguoi);
            if (taiKhoan == null)
                throw new KeyNotFoundException("Tài khoản không tồn tại");

            var danhGias = await GetDanhGiaCuaNguoiAsync(maNguoi);

            string tenNguoi = string.Empty;
            decimal diemTB = 0;
            string? capBac = null;
            int tongSuKienThamGia = 0;

            if (taiKhoan.VaiTro == "User")
            {
                var tnv = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == maNguoi);
                tenNguoi = tnv?.HoTen ?? "";
                diemTB = tnv?.DiemTrungBinh ?? 0;
                
                // Nếu chưa có cấp bậc, cập nhật
                if (string.IsNullOrEmpty(tnv?.CapBac))
                {
                    await CapNhatDiemVaCapBacAsync(maNguoi);
                    // Refresh từ database sau khi cập nhật
                    tnv = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == maNguoi);
                }
                
                capBac = tnv?.CapBac;
                tongSuKienThamGia = tnv?.TongSuKienThamGia ?? 0;
            }
            else if (taiKhoan.VaiTro == "Organization")
            {
                var org = await _context.Organization.FirstOrDefaultAsync(o => o.MaTaiKhoan == maNguoi);
                tenNguoi = org?.TenToChuc ?? "";
                diemTB = org?.DiemTrungBinh ?? 0;
            }

            return new ThongKeDanhGiaDto
            {
                MaNguoi = maNguoi,
                TenNguoi = tenNguoi,
                DiemTrungBinh = diemTB,
                TongSoDanhGia = danhGias.Count,
                CapBac = capBac,
                TongSuKienThamGia = tongSuKienThamGia,
                DanhSachs = danhGias
            };
        }

        public async Task<bool> XoaDanhGiaAsync(int maDanhGia, int currentUserId, string currentUserRole)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var danhGia = await _context.DanhGia.FindAsync(maDanhGia);
                    if (danhGia == null)
                        throw new KeyNotFoundException("Đánh giá không tồn tại");

                    // Kiểm tra quyền: Chỉ người tạo hoặc Admin mới xóa được
                    if (danhGia.MaNguoiDanhGia != currentUserId && currentUserRole != "Admin")
                        throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này");

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

        // ========== PRIVATE METHODS ==========

        private async Task ValidateTNVDanhGiaToChucAsync(int maTaiKhoanTNV, int maTaiKhoanToChuc, SuKien suKien, int maSuKien)
        {
            var tnv = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == maTaiKhoanTNV);
            if (tnv == null)
                throw new KeyNotFoundException("Tình nguyện viên không tồn tại");

            // Kiểm tra TNV đã tham gia sự kiện
            var donDangKy = await _context.DonDangKy
                .FirstOrDefaultAsync(d => d.MaTNV == tnv.MaTNV && 
                                         d.MaSuKien == maSuKien && 
                                         d.TrangThai == 1);
            
            if (donDangKy == null)
                throw new InvalidOperationException("Bạn chưa tham gia sự kiện này nên không thể đánh giá");

            // Kiểm tra sự kiện thuộc tổ chức được đánh giá
            var toChuc = await _context.Organization.FirstOrDefaultAsync(o => o.MaTaiKhoan == maTaiKhoanToChuc);
            if (toChuc == null || suKien.MaToChuc != toChuc.MaToChuc)
                throw new InvalidOperationException("Sự kiện không thuộc tổ chức này");
        }

        private async Task ValidateToChucDanhGiaTNVAsync(int maTaiKhoanToChuc, int maTaiKhoanTNV, SuKien suKien, int maSuKien)
        {
            var toChuc = await _context.Organization.FirstOrDefaultAsync(o => o.MaTaiKhoan == maTaiKhoanToChuc);
            if (toChuc == null)
                throw new KeyNotFoundException("Tổ chức không tồn tại");

            // Kiểm tra tổ chức là chủ sự kiện
            if (suKien.MaToChuc != toChuc.MaToChuc)
                throw new UnauthorizedAccessException("Bạn không phải chủ sự kiện nên không thể đánh giá");

            // Kiểm tra TNV đã tham gia sự kiện
            var tnv = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == maTaiKhoanTNV);
            if (tnv == null)
                throw new KeyNotFoundException("Tình nguyện viên không tồn tại");

            var donDangKy = await _context.DonDangKy
                .FirstOrDefaultAsync(d => d.MaTNV == tnv.MaTNV && 
                                         d.MaSuKien == maSuKien && 
                                         d.TrangThai == 1);
            
            if (donDangKy == null)
                throw new InvalidOperationException("Tình nguyện viên chưa tham gia sự kiện này");
        }

        private async Task<decimal> CapNhatDiemTrungBinhAsync(int maTaiKhoan)
        {
            var danhGias = await _context.DanhGia
                .Where(d => d.MaNguoiDuocDanhGia == maTaiKhoan)
                .ToListAsync();

            decimal diemTB = 0;
            if (danhGias.Any())
            {
                diemTB = Math.Round((decimal)danhGias.Average(d => d.DiemSo), 1);
            }

            var taiKhoan = await _context.User.FindAsync(maTaiKhoan);
            if (taiKhoan == null)
                return diemTB;

            if (taiKhoan.VaiTro == "User")
            {
                var tnv = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == maTaiKhoan);
                if (tnv != null)
                {
                    tnv.DiemTrungBinh = diemTB;
                }
            }
            else if (taiKhoan.VaiTro == "Organization")
            {
                var org = await _context.Organization.FirstOrDefaultAsync(o => o.MaTaiKhoan == maTaiKhoan);
                if (org != null)
                {
                    org.DiemTrungBinh = diemTB;
                }
            }

            await _context.SaveChangesAsync();
            return diemTB;
        }
        
        public async Task<decimal> TinhDiemTrungBinhAsync(int maNguoi)
        {
            return await CapNhatDiemTrungBinhAsync(maNguoi);
        }
        
        public Task<List<CapBacDto>> GetDanhSachCapBacAsync()
        {
            // Danh sách cấp bậc cố định
            var danhSach = new List<CapBacDto>
            {
                new CapBacDto { Ten = "Tình nguyện viên Mới", DiemTuongUng = 0, MoTa = "Tình nguyện viên mới tham gia hệ thống" },
                new CapBacDto { Ten = "Tình nguyện viên Đồng", DiemTuongUng = 2, MoTa = "Đã tham gia ít nhất 1 sự kiện và có đánh giá trung bình từ 2 sao" },
                new CapBacDto { Ten = "Tình nguyện viên Bạc", DiemTuongUng = 3, MoTa = "Có đánh giá trung bình từ 3 sao" },
                new CapBacDto { Ten = "Tình nguyện viên Vàng", DiemTuongUng = 4, MoTa = "Có đánh giá trung bình từ 4 sao" },
                new CapBacDto { Ten = "Tình nguyện viên Kim Cương", DiemTuongUng = 4.5M, MoTa = "Có đánh giá trung bình từ 4.5 sao" }
            };

            return Task.FromResult(danhSach);
        }
        
        public async Task<string> GetCapBacTheoSoSaoAsync(decimal soSao)
        {
            var danhSachCapBac = await GetDanhSachCapBacAsync();
            
            // Tìm cấp bậc phù hợp dựa trên số sao
            var capBac = danhSachCapBac
                .OrderByDescending(c => c.DiemTuongUng)
                .FirstOrDefault(c => c.DiemTuongUng <= soSao);
                
            return capBac?.Ten ?? "Tình nguyện viên Mới";
        }
        
        public async Task<bool> CapNhatDiemVaCapBacAsync(int maNguoi)
        {
            try
            {
                // Cập nhật điểm trung bình
                var diemTB = await CapNhatDiemTrungBinhAsync(maNguoi);
                
                // Lấy tài khoản
                var taiKhoan = await _context.User.FindAsync(maNguoi);
                if (taiKhoan == null)
                    return false;
                    
                // Chỉ cập nhật cấp bậc cho tình nguyện viên
                if (taiKhoan.VaiTro == "User")
                {
                    var tnv = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == maNguoi);
                    if (tnv != null)
                    {
                        // Lấy cấp bậc dựa trên số sao
                        var capBac = await GetCapBacTheoSoSaoAsync(diemTB);
                        tnv.CapBac = capBac;
                        
                        // Cập nhật tổng số sự kiện đã tham gia
                        var tongSuKien = await _context.DonDangKy
                            .CountAsync(d => d.MaTNV == tnv.MaTNV && d.TrangThai == 1); // Đã duyệt
                            
                        tnv.TongSuKienThamGia = tongSuKien;
                        
                        await _context.SaveChangesAsync();
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật điểm và cấp bậc: {ex.Message}");
                return false;
            }
        }

        private DanhGiaResponseDto MapToDto(DanhGia danhGia)
        {
            var nguoiDanhGia = danhGia.NguoiDanhGia;
            var nguoiDuocDanhGia = danhGia.NguoiDuocDanhGia;

            string tenNguoiDanhGia = string.Empty;
            string vaiTroNguoiDanhGia = nguoiDanhGia?.VaiTro ?? string.Empty;
            if (vaiTroNguoiDanhGia == "User")
            {
                tenNguoiDanhGia = nguoiDanhGia?.Volunteer?.HoTen ?? string.Empty;
            }
            else if (vaiTroNguoiDanhGia == "Organization")
            {
                tenNguoiDanhGia = nguoiDanhGia?.Organization?.TenToChuc ?? string.Empty;
            }

            string tenNguoiDuocDanhGia = string.Empty;
            string vaiTroNguoiDuocDanhGia = nguoiDuocDanhGia?.VaiTro ?? string.Empty;
            if (vaiTroNguoiDuocDanhGia == "User")
            {
                tenNguoiDuocDanhGia = nguoiDuocDanhGia?.Volunteer?.HoTen ?? string.Empty;
            }
            else if (vaiTroNguoiDuocDanhGia == "Organization")
            {
                tenNguoiDuocDanhGia = nguoiDuocDanhGia?.Organization?.TenToChuc ?? string.Empty;
            }

            return new DanhGiaResponseDto
            {
                MaDanhGia = danhGia.MaDanhGia,
                MaNguoiDanhGia = danhGia.MaNguoiDanhGia,
                TenNguoiDanhGia = tenNguoiDanhGia,
                VaiTroNguoiDanhGia = vaiTroNguoiDanhGia,
                MaNguoiDuocDanhGia = danhGia.MaNguoiDuocDanhGia,
                TenNguoiDuocDanhGia = tenNguoiDuocDanhGia,
                VaiTroNguoiDuocDanhGia = vaiTroNguoiDuocDanhGia,
                MaSuKien = danhGia.MaSuKien,
                TenSuKien = danhGia.Event?.TenSuKien,
                DiemSo = danhGia.DiemSo,
                NoiDung = danhGia.NoiDung,
                NgayTao = danhGia.NgayTao
            };
        }

        public async Task<List<DanhGiaResponseDto>> TaoNhieuDanhGiaAsync(BulkCreateDanhGiaDto bulkDto, int maNguoiDanhGia)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Kiểm tra sự kiện
                    var suKien = await _context.Event.FindAsync(bulkDto.MaSuKien);
                    if (suKien == null)
                        throw new KeyNotFoundException("Sự kiện không tồn tại");

                    // Kiểm tra sự kiện đã kết thúc
                    if (suKien.NgayKetThuc > DateTime.Now)
                        throw new InvalidOperationException("Sự kiện chưa kết thúc, chưa thể đánh giá");

                    // 2. Kiểm tra tài khoản đánh giá (tổ chức)
                    var taiKhoanDanhGia = await _context.User.FindAsync(maNguoiDanhGia);
                    if (taiKhoanDanhGia == null || taiKhoanDanhGia.VaiTro != "Organization")
                        throw new InvalidOperationException("Chỉ tổ chức mới có thể đánh giá hàng loạt");

                    var results = new List<DanhGiaResponseDto>();
                    var updatedVolunteers = new HashSet<int>();

                    // 3. Lấy danh sách TNV và kiểm tra
                    var tnvs = await _context.Volunteer
                        .Where(v => bulkDto.MaTNVs.Contains(v.MaTNV))
                        .Include(v => v.TaiKhoan)
                        .ToListAsync();

                    if (tnvs.Count == 0)
                        throw new KeyNotFoundException("Không tìm thấy tình nguyện viên nào trong danh sách");

                    if (tnvs.Count < bulkDto.MaTNVs.Count)
                    {
                        var foundIds = tnvs.Select(t => t.MaTNV).ToList();
                        var missingIds = bulkDto.MaTNVs.Except(foundIds).ToList();
                        _logger.LogWarning($"Một số tình nguyện viên không tồn tại: {string.Join(", ", missingIds)}");
                    }

                    // 4. Kiểm tra từng TNV có tham gia sự kiện và đã được duyệt chưa
                    foreach (var tnv in tnvs)
                    {
                        var donDangKy = await _context.DonDangKy
                            .FirstOrDefaultAsync(d => d.MaTNV == tnv.MaTNV && d.MaSuKien == bulkDto.MaSuKien && d.TrangThai == 1);

                        if (donDangKy == null)
                        {
                            _logger.LogWarning($"TNV {tnv.MaTNV} chưa tham gia hoặc chưa được duyệt trong sự kiện {bulkDto.MaSuKien}");
                            continue; // Bỏ qua TNV này
                        }

                        // 5. Kiểm tra đã đánh giá chưa
                        var daDanhGia = await _context.DanhGia
                            .AnyAsync(d => d.MaNguoiDanhGia == maNguoiDanhGia &&
                                          d.MaNguoiDuocDanhGia == tnv.MaTaiKhoan &&
                                          d.MaSuKien == bulkDto.MaSuKien);

                        if (daDanhGia)
                        {
                            _logger.LogWarning($"Đã đánh giá TNV {tnv.MaTNV} trong sự kiện {bulkDto.MaSuKien}");
                            continue; // Bỏ qua TNV đã được đánh giá
                        }

                        // 6. Tạo đánh giá
                        var danhGia = new DanhGia
                        {
                            MaNguoiDanhGia = maNguoiDanhGia,
                            MaNguoiDuocDanhGia = tnv.MaTaiKhoan,
                            MaSuKien = bulkDto.MaSuKien,
                            DiemSo = bulkDto.DiemSo,
                            NoiDung = bulkDto.NoiDung?.Trim(),
                            NgayTao = DateTime.UtcNow
                        };

                        _context.DanhGia.Add(danhGia);
                        updatedVolunteers.Add(tnv.MaTaiKhoan);
                    }

                    await _context.SaveChangesAsync();

                    // 7. Cập nhật điểm trung bình và cấp bậc cho các TNV đã được đánh giá
                    foreach (var maTaiKhoan in updatedVolunteers)
                    {
                        await CapNhatDiemVaCapBacAsync(maTaiKhoan);
                    }

                    // 8. Lấy danh sách đánh giá vừa tạo
                    var maDanhGias = await _context.DanhGia
                        .Where(d => d.MaNguoiDanhGia == maNguoiDanhGia &&
                                   d.MaSuKien == bulkDto.MaSuKien &&
                                   updatedVolunteers.Contains(d.MaNguoiDuocDanhGia) &&
                                   d.NgayTao >= DateTime.UtcNow.AddMinutes(-1)) // Lấy các đánh giá vừa tạo
                        .Select(d => d.MaDanhGia)
                        .ToListAsync();

                    foreach (var maDanhGia in maDanhGias)
                    {
                        var danhGia = await GetDanhGiaAsync(maDanhGia);
                        results.Add(danhGia);
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation($"Đã tạo {results.Count} đánh giá cho {bulkDto.MaTNVs.Count} TNV trong sự kiện {bulkDto.MaSuKien}");

                    return results;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi tạo đánh giá hàng loạt: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
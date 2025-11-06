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
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách thông báo của người dùng
        /// </summary>
        public async Task<List<NotificationResponseDto>> GetNotificationsAsync(int userId, bool? read = null)
        {
            try
            {
                var query = _context.NguoiNhanThongBao
                    .Where(n => n.MaNguoiNhanThongBao == userId)
                    .Include(n => n.ThongBao)
                    .ThenInclude(t => t.TaiKhoan)
                    .AsQueryable();

                if (read.HasValue)
                {
                    byte trangThai = read.Value ? (byte)1 : (byte)0;
                    query = query.Where(n => n.TrangThai == trangThai);
                }

                var notifications = await query
                    .OrderByDescending(n => n.ThongBao.NgayGui)
                    .ToListAsync();

                return notifications.Select(n => 
                {
                    // Lấy thông tin người tạo
                    string tenNguoiTao = "Hệ thống";
                    string anhDaiDienNguoiTao = null;
                    
                    if (n.ThongBao.MaNguoiTao > 0)
                    {
                        var taiKhoanNguoiTao = n.ThongBao.TaiKhoan;
                        if (taiKhoanNguoiTao != null)
                        {
                            if (taiKhoanNguoiTao.VaiTro == "Organization")
                            {
                                var toChuc = _context.Organization
                                    .FirstOrDefault(t => t.MaTaiKhoan == taiKhoanNguoiTao.MaTaiKhoan);
                                
                                if (toChuc != null)
                                {
                                    tenNguoiTao = toChuc.TenToChuc;
                                    anhDaiDienNguoiTao = toChuc.AnhDaiDien;
                                }
                            }
                            else if (taiKhoanNguoiTao.VaiTro == "User")
                            {
                                var tnv = _context.Volunteer
                                    .FirstOrDefault(t => t.MaTaiKhoan == taiKhoanNguoiTao.MaTaiKhoan);
                                
                                if (tnv != null)
                                {
                                    tenNguoiTao = tnv.HoTen;
                                    anhDaiDienNguoiTao = tnv.AnhDaiDien;
                                }
                            }
                            else if (taiKhoanNguoiTao.VaiTro == "Admin")
                            {
                                var admin = _context.Admin
                                    .FirstOrDefault(a => a.MaTaiKhoan == taiKhoanNguoiTao.MaTaiKhoan);
                                
                                if (admin != null)
                                {
                                    tenNguoiTao = admin.HoTen;
                                    anhDaiDienNguoiTao = null; // Admin không có ảnh đại diện
                                }
                                else
                                {
                                    tenNguoiTao = "Quản trị viên";
                                }
                            }
                        }
                    }
                    
                    return new NotificationResponseDto
                    {
                        MaThongBao = n.ThongBao.MaThongBao,
                        MaNguoiTao = n.ThongBao.MaNguoiTao,
                        TenNguoiTao = tenNguoiTao,
                        AnhDaiDienNguoiTao = anhDaiDienNguoiTao,
                        PhanLoai = n.ThongBao.PhanLoai,
                        PhanLoaiText = GetPhanLoaiText(n.ThongBao.PhanLoai),
                        NoiDung = n.ThongBao.NoiDung,
                        NgayGui = n.ThongBao.NgayGui,
                        TrangThai = n.TrangThai,
                        TrangThaiText = GetTrangThaiText(n.TrangThai),
                        MaNguoiNhanThongBao = n.MaNguoiNhanThongBao
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thông báo: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy số lượng thông báo của người dùng
        /// </summary>
        public async Task<NotificationCountDto> GetNotificationCountAsync(int userId)
        {
            try
            {
                var notifications = await _context.NguoiNhanThongBao
                    .Where(n => n.MaNguoiNhanThongBao == userId)
                    .ToListAsync();

                return new NotificationCountDto
                {
                    TongSo = notifications.Count,
                    ChuaDoc = notifications.Count(n => n.TrangThai == 0),
                    DaDoc = notifications.Count(n => n.TrangThai == 1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy số lượng thông báo: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tạo thông báo mới
        /// </summary>
        public async Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Kiểm tra người dùng
                    var nguoiTao = await _context.User.FindAsync(createDto.MaNguoiTao);
                    if (nguoiTao == null)
                    {
                        throw new Exception("Người tạo không tồn tại");
                    }

                    // Tạo thông báo
                    var thongBao = new ThongBao
                    {
                        MaNguoiTao = createDto.MaNguoiTao,
                        PhanLoai = createDto.PhanLoai,
                        NoiDung = createDto.NoiDung,
                        NgayGui = DateTime.Now
                    };

                    _context.ThongBao.Add(thongBao);
                    await _context.SaveChangesAsync();

                    // Thêm người nhận thông báo
                    foreach (var maNguoiNhan in createDto.MaNguoiNhans)
                    {
                        var nguoiNhan = await _context.User.FindAsync(maNguoiNhan);
                        if (nguoiNhan == null) continue;

                        var nguoiNhanThongBao = new NguoiNhanThongBao
                        {
                            MaThongBao = thongBao.MaThongBao,
                            MaNguoiNhanThongBao = maNguoiNhan,
                            TrangThai = 0 // Chưa đọc
                        };

                        _context.NguoiNhanThongBao.Add(nguoiNhanThongBao);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Lấy thông tin người tạo
                    string tenNguoiTao = "Hệ thống";
                    string anhDaiDienNguoiTao = null;
                    
                    if (nguoiTao.VaiTro == "Organization")
                    {
                        var toChuc = await _context.Organization
                            .FirstOrDefaultAsync(t => t.MaTaiKhoan == nguoiTao.MaTaiKhoan);
                        
                        if (toChuc != null)
                        {
                            tenNguoiTao = toChuc.TenToChuc;
                            anhDaiDienNguoiTao = toChuc.AnhDaiDien;
                        }
                    }
                    else if (nguoiTao.VaiTro == "User")
                    {
                        var tnv = await _context.Volunteer
                            .FirstOrDefaultAsync(t => t.MaTaiKhoan == nguoiTao.MaTaiKhoan);
                        
                        if (tnv != null)
                        {
                            tenNguoiTao = tnv.HoTen;
                            anhDaiDienNguoiTao = tnv.AnhDaiDien;
                        }
                    }
                    else if (nguoiTao.VaiTro == "Admin")
                    {
                        var admin = await _context.Admin
                            .FirstOrDefaultAsync(a => a.MaTaiKhoan == nguoiTao.MaTaiKhoan);
                        
                        if (admin != null)
                        {
                            tenNguoiTao = admin.HoTen;
                        }
                        else
                        {
                            tenNguoiTao = "Quản trị viên";
                        }
                    }

                    return new NotificationResponseDto
                    {
                        MaThongBao = thongBao.MaThongBao,
                        MaNguoiTao = thongBao.MaNguoiTao,
                        TenNguoiTao = tenNguoiTao,
                        AnhDaiDienNguoiTao = anhDaiDienNguoiTao,
                        PhanLoai = thongBao.PhanLoai,
                        PhanLoaiText = GetPhanLoaiText(thongBao.PhanLoai),
                        NoiDung = thongBao.NoiDung,
                        NgayGui = thongBao.NgayGui,
                        TrangThai = 0, // Mặc định là chưa đọc
                        TrangThaiText = "Chưa đọc",
                        MaNguoiNhanThongBao = createDto.MaNguoiNhans.FirstOrDefault()
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi tạo thông báo: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái thông báo
        /// </summary>
        public async Task<NotificationResponseDto> UpdateNotificationStatusAsync(int userId, UpdateNotificationStatusDto updateDto)
        {
            try
            {
                var notification = await _context.NguoiNhanThongBao
                    .Include(n => n.ThongBao)
                    .FirstOrDefaultAsync(n => 
                        n.MaNguoiNhanThongBao == userId && 
                        n.MaThongBao == updateDto.MaThongBao);

                if (notification == null)
                {
                    throw new Exception("Thông báo không tồn tại");
                }

                notification.TrangThai = updateDto.TrangThai;
                await _context.SaveChangesAsync();

                // Lấy thông tin người tạo
                string tenNguoiTao = "Hệ thống";
                string anhDaiDienNguoiTao = null;
                
                var nguoiTao = await _context.User.FindAsync(notification.ThongBao.MaNguoiTao);
                if (nguoiTao != null)
                {
                    if (nguoiTao.VaiTro == "Organization")
                    {
                        var toChuc = await _context.Organization
                            .FirstOrDefaultAsync(t => t.MaTaiKhoan == nguoiTao.MaTaiKhoan);
                        
                        if (toChuc != null)
                        {
                            tenNguoiTao = toChuc.TenToChuc;
                            anhDaiDienNguoiTao = toChuc.AnhDaiDien;
                        }
                    }
                    else if (nguoiTao.VaiTro == "User")
                    {
                        var tnv = await _context.Volunteer
                            .FirstOrDefaultAsync(t => t.MaTaiKhoan == nguoiTao.MaTaiKhoan);
                        
                        if (tnv != null)
                        {
                            tenNguoiTao = tnv.HoTen;
                            anhDaiDienNguoiTao = tnv.AnhDaiDien;
                        }
                    }
                    else if (nguoiTao.VaiTro == "Admin")
                    {
                        var admin = await _context.Admin
                            .FirstOrDefaultAsync(a => a.MaTaiKhoan == nguoiTao.MaTaiKhoan);
                        
                        if (admin != null)
                        {
                            tenNguoiTao = admin.HoTen;
                        }
                        else
                        {
                            tenNguoiTao = "Quản trị viên";
                        }
                    }
                }

                return new NotificationResponseDto
                {
                    MaThongBao = notification.ThongBao.MaThongBao,
                    MaNguoiTao = notification.ThongBao.MaNguoiTao,
                    TenNguoiTao = tenNguoiTao,
                    AnhDaiDienNguoiTao = anhDaiDienNguoiTao,
                    PhanLoai = notification.ThongBao.PhanLoai,
                    PhanLoaiText = GetPhanLoaiText(notification.ThongBao.PhanLoai),
                    NoiDung = notification.ThongBao.NoiDung,
                    NgayGui = notification.ThongBao.NgayGui,
                    TrangThai = notification.TrangThai,
                    TrangThaiText = GetTrangThaiText(notification.TrangThai),
                    MaNguoiNhanThongBao = notification.MaNguoiNhanThongBao
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật trạng thái thông báo: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Xóa một thông báo
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int userId, int notificationId)
        {
            try
            {
                var notification = await _context.NguoiNhanThongBao
                    .FirstOrDefaultAsync(n => 
                        n.MaNguoiNhanThongBao == userId && 
                        n.MaThongBao == notificationId);

                if (notification == null)
                {
                    throw new Exception("Thông báo không tồn tại");
                }

                _context.NguoiNhanThongBao.Remove(notification);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa thông báo: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo là đã đọc
        /// </summary>
        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var notifications = await _context.NguoiNhanThongBao
                    .Where(n => n.MaNguoiNhanThongBao == userId && n.TrangThai == 0)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.TrangThai = 1; // Đã đọc
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đánh dấu tất cả thông báo: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gửi thông báo về sự kiện
        /// </summary>
        public async Task<bool> SendEventNotificationAsync(int eventId, string action, List<int> recipientIds)
        {
            try
            {
                var suKien = await _context.Event
                    .Include(s => s.Organization)
                    .FirstOrDefaultAsync(s => s.MaSuKien == eventId);

                if (suKien == null)
                {
                    _logger.LogError($"Không tìm thấy sự kiện: {eventId}");
                    return false;
                }

                string noiDung = "";
                switch (action.ToLower())
                {
                    case "create":
                        noiDung = $"Sự kiện mới: {suKien.TenSuKien} đã được tạo.";
                        break;
                    case "update":
                        noiDung = $"Sự kiện {suKien.TenSuKien} đã được cập nhật.";
                        break;
                    case "delete":
                        noiDung = $"Sự kiện {suKien.TenSuKien} đã bị hủy.";
                        break;
                    default:
                        noiDung = $"Có cập nhật về sự kiện {suKien.TenSuKien}.";
                        break;
                }

                var createDto = new CreateNotificationDto
                {
                    MaNguoiTao = suKien.Organization?.MaTaiKhoan ?? 0,
                    PhanLoai = 2, // Thông báo sự kiện
                    NoiDung = noiDung,
                    MaNguoiNhans = recipientIds
                };

                await CreateNotificationAsync(createDto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi gửi thông báo sự kiện: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo về đăng ký tham gia sự kiện
        /// </summary>
        public async Task<bool> SendRegistrationNotificationAsync(int eventId, int userId, string action)
        {
            try
            {
                var suKien = await _context.Event
                    .Include(s => s.Organization)
                    .FirstOrDefaultAsync(s => s.MaSuKien == eventId);

                if (suKien == null)
                {
                    _logger.LogError($"Không tìm thấy sự kiện: {eventId}");
                    return false;
                }

                var tnv = await _context.Volunteer
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == userId);

                if (tnv == null)
                {
                    _logger.LogError($"Không tìm thấy tình nguyện viên: {userId}");
                    return false;
                }

                string noiDung = "";
                List<int> recipientIds = new List<int>();
                
                switch (action.ToLower())
                {
                    case "register":
                        noiDung = $"Bạn đã đăng ký tham gia sự kiện: {suKien.TenSuKien}.";
                        recipientIds.Add(userId); // Gửi cho TNV
                        break;
                    case "approve":
                        noiDung = $"Đơn đăng ký tham gia sự kiện {suKien.TenSuKien} của bạn đã được duyệt.";
                        recipientIds.Add(userId); // Gửi cho TNV
                        break;
                    case "reject":
                        noiDung = $"Đơn đăng ký tham gia sự kiện {suKien.TenSuKien} của bạn đã bị từ chối.";
                        recipientIds.Add(userId); // Gửi cho TNV
                        break;
                    case "new_registration":
                        noiDung = $"{tnv.HoTen} đã đăng ký tham gia sự kiện {suKien.TenSuKien}.";
                        recipientIds.Add(suKien.Organization?.MaTaiKhoan ?? 0); // Gửi cho BTC
                        break;
                    default:
                        return false;
                }

                var createDto = new CreateNotificationDto
                {
                    MaNguoiTao = action.ToLower() == "new_registration" ? userId : suKien.Organization?.MaTaiKhoan ?? 0,
                    PhanLoai = 2, // Thông báo sự kiện
                    NoiDung = noiDung,
                    MaNguoiNhans = recipientIds
                };

                await CreateNotificationAsync(createDto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi gửi thông báo đăng ký: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo về đánh giá
        /// </summary>
        public async Task<bool> SendEvaluationNotificationAsync(int evaluationId, int recipientId)
        {
            try
            {
                var danhGia = await _context.DanhGia
                    .Include(d => d.NguoiDanhGia)
                    .Include(d => d.NguoiDuocDanhGia)
                    .FirstOrDefaultAsync(d => d.MaDanhGia == evaluationId);

                if (danhGia == null)
                {
                    _logger.LogError($"Không tìm thấy đánh giá: {evaluationId}");
                    return false;
                }

                string tenNguoiDanhGia = "Ai đó";
                
                if (danhGia.NguoiDanhGia.VaiTro == "Organization")
                {
                    var toChuc = await _context.Organization
                        .FirstOrDefaultAsync(t => t.MaTaiKhoan == danhGia.MaNguoiDanhGia);
                    
                    if (toChuc != null)
                    {
                        tenNguoiDanhGia = toChuc.TenToChuc;
                    }
                }
                else if (danhGia.NguoiDanhGia.VaiTro == "User")
                {
                    var tnv = await _context.Volunteer
                        .FirstOrDefaultAsync(t => t.MaTaiKhoan == danhGia.MaNguoiDanhGia);
                    
                    if (tnv != null)
                    {
                        tenNguoiDanhGia = tnv.HoTen;
                    }
                }

                string noiDung = $"{tenNguoiDanhGia} đã đánh giá bạn {danhGia.DiemSo} sao.";

                var createDto = new CreateNotificationDto
                {
                    MaNguoiTao = danhGia.MaNguoiDanhGia,
                    PhanLoai = 3, // Thông báo đánh giá
                    NoiDung = noiDung,
                    MaNguoiNhans = new List<int> { recipientId }
                };

                await CreateNotificationAsync(createDto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi gửi thông báo đánh giá: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy text phân loại thông báo
        /// </summary>
        private string GetPhanLoaiText(int phanLoai)
        {
            switch (phanLoai)
            {
                case 1:
                    return "Hệ thống";
                case 2:
                    return "Sự kiện";
                case 3:
                    return "Đánh giá";
                case 4:
                    return "Chứng nhận";
                default:
                    return "Khác";
            }
        }

        /// <summary>
        /// Lấy text trạng thái thông báo
        /// </summary>
        private string GetTrangThaiText(byte? trangThai)
        {
            switch (trangThai)
            {
                case 0:
                    return "Chưa đọc";
                case 1:
                    return "Đã đọc";
                default:
                    return "Không xác định";
            }
        }

        /// <summary>
        /// Mời TNV tham gia sự kiện
        /// </summary>
        public async Task<NotificationResponseDto> InviteVolunteerToEventAsync(int organizationUserId, InviteEventDto inviteDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Kiểm tra tổ chức
                    var organization = await _context.Organization
                        .FirstOrDefaultAsync(o => o.MaTaiKhoan == organizationUserId);
                    
                    if (organization == null)
                    {
                        throw new Exception("Tổ chức không tồn tại");
                    }

                    // Kiểm tra sự kiện
                    var suKien = await _context.Event
                        .FirstOrDefaultAsync(e => e.MaSuKien == inviteDto.MaSuKien && e.MaToChuc == organization.MaToChuc);
                    
                    if (suKien == null)
                    {
                        throw new Exception("Sự kiện không tồn tại hoặc không thuộc về tổ chức của bạn");
                    }

                    // Kiểm tra TNV
                    var volunteer = await _context.Volunteer
                        .FirstOrDefaultAsync(v => v.MaTaiKhoan == inviteDto.MaNguoiNhan);
                    
                    if (volunteer == null)
                    {
                        throw new Exception("Tình nguyện viên không tồn tại");
                    }

                    // Tạo nội dung thông báo
                    string noiDung = $"Bạn đã được tổ chức {organization.TenToChuc} mời tham gia sự kiện \"{suKien.TenSuKien}\"";

                    // Tạo thông báo
                    var thongBao = new ThongBao
                    {
                        MaNguoiTao = organizationUserId,
                        PhanLoai = 2, // Thông báo sự kiện
                        NoiDung = noiDung,
                        NgayGui = DateTime.Now
                    };

                    _context.ThongBao.Add(thongBao);
                    await _context.SaveChangesAsync();

                    // Thêm người nhận thông báo
                    var nguoiNhanThongBao = new NguoiNhanThongBao
                    {
                        MaThongBao = thongBao.MaThongBao,
                        MaNguoiNhanThongBao = inviteDto.MaNguoiNhan,
                        TrangThai = 0 // Chưa đọc
                    };

                    _context.NguoiNhanThongBao.Add(nguoiNhanThongBao);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new NotificationResponseDto
                    {
                        MaThongBao = thongBao.MaThongBao,
                        MaNguoiTao = thongBao.MaNguoiTao,
                        TenNguoiTao = organization.TenToChuc,
                        AnhDaiDienNguoiTao = organization.AnhDaiDien,
                        PhanLoai = thongBao.PhanLoai,
                        PhanLoaiText = GetPhanLoaiText(thongBao.PhanLoai),
                        NoiDung = thongBao.NoiDung,
                        NgayGui = thongBao.NgayGui,
                        TrangThai = 0,
                        TrangThaiText = "Chưa đọc",
                        MaNguoiNhanThongBao = inviteDto.MaNguoiNhan
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Lỗi mời TNV tham gia sự kiện: {ex.Message}");
                    throw;
                }
            }
        }
    }
}

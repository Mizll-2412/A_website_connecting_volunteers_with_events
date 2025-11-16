using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public AdminService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Quản lý tài khoản
        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            return await _context.User
                .Select(u => new AdminUserDto
                {
                    MaTaiKhoan = u.MaTaiKhoan,
                    Email = u.Email,
                    VaiTro = u.VaiTro,
                    TrangThai = u.TrangThai,
                    NgayTao = u.NgayTao,
                    LanDangNhapCuoi = u.LanDangNhapCuoi,
                    Volunteer = u.Volunteer != null
                        ? new AdminUserVolunteerDto
                        {
                            MaTNV = u.Volunteer.MaTNV,
                            HoTen = u.Volunteer.HoTen,
                            AnhDaiDien = u.Volunteer.AnhDaiDien
                        }
                        : null,
                    Organization = u.Organization != null
                        ? new AdminUserOrganizationDto
                        {
                            MaToChuc = u.Organization.MaToChuc,
                            TenToChuc = u.Organization.TenToChuc,
                            AnhDaiDien = u.Organization.AnhDaiDien
                        }
                        : null
                })
                .ToListAsync();
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(int id)
        {
            return await _context.User
                .Where(u => u.MaTaiKhoan == id)
                .Select(u => new AdminUserDto
                {
                    MaTaiKhoan = u.MaTaiKhoan,
                    Email = u.Email,
                    VaiTro = u.VaiTro,
                    TrangThai = u.TrangThai,
                    NgayTao = u.NgayTao,
                    LanDangNhapCuoi = u.LanDangNhapCuoi,
                    Volunteer = u.Volunteer != null
                        ? new AdminUserVolunteerDto
                        {
                            MaTNV = u.Volunteer.MaTNV,
                            HoTen = u.Volunteer.HoTen,
                            AnhDaiDien = u.Volunteer.AnhDaiDien
                        }
                        : null,
                    Organization = u.Organization != null
                        ? new AdminUserOrganizationDto
                        {
                            MaToChuc = u.Organization.MaToChuc,
                            TenToChuc = u.Organization.TenToChuc,
                            AnhDaiDien = u.Organization.AnhDaiDien
                        }
                        : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateUserRoleAsync(int id, string role)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == id);
            if (user == null)
                return false;

            // Kiểm tra vai trò hợp lệ
            if (role != "User" && role != "Organization" && role != "Admin")
                return false;

            user.VaiTro = role;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserStatusAsync(int id, bool status)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == id);
            if (user == null)
                return false;

            user.TrangThai = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == id);
            if (user == null)
                return false;

            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == userId);
            if (user == null)
                return false;

            // Tạo salt mới và hash mật khẩu mới (giống cách AuthService)
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            var salt = Convert.ToBase64String(saltBytes);
            
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(newPassword + salt);
                var hashBytes = sha256.ComputeHash(passwordBytes);
                var hashedPassword = Convert.ToBase64String(hashBytes);
                
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        // Quản lý tổ chức
        public async Task<List<ToChuc>> GetAllOrganizationsAsync()
        {
            try 
            {
                // Truy vấn trực tiếp từ database và map thành DTO để tránh circular reference
                var organizations = await _context.Organization
                    .Select(o => new ToChuc
                    {
                        MaToChuc = o.MaToChuc,
                        MaTaiKhoan = o.MaTaiKhoan,
                        TenToChuc = o.TenToChuc,
                        Email = o.Email,
                        SoDienThoai = o.SoDienThoai,
                        DiaChi = o.DiaChi,
                        NgayTao = o.NgayTao,
                        GioiThieu = o.GioiThieu,
                        AnhDaiDien = o.AnhDaiDien,
                        TrangThaiXacMinh = o.TrangThaiXacMinh,
                        LyDoTuChoi = o.LyDoTuChoi,
                        DiemTrungBinh = o.DiemTrungBinh,
                        TaiKhoan = new TaiKhoan
                        {
                            MaTaiKhoan = o.TaiKhoan.MaTaiKhoan,
                            Email = o.TaiKhoan.Email,
                            VaiTro = o.TaiKhoan.VaiTro,
                            TrangThai = o.TaiKhoan.TrangThai,
                            NgayTao = o.TaiKhoan.NgayTao,
                            LanDangNhapCuoi = o.TaiKhoan.LanDangNhapCuoi
                        }
                    })
                    .ToListAsync();

                return organizations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách tất cả tổ chức: {ex.Message}");
                return new List<ToChuc>();
            }
        }

        public async Task<List<ToChuc>> GetPendingOrganizationsAsync()
        {
            try 
            {
                // Truy vấn trực tiếp từ database và map thành DTO để tránh circular reference
                return await _context.Organization
                    .Where(o => o.TrangThaiXacMinh == 0 || o.TrangThaiXacMinh == null)
                    .Select(o => new ToChuc
                    {
                        MaToChuc = o.MaToChuc,
                        MaTaiKhoan = o.MaTaiKhoan,
                        TenToChuc = o.TenToChuc,
                        Email = o.Email,
                        SoDienThoai = o.SoDienThoai,
                        DiaChi = o.DiaChi,
                        NgayTao = o.NgayTao,
                        GioiThieu = o.GioiThieu,
                        AnhDaiDien = o.AnhDaiDien,
                        TrangThaiXacMinh = o.TrangThaiXacMinh,
                        LyDoTuChoi = o.LyDoTuChoi,
                        DiemTrungBinh = o.DiemTrungBinh,
                        TaiKhoan = new TaiKhoan
                        {
                            MaTaiKhoan = o.TaiKhoan.MaTaiKhoan,
                            Email = o.TaiKhoan.Email,
                            VaiTro = o.TaiKhoan.VaiTro,
                            TrangThai = o.TaiKhoan.TrangThai,
                            NgayTao = o.TaiKhoan.NgayTao,
                            LanDangNhapCuoi = o.TaiKhoan.LanDangNhapCuoi
                        }
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách tổ chức: {ex.Message}");
                // Trả về danh sách rỗng nếu có lỗi
                return new List<ToChuc>();
            }
        }

        public async Task<bool> VerifyOrganizationAsync(int adminUserId, int id, string action, string lyDoTuChoi = "")
        {
            try
            {
                var org = await _context.Organization.FirstOrDefaultAsync(o => o.MaToChuc == id);
                if (org == null)
                    return false;

                var normalizedAction = (action ?? string.Empty).Trim().ToLowerInvariant();
                string message;

                switch (normalizedAction)
                {
                    case "approve":
                        org.TrangThaiXacMinh = 1;
                        org.LyDoTuChoi = null;
                        message = "Tổ chức của bạn đã được xác minh thành công.";
                        break;
                    case "reject":
                        org.TrangThaiXacMinh = 2;
                        org.LyDoTuChoi = lyDoTuChoi;
                        message = $"Yêu cầu xác minh bị từ chối. Lý do: {(!string.IsNullOrWhiteSpace(lyDoTuChoi) ? lyDoTuChoi : "Không có lý do cụ thể")}";
                        break;
                    case "revoke":
                        org.TrangThaiXacMinh = 3; // 3: Đã thu hồi
                        org.LyDoTuChoi = lyDoTuChoi;
                        message = $"Xác minh đã bị thu hồi. Lý do: {(!string.IsNullOrWhiteSpace(lyDoTuChoi) ? lyDoTuChoi : "Không có lý do cụ thể")}";
                        break;
                    default:
                        throw new Exception("Hành động xác minh không hợp lệ");
                }

                await _context.SaveChangesAsync();

                if (org.MaTaiKhoan > 0)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            MaNguoiTao = adminUserId,
                            PhanLoai = 1,
                            NoiDung = message,
                            MaNguoiNhans = new List<int> { org.MaTaiKhoan }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi gửi thông báo xác minh: {ex.Message}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xác minh tổ chức: {ex.Message}");
                return false;
            }
        }
    }
}
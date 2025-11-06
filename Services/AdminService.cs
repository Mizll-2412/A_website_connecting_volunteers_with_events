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

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        // Quản lý tài khoản
        public async Task<List<TaiKhoan>> GetAllUsersAsync()
        {
            return await _context.User.ToListAsync();
        }

        public async Task<TaiKhoan> GetUserByIdAsync(int id)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == id);
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

        public async Task<bool> VerifyOrganizationAsync(int id, bool isVerified, string lyDoTuChoi = "")
        {
            try
            {
                var org = await _context.Organization.FirstOrDefaultAsync(o => o.MaToChuc == id);
                if (org == null)
                    return false;

                org.TrangThaiXacMinh = isVerified ? (byte)1 : (byte)2; // 1: Đã xác minh, 2: Từ chối
                
                // Nếu từ chối, lưu lại lý do
                if (!isVerified && !string.IsNullOrEmpty(lyDoTuChoi))
                {
                    org.LyDoTuChoi = lyDoTuChoi;
                }
                
                await _context.SaveChangesAsync();
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
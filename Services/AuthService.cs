using System;
using System.Security.Cryptography;
using System.Text;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string GenerateJwtToken(int mataikhoan, string email, string vaiTro)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, mataikhoan.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, vaiTro ?? "User")
            };
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt: Issuer"],
                audience: _configuration["Jwt: Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateSalt()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        public string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }

        public async Task<LoginRespone> LoginAsync(LoginRequest request)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return new LoginRespone
                {
                    Success = false,
                    Message = "Email hoặc Mặt khẩu không chính xác"
                };
            }
            if (!user.TrangThai)
            {
                return new LoginRespone
                {
                    Success = false,
                    Message = "Tài khoản đã bị khóa"
                };
            }
            var hashedPassword = HashPassword(request.Password, user.PasswordSalt);
            if (hashedPassword != user.Password)
            {
                return new LoginRespone
                {
                    Success = false,
                    Message = "Email hoặc Mật khẩu không chính xác"
                };
            }
            // cap nhat dang nhap lan cuoi
            user.LanDangNhapCuoi = DateTime.Now;
            await _context.SaveChangesAsync();
            var token = GenerateJwtToken(user.MaTaiKhoan, user.Email, user.VaiTro);
            return new LoginRespone
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                UserInfo = new UserInfo
                {
                    MaTaiKhoan = user.MaTaiKhoan,
                    HoTen = user.HoTen,
                    Email = user.Email,
                    VaiTro = user.VaiTro
                }
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email đã được sử dụng"
                };
            }
            var salt = GenerateSalt();
            var hashedPassword = HashPassword(request.Password, salt);
            var newUser = new User
            {
                HoTen = request.HoTen,
                Email = request.Email,
                Password = request.Password,
                PasswordSalt = hashedPassword,
                VaiTro = request.VaiTro ?? "User",
                TrangThai = true,
                NgayTao = DateTime.Now
            };
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();
            return new RegisterResponse
            {
                Success = true,
                Message = "Đăng ký thành công",
                UserInfo = new UserInfo
                {
                    MaTaiKhoan = newUser.MaTaiKhoan,
                    HoTen = newUser.HoTen,
                    Email = newUser.Email,
                    VaiTro = newUser.VaiTro
                }
            };
            throw new NotImplementedException();
        }
    }
}
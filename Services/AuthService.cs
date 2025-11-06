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
using System.Net.Mail;
using System.Net;

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
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
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

            // Lấy thông tin HoTen từ các bảng con tùy theo vai trò
            string hoTen = null;
            if (user.VaiTro.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                var volunteer = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == user.MaTaiKhoan);
                if (volunteer != null)
                {
                    hoTen = volunteer.HoTen;
                }
            }
            else if (user.VaiTro.Equals("Organization", StringComparison.OrdinalIgnoreCase))
            {
                var org = await _context.Organization.FirstOrDefaultAsync(o => o.MaTaiKhoan == user.MaTaiKhoan);
                if (org != null)
                {
                    hoTen = org.TenToChuc;
                }
            }
            else if (user.VaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var admin = await _context.Admin.FirstOrDefaultAsync(a => a.MaTaiKhoan == user.MaTaiKhoan);
                if (admin != null)
                {
                    hoTen = admin.HoTen;
                }
            }

            return new LoginRespone
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                UserInfo = new UserInfo
                {
                    MaTaiKhoan = user.MaTaiKhoan,
                    HoTen = hoTen, // Thêm họ tên vào thông tin trả về
                    Email = user.Email,
                    VaiTro = user.VaiTro
                }
            };
        }

        private async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);
                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GenerateRandomToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData)
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .Replace("=", "")
                    .Substring(0, 20);
            }
        }

        public async Task<BaseResponse> LogoutAsync(int userId)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == userId);
                
                if (user == null)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Người dùng không tồn tại"
                    };
                }

                // Cập nhật thời gian đăng nhập cuối (không thực sự cần thiết cho đăng xuất, 
                // nhưng giữ lại để track user activity)
                user.LanDangNhapCuoi = DateTime.Now;
                await _context.SaveChangesAsync();

                return new BaseResponse
                {
                    Success = true,
                    Message = "Đăng xuất thành công"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
        
        public async Task<BaseResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                // Tìm người dùng theo email
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    // Không thông báo chi tiết để đảm bảo bảo mật
                    return new BaseResponse
                    {
                        Success = true,
                        Message = "Nếu email tồn tại, hệ thống sẽ gửi hướng dẫn khôi phục mật khẩu."
                    };
                }

                // Tạo token để đặt lại mật khẩu
                var token = GenerateRandomToken();
                var expiryDate = DateTime.Now.AddHours(24); // Token có hiệu lực 24 giờ

                // Lưu token vào database
                var resetToken = new TokenResetMatKhau
                {
                    MaTaiKhoan = user.MaTaiKhoan,
                    Token = token,
                    NgayHetHan = expiryDate,
                    DaSuDung = false
                };

                _context.TokenResetMatKhau.Add(resetToken);
                await _context.SaveChangesAsync();

                // Tạo URL để đặt lại mật khẩu
                var appUrl = _configuration["AppUrl"];
                var resetUrl = $"{appUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

                // Chuẩn bị nội dung email
                var subject = "Khôi phục mật khẩu - Hệ thống Tình nguyện viên";
                var body = $@"<html><body style='background:#f6f9fc;padding:24px'>
                    <div style='max-width:560px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #eaeaea'>
                      <div style='padding:24px 24px 8px 24px'>
                        <h2 style='margin:0;color:#111827;font-family:Arial'>Khôi phục mật khẩu</h2>
                        <p style='color:#4b5563;font-family:Arial'>Chúng tôi nhận được yêu cầu khôi phục mật khẩu cho tài khoản của bạn.</p>
                        <a href='{resetUrl}' style='display:inline-block;background:#10b981;color:#ffffff;text-decoration:none;padding:12px 16px;border-radius:8px;font-weight:600;font-family:Arial'>Đặt lại mật khẩu</a>
                        <p style='color:#6b7280;font-size:12px;margin-top:16px;font-family:Arial'>Liên kết có hiệu lực trong 24 giờ. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
                        <hr style='border:none;border-top:1px solid #e5e7eb;margin:16px 0'>
                        <p style='color:#9ca3af;font-size:12px;font-family:Arial'>Hệ thống Tình nguyện viên</p>
                      </div>
                    </div>
                  </body></html>";

                // Gửi email
                var emailSent = await SendEmail(user.Email, subject, body);
                if (!emailSent)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Không thể gửi email. Vui lòng thử lại sau."
                    };
                }

                return new BaseResponse
                {
                    Success = true,
                    Message = "Hướng dẫn khôi phục mật khẩu đã được gửi đến email của bạn."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                // Tìm người dùng theo email
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng."
                    };
                }

                // Tìm token reset mật khẩu
                var resetToken = await _context.TokenResetMatKhau
                    .FirstOrDefaultAsync(t => 
                        t.MaTaiKhoan == user.MaTaiKhoan && 
                        t.Token == request.Token && 
                        !t.DaSuDung && 
                        t.NgayHetHan > DateTime.Now);

                if (resetToken == null)
                {
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc đã hết hạn."
                    };
                }

                // Đặt lại mật khẩu
                var salt = GenerateSalt();
                var hashedPassword = HashPassword(request.NewPassword, salt);

                user.Password = hashedPassword;
                user.PasswordSalt = salt;

                // Đánh dấu token đã được sử dụng
                resetToken.DaSuDung = true;

                await _context.SaveChangesAsync();

                return new BaseResponse
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu thành công."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> RequestChangeEmailAsync(int userId, ChangeEmailRequest request)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == userId);
                if (user == null)
                {
                    return new BaseResponse { Success = false, Message = "Người dùng không tồn tại" };
                }

                // Kiểm tra trùng email
                var dup = await _context.User.AnyAsync(u => u.Email == request.NewEmail);
                if (dup)
                {
                    return new BaseResponse { Success = false, Message = "Email đã tồn tại trong hệ thống" };
                }

                var token = GenerateRandomToken();
                var expiryDate = DateTime.Now.AddHours(24);

                var changeToken = new TokenDoiEmail
                {
                    MaTaiKhoan = user.MaTaiKhoan,
                    EmailMoi = request.NewEmail,
                    Token = token,
                    NgayHetHan = expiryDate,
                    DaSuDung = false
                };

                _context.TokenDoiEmail.Add(changeToken);
                await _context.SaveChangesAsync();

                var appUrl = _configuration["AppUrl"];
                var confirmUrl = $"{appUrl}/confirm-change-email?token={Uri.EscapeDataString(token)}";

                var subject = "Xác nhận đổi email - Hệ thống Tình nguyện viên";
                var body = $@"<html><body style='background:#f6f9fc;padding:24px'>
                    <div style='max-width:560px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #eaeaea'>
                      <div style='padding:24px 24px 8px 24px'>
                        <h2 style='margin:0;color:#111827;font-family:Arial'>Xác nhận đổi email</h2>
                        <p style='color:#4b5563;font-family:Arial'>Bạn yêu cầu đổi email đăng nhập sang: <b>{System.Net.WebUtility.HtmlEncode(request.NewEmail)}</b></p>
                        <a href='{confirmUrl}' style='display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;padding:12px 16px;border-radius:8px;font-weight:600;font-family:Arial'>Xác nhận đổi email</a>
                        <p style='color:#6b7280;font-size:12px;margin-top:16px;font-family:Arial'>Liên kết có hiệu lực trong 24 giờ.</p>
                        <hr style='border:none;border-top:1px solid #e5e7eb;margin:16px 0'>
                        <p style='color:#9ca3af;font-size:12px;font-family:Arial'>Hệ thống Tình nguyện viên</p>
                      </div>
                    </div>
                  </body></html>";

                var emailSent = await SendEmail(request.NewEmail, subject, body);
                if (!emailSent)
                {
                    return new BaseResponse { Success = false, Message = "Không thể gửi email xác nhận. Vui lòng thử lại sau." };
                }

                return new BaseResponse { Success = true, Message = "Đã gửi email xác nhận tới địa chỉ mới." };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<BaseResponse> ConfirmChangeEmailAsync(ConfirmChangeEmailRequest request)
        {
            try
            {
                var tokenModel = await _context.TokenDoiEmail
                    .FirstOrDefaultAsync(t => t.Token == request.Token && !t.DaSuDung && t.NgayHetHan > DateTime.Now);

                if (tokenModel == null)
                {
                    return new BaseResponse { Success = false, Message = "Token không hợp lệ hoặc đã hết hạn." };
                }

                var user = await _context.User.FirstOrDefaultAsync(u => u.MaTaiKhoan == tokenModel.MaTaiKhoan);
                if (user == null)
                {
                    return new BaseResponse { Success = false, Message = "Người dùng không tồn tại" };
                }

                // Kiểm tra trùng email một lần nữa trước khi cập nhật
                var dup = await _context.User.AnyAsync(u => u.Email == tokenModel.EmailMoi && u.MaTaiKhoan != user.MaTaiKhoan);
                if (dup)
                {
                    return new BaseResponse { Success = false, Message = "Email đã tồn tại trong hệ thống" };
                }

                // Cập nhật email ở cả bảng tài khoản và bảng con nếu có
                user.Email = tokenModel.EmailMoi;

                var volunteer = await _context.Volunteer.FirstOrDefaultAsync(v => v.MaTaiKhoan == user.MaTaiKhoan);
                if (volunteer != null)
                {
                    volunteer.Email = tokenModel.EmailMoi;
                }
                var org = await _context.Organization.FirstOrDefaultAsync(o => o.MaTaiKhoan == user.MaTaiKhoan);
                if (org != null)
                {
                    org.Email = tokenModel.EmailMoi;
                }
                var admin = await _context.Admin.FirstOrDefaultAsync(a => a.MaTaiKhoan == user.MaTaiKhoan);
                if (admin != null)
                {
                    admin.Email = tokenModel.EmailMoi;
                }

                tokenModel.DaSuDung = true;
                await _context.SaveChangesAsync();

                return new BaseResponse { Success = true, Message = "Đổi email thành công." };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                var newUser = new TaiKhoan
                {
                    // Trường HoTen đã được loại bỏ khỏi mô hình, sẽ lưu vào bảng riêng (TinhNguyenVien/ToChuc)
                    Email = request.Email,
                    Password = hashedPassword,
                    PasswordSalt = salt,
                    VaiTro = request.VaiTro ?? "User",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };

                _context.User.Add(newUser);
                await _context.SaveChangesAsync();
                int? maTNV = null;
                int? maToChuc = null;
                int? maAdmin = null;
                if (newUser.VaiTro.Equals("User", StringComparison.OrdinalIgnoreCase))
                {
                    var volunteer = new TinhNguyenVien
                    {
                        MaTaiKhoan = newUser.MaTaiKhoan,
                        HoTen = request.HoTen, // Lưu họ tên từ request vào bảng TinhNguyenVien
                        Email = newUser.Email
                    };
                    _context.Volunteer.Add(volunteer);
                    await _context.SaveChangesAsync();
                    maTNV = volunteer.MaTNV;

                }
                else if (newUser.VaiTro.Equals("Organization", StringComparison.OrdinalIgnoreCase))
                {
                    var org = new ToChuc
                    {
                        MaTaiKhoan = newUser.MaTaiKhoan,
                        TenToChuc = request.HoTen, // Lưu tên tổ chức từ request
                        Email = newUser.Email
                    };
                    _context.Organization.Add(org);
                    await _context.SaveChangesAsync();
                    maToChuc = org.MaToChuc;
                }
                else if (newUser.VaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var ad = new Admin
                    {
                        MaTaiKhoan = newUser.MaTaiKhoan,
                        HoTen = request.HoTen, // Lưu họ tên từ request
                        Email = newUser.Email
                    };
                    _context.Admin.Add(ad);
                    await _context.SaveChangesAsync();
                    maAdmin = ad.MaAdmin;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new RegisterResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    UserInfo = new UserInfo
                    {
                        MaTaiKhoan = newUser.MaTaiKhoan,
                        HoTen = request.HoTen, // Thêm họ tên vào response
                        Email = newUser.Email,
                        VaiTro = newUser.VaiTro
                    },
                    MaTNV = maTNV,
                    MaToChuc = maToChuc,
                    MaAdmin = maAdmin
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
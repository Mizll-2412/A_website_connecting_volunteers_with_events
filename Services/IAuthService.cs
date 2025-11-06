using khoaluantotnghiep.DTOs;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginRespone> LoginAsync(LoginRequest request);
        Task<BaseResponse> LogoutAsync(int userId);
        Task<BaseResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<BaseResponse> ResetPasswordAsync(ResetPasswordRequest request);
        Task<BaseResponse> RequestChangeEmailAsync(int userId, ChangeEmailRequest request);
        Task<BaseResponse> ConfirmChangeEmailAsync(ConfirmChangeEmailRequest request);
        string HashPassword(string password, string salt);
        string GenerateSalt();
        string GenerateJwtToken(int mataikhoan, string email, string vaiTro);
    }
}
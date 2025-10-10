using khoaluantotnghiep.DTOs;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginRespone> LoginAsync(LoginRequest request);
        string HashPassword(string password, string salt);
        string GenerateSalt();
        string GenerateJwtToken(int mataikhoan, string email, string vaiTro);
    }
}
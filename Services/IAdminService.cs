using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Services
{
    public interface IAdminService
    {
        // Quản lý tài khoản
        Task<List<AdminUserDto>> GetAllUsersAsync();
        Task<AdminUserDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserRoleAsync(int id, string role);
        Task<bool> UpdateUserStatusAsync(int id, bool status);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AdminResetPasswordAsync(int userId, string newPassword);

        // Quản lý tổ chức
        Task<List<ToChuc>> GetAllOrganizationsAsync();
        Task<List<ToChuc>> GetPendingOrganizationsAsync();
        Task<bool> VerifyOrganizationAsync(int adminUserId, int id, string action, string lyDoTuChoi = "");
    }
}

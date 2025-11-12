using Microsoft.AspNetCore.Mvc;
using khoaluantotnghiep.Services;
using khoaluantotnghiep.DTOs;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // API quản lý tài khoản
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "Không tìm thấy tài khoản" });

            return Ok(user);
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.UpdateUserRoleAsync(id, request.VaiTro);
            if (!result)
                return NotFound(new { Message = "Không tìm thấy tài khoản hoặc vai trò không hợp lệ" });

            return Ok(new { Message = "Cập nhật vai trò thành công" });
        }

        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.UpdateUserStatusAsync(id, request.TrangThai);
            if (!result)
                return NotFound(new { Message = "Không tìm thấy tài khoản" });

            return Ok(new { Message = "Cập nhật trạng thái thành công" });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _adminService.DeleteUserAsync(id);
            if (!result)
                return NotFound(new { Message = "Không tìm thấy tài khoản" });

            return Ok(new { Message = "Xóa tài khoản thành công" });
        }

        [HttpPost("users/{id}/reset-password")]
        public async Task<IActionResult> AdminResetPassword(int id, [FromBody] AdminResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.AdminResetPasswordAsync(id, request.NewPassword);
            if (!result)
                return NotFound(new { Message = "Không tìm thấy tài khoản" });

            return Ok(new { Message = "Đặt lại mật khẩu thành công" });
        }

        // API quản lý tổ chức
        [HttpGet("organizations")]
        public async Task<IActionResult> GetAllOrganizations()
        {
            var organizations = await _adminService.GetAllOrganizationsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách tổ chức thành công",
                data = organizations
            });
        }

        [HttpGet("organizations/pending")]
        public async Task<IActionResult> GetPendingOrganizations()
        {
            var organizations = await _adminService.GetPendingOrganizationsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách tổ chức chờ xác minh thành công",
                data = organizations
            });
        }

        [HttpPut("organizations/{id}/verify")]
        public async Task<IActionResult> VerifyOrganization(int id, [FromBody] VerifyOrganizationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var action = request.HanhDong?.Trim().ToLowerInvariant() ?? (request.DaXacMinh ? "approve" : "reject");

            var result = await _adminService.VerifyOrganizationAsync(adminUserId, id, action, request.LyDoTuChoi);
            if (!result)
                return NotFound(new { Message = "Không tìm thấy tổ chức" });

            string message = action switch
            {
                "approve" => "Xác minh tổ chức thành công",
                "revoke" => "Thu hồi xác minh tổ chức thành công",
                _ => "Từ chối xác minh tổ chức thành công"
            };

            return Ok(new { Message = message });
        }
    }
}

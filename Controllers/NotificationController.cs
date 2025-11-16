using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService service, ILogger<NotificationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // Lấy tất cả thông báo của người dùng hiện tại
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool? read = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                
                var notifications = await _service.GetNotificationsAsync(userId, read);
                return Ok(new { data = notifications });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thông báo: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Lấy số lượng thông báo
        [HttpGet("count")]
        public async Task<IActionResult> GetNotificationCount()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                
                var count = await _service.GetNotificationCountAsync(userId);
                return Ok(new { data = count });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy số lượng thông báo: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Đánh dấu đã đọc một thông báo
        [HttpPut("status")]
        public async Task<IActionResult> UpdateNotificationStatus([FromBody] UpdateNotificationStatusDto updateDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                
                var result = await _service.UpdateNotificationStatusAsync(userId, updateDto);
                return Ok(new { message = "Cập nhật trạng thái thông báo thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật trạng thái thông báo: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Đánh dấu tất cả thông báo là đã đọc
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                
                await _service.MarkAllAsReadAsync(userId);
                return Ok(new { message = "Đánh dấu tất cả thông báo là đã đọc thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đánh dấu tất cả thông báo: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Xóa một thông báo
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                
                await _service.DeleteNotificationAsync(userId, notificationId);
                return Ok(new { message = "Xóa thông báo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa thông báo: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Xóa tất cả thông báo
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                
                await _service.DeleteAllNotificationsAsync(userId);
                return Ok(new { message = "Xóa tất cả thông báo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa tất cả thông báo: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
        
        // API cho Admin tạo thông báo
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                
                // Set người tạo là Admin hiện tại
                createDto.MaNguoiTao = userId;
                
                var result = await _service.CreateNotificationAsync(createDto);
                return Ok(new { message = "Tạo thông báo thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo thông báo: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // API mời TNV tham gia sự kiện (dành cho Tổ chức)
        [HttpPost("invite-event")]
        [Authorize(Roles = "Organization")]
        public async Task<IActionResult> InviteVolunteerToEvent([FromBody] InviteEventDto inviteDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });

                var result = await _service.InviteVolunteerToEventAsync(userId, inviteDto);
                return Ok(new { message = "Gửi lời mời thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi gửi lời mời: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // API TNV yêu cầu tổ chức đánh giá (dành cho User/TNV)
        [HttpPost("request-evaluation")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RequestEvaluationFromOrganization([FromBody] RequestEvaluationDto requestDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });

                // Tạo notification yêu cầu đánh giá
                var createDto = new CreateNotificationDto
                {
                    MaNguoiTao = userId,
                    PhanLoai = 3, // Thông báo đánh giá
                    NoiDung = requestDto.NoiDung,
                    MaNguoiNhans = new List<int> { requestDto.MaTaiKhoanToChuc }
                };

                var result = await _service.CreateNotificationAsync(createDto);
                return Ok(new { message = "Đã gửi yêu cầu đánh giá thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi gửi yêu cầu đánh giá: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

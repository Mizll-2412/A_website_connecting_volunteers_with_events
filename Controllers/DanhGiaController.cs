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
    public class DanhGiaController : ControllerBase
    {
        private readonly IDanhGiaService _service;
        private readonly ILogger<DanhGiaController> _logger;

        public DanhGiaController(IDanhGiaService service, ILogger<DanhGiaController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// Tạo đánh giá mới
        /// User đánh giá Organization, Organization đánh giá User
        [HttpPost]
        [Authorize(Roles = "User,Organization")]
        public async Task<IActionResult> TaoMoiDanhGia([FromBody] CreateDanhGiaDto createDto)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại từ JWT token
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (currentUserId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });

                // Set người đánh giá là current user
                createDto.MaNguoiDanhGia = currentUserId;

                var result = await _service.TaoMoiDanhGiaAsync(createDto);
                return Ok(new 
                { 
                    success = true,
                    message = "Đánh giá thành công", 
                    data = result 
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo đánh giá: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi tạo đánh giá" });
            }
        }

        /// Cập nhật đánh giá
        /// Chỉ người tạo hoặc Admin mới được sửa
        [HttpPut("{maDanhGia}")]
        [Authorize(Roles = "User,Organization,Admin")]
        public async Task<IActionResult> CapNhatDanhGia(int maDanhGia, [FromBody] UpdateDanhGiaDto updateDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                if (currentUserId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });

                var result = await _service.CapNhatDanhGiaAsync(maDanhGia, updateDto, currentUserId, currentUserRole);
                return Ok(new 
                { 
                    success = true,
                    message = "Cập nhật đánh giá thành công", 
                    data = result 
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật đánh giá: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi cập nhật đánh giá" });
            }
        }

        /// Lấy chi tiết một đánh giá
        /// Tất cả user đã đăng nhập đều xem được
        [HttpGet("{maDanhGia}")]
        [Authorize(Roles = "User,Organization,Admin")]
        public async Task<IActionResult> GetDanhGia(int maDanhGia)
        {
            try
            {
                var result = await _service.GetDanhGiaAsync(maDanhGia);
                return Ok(new 
                { 
                    success = true,
                    data = result 
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy đánh giá: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// Lấy danh sách đánh giá của một người dùng
        /// Tất cả user đã đăng nhập đều xem được
        [HttpGet("user/{mauser}")]
        [Authorize(Roles = "User,Organization,Admin")]
        public async Task<IActionResult> GetDanhGiaCuaNguoi(int mauser)
        {
            try
            {
                var result = await _service.GetDanhGiaCuaNguoiAsync(mauser);
                return Ok(new 
                { 
                    success = true,
                    data = result,
                    count = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách đánh giá: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// Lấy thống kê đánh giá của một người dùng
        /// User/Organization: Chỉ xem thống kê của chính mình
        /// Admin: Xem tất cả
        [HttpGet("thongke/{mauser}")]
        [Authorize(Roles = "User,Organization,Admin")]
        public async Task<IActionResult> GetThongKe(int mauser)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                // Kiểm tra quyền: User/Org chỉ xem thống kê của mình, Admin xem tất cả
                if (currentUserRole != "Admin" && currentUserId != mauser)
                {
                    return StatusCode(403, new 
                    { 
                        success = false,
                        message = "Bạn chỉ có thể xem thống kê của chính mình" 
                    });
                }

                var result = await _service.GetThongKeDanhGiaAsync(mauser);
                return Ok(new 
                { 
                    success = true,
                    data = result 
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// Xóa đánh giá
        /// Chỉ người tạo hoặc Admin mới được xóa
        [HttpDelete("{maDanhGia}")]
        [Authorize(Roles = "User,Organization,Admin")]
        public async Task<IActionResult> XoaDanhGia(int maDanhGia)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                if (currentUserId == 0)
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });

                await _service.XoaDanhGiaAsync(maDanhGia, currentUserId, currentUserRole);
                return Ok(new 
                { 
                    success = true,
                    message = "Xóa đánh giá thành công" 
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa đánh giá: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xóa đánh giá" });
            }
        }
    }
}
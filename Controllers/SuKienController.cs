using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;
using System.Security.Claims;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuKienController : ControllerBase
    {
        private readonly IEventService _service;
        private readonly ILogger<SuKienController> _logger;
        private readonly INotificationService _notificationService;
        private readonly Data.AppDbContext _context;
        private readonly IConfiguration _configuration;

        public SuKienController(IEventService service, ILogger<SuKienController> logger, INotificationService notificationService, Data.AppDbContext context, IConfiguration configuration)
        {
            _service = service;
            _logger = logger;
            _notificationService = notificationService;
            _context = context;
            _configuration = configuration;
        }

        // Kết thúc sự kiện (đổi trạng thái)
        [HttpPost("{maSuKien}/finish")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> FinishEvent(int maSuKien)
        {
            try
            {
                var sk = await _context.Event.FindAsync(maSuKien);
                if (sk == null) return NotFound(new { message = "Sự kiện không tồn tại" });
                // Chỉ tổ chức sở hữu sự kiện hoặc admin mới được kết thúc
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
                if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    var org = await _context.Organization.FindAsync(sk.MaToChuc);
                    if (org == null || org.MaTaiKhoan != userId)
                    {
                        return Forbid();
                    }
                }

                sk.TrangThai = "Đã kết thúc";
                await _context.SaveChangesAsync();
                return Ok(new { message = "Sự kiện đã kết thúc" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi kết thúc sự kiện: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> CreateSuKien([FromForm] CreateSuKienDto createDto, IFormFile? anhFile)
        {
            try
            {
                if (anhFile != null)
                {
                    var imagePath = await _service.UploadAnh(anhFile);
                    createDto.HinhAnh = imagePath;
                }
                var result = await _service.CreateSuKienAsync(createDto);
                return CreatedAtAction(nameof(GetSuKien), new { maSuKien = result.MaSuKien }, 
                    new { message = "Tạo sự kiện thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Mời tình nguyện viên tham gia sự kiện: gửi thông báo tới TNV (và có thể đăng ký sau)
        [HttpPost("{maSuKien}/invite/{maTNV}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> InviteVolunteer(int maSuKien, int maTNV)
        {
            try
            {
                // Kiểm tra sự kiện và tình nguyện viên tồn tại
                var suKien = await _context.Event.FindAsync(maSuKien);
                if (suKien == null) return NotFound(new { message = "Sự kiện không tồn tại" });

                var tnv = await _context.Volunteer.FindAsync(maTNV);
                if (tnv == null) return NotFound(new { message = "Tình nguyện viên không tồn tại" });

                // Lấy user hiện tại làm người tạo thông báo
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized(new { message = "Không thể xác thực người dùng" });

                // Xây dựng nội dung thông báo
                var appUrl = _configuration["AppUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
                var eventLink = $"{appUrl}/su-kien/{maSuKien}";

                var createDto = new DTOs.CreateNotificationDto
                {
                    MaNguoiTao = userId,
                    PhanLoai = 2, // Thông báo sự kiện
                    NoiDung = $"Bạn được mời tham gia sự kiện: {suKien.TenSuKien}. Xem chi tiết và đăng ký tại: {eventLink}",
                    MaNguoiNhans = new List<int> { tnv.MaTaiKhoan }
                };

                await _notificationService.CreateNotificationAsync(createDto);
                return Ok(new { message = "Đã gửi lời mời tới tình nguyện viên." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi mời TNV: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("{maSuKien}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSuKien(int maSuKien)
        {
            try
            {
                var result = await _service.GetSuKienAsync(maSuKien);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSuKien()
        {
            try
            {
                var result = await _service.GetAllSuKienAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Lấy danh sách sự kiện theo mã tổ chức
        [HttpGet("organization/{maToChuc}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSuKiensByToChuc(int maToChuc)
        {
            try
            {
                var result = await _service.GetSuKiensByToChucAsync(maToChuc);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy danh sách sự kiện theo tổ chức: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{maSuKien}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> UpdateSuKien(int maSuKien, [FromForm] UpdateSuKienDto updateDto, IFormFile? anhFile)
        {
            try
            {
                if (anhFile != null)
                {
                    var imagePath = await _service.UploadAnhAsync(maSuKien, anhFile);
                    updateDto.HinhAnh = imagePath;
                }
                var result = await _service.UpdateSuKienAsync(maSuKien, updateDto);
                return Ok(new { message = "Cập nhật thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{maSuKien}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> DeleteSuKien(int maSuKien)
        {
            try
            {
                await _service.DeleteSuKienAsync(maSuKien);
                return Ok(new { message = "Xóa sự kiện thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

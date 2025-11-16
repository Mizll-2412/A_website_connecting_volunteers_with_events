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
                sk.NgayKetThuc = DateTime.Now; // Cập nhật ngày kết thúc
                await _context.SaveChangesAsync();
                
                // Gửi thông báo cho TNV đã đăng ký
                try
                {
                    await _notificationService.SendEventFinishedNotificationAsync(maSuKien);
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning($"Không thể gửi thông báo kết thúc sự kiện: {notifEx.Message}");
                }
                
                return Ok(new { message = "Sự kiện đã kết thúc" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi kết thúc sự kiện: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Đóng phiên tuyển dụng sớm
        [HttpPost("{maSuKien}/close-recruitment")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> CloseRecruitment(int maSuKien)
        {
            try
            {
                var sk = await _context.Event.FindAsync(maSuKien);
                if (sk == null) return NotFound(new { message = "Sự kiện không tồn tại" });
                
                // Chỉ tổ chức sở hữu sự kiện hoặc admin mới được đóng phiên tuyển
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

                // Kiểm tra nếu sự kiện đã kết thúc thì không cho đóng phiên tuyển
                if (sk.TrangThai == "Đã kết thúc")
                {
                    return BadRequest(new { message = "Không thể đóng phiên tuyển của sự kiện đã kết thúc" });
                }

                // Đóng phiên tuyển
                sk.TrangThaiTuyen = "Đóng";
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Phiên tuyển của sự kiện {maSuKien} đã được đóng");
                
                return Ok(new { message = "Đã đóng phiên tuyển dụng" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đóng phiên tuyển: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Mở lại phiên tuyển dụng
        [HttpPost("{maSuKien}/open-recruitment")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> OpenRecruitment(int maSuKien)
        {
            try
            {
                var sk = await _context.Event.FindAsync(maSuKien);
                if (sk == null) return NotFound(new { message = "Sự kiện không tồn tại" });
                
                // Chỉ tổ chức sở hữu sự kiện hoặc admin mới được mở lại phiên tuyển
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

                // Kiểm tra nếu sự kiện đã kết thúc thì không cho mở lại phiên tuyển
                if (sk.TrangThai == "Đã kết thúc")
                {
                    return BadRequest(new { message = "Không thể mở lại phiên tuyển của sự kiện đã kết thúc" });
                }

                // Kiểm tra nếu phiên tuyển chưa đóng thì không cần mở lại
                if (string.IsNullOrEmpty(sk.TrangThaiTuyen) || sk.TrangThaiTuyen != "Đóng")
                {
                    return BadRequest(new { message = "Phiên tuyển chưa được đóng" });
                }

                // Kiểm tra xem có còn trong thời gian tuyển không
                var now = DateTime.Now;
                if (!sk.TuyenBatDau.HasValue || !sk.TuyenKetThuc.HasValue)
                {
                    return BadRequest(new { message = "Sự kiện không có thời gian tuyển dụng" });
                }

                if (now < sk.TuyenBatDau.Value || now > sk.TuyenKetThuc.Value)
                {
                    return BadRequest(new { message = "Không thể mở lại phiên tuyển vì đã ngoài thời gian tuyển dụng" });
                }

                // Mở lại phiên tuyển bằng cách set null, backend sẽ tự động tính toán lại trạng thái
                sk.TrangThaiTuyen = null;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Phiên tuyển của sự kiện {maSuKien} đã được mở lại");
                
                return Ok(new { message = "Đã mở lại phiên tuyển dụng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi mở lại phiên tuyển: {ex.Message}");
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
                
                // Gửi thông báo cho Admin
                try
                {
                    await _notificationService.SendEventCreatedNotificationAsync(result.MaSuKien);
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning($"Không thể gửi thông báo tạo sự kiện: {notifEx.Message}");
                }
                
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
                // Lấy tên sự kiện trước khi xóa
                var suKien = await _context.Event.FindAsync(maSuKien);
                string eventName = suKien?.TenSuKien ?? "Sự kiện";
                
                // Gửi thông báo cho TNV đã đăng ký trước khi xóa
                try
                {
                    await _notificationService.SendEventDeletedNotificationAsync(maSuKien, eventName);
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning($"Không thể gửi thông báo xóa sự kiện: {notifEx.Message}");
                }
                
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

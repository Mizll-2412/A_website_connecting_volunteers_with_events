using System;
using System.Collections.Generic;
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
    public class DonDangKyController : ControllerBase
    {
        private readonly IRegistrationFormService _service;
        private readonly ILogger<DonDangKyController> _logger;

        public DonDangKyController(IRegistrationFormService service, ILogger<DonDangKyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// Tình nguyện viên đăng ký sự kiện
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DangKySuKien([FromBody] CreateDonDangKyDto createDto)
        {
            try
            {
                // Kiểm tra và xử lý dữ liệu đầu vào
                if (createDto == null || createDto.MaTNV <= 0 || createDto.MaSuKien <= 0)
                {
                    return BadRequest(new { message = "Dữ liệu đăng ký không hợp lệ" });
                }
                
                // Gọi service để đăng ký
                var result = await _service.DangKySuKienAsync(createDto);
                return Ok(new { message = "Đăng ký thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đăng ký sự kiện: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Lấy đơn đăng ký cụ thể
        [HttpGet("{maTNV}/{maSuKien}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetDonDangKy(int maTNV, int maSuKien)
        {
            try
            {
                var result = await _service.GetDonDangKyAsync(maTNV, maSuKien);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
        }

        /// Lấy danh sách đơn đăng ký của tình nguyện viên
        [HttpGet("volunteer/{maTNV}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetDonDangKyByTNV(int maTNV)
        {
            try
            {
                var result = await _service.GetDonDangKyByTNVAsync(maTNV);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Lấy danh sách đơn đăng ký của sự kiện (cho tổ chức)
        [HttpGet("event/{maSuKien}")]
        [Authorize(Roles = "Admin,Organization")]
        public async Task<IActionResult> GetDonDangKyBySuKien(int maSuKien)
        {
            try
            {
                var result = await _service.GetDonDangKyBySuKienAsync(maSuKien);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Tổ chức duyệt/từ chối đơn đăng ký
        [HttpPut("{maTNV}/{maSuKien}")]
        [Authorize(Roles = "Admin,Organization")]
        public async Task<IActionResult> UpdateTrangThai(int maTNV, int maSuKien, [FromBody] UpdateDonDangKyDto updateDto)
        {
            try
            {
                var result = await _service.UpdateTrangThaiAsync(maTNV, maSuKien, updateDto);
                return Ok(new { message = "Cập nhật thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Hủy đăng ký sự kiện
        [HttpDelete("{maTNV}/{maSuKien}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> HuyDangKy(int maTNV, int maSuKien)
        {
            try
            {
                await _service.HuyDangKyAsync(maTNV, maSuKien);
                return Ok(new { message = "Hủy đăng ký thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Lịch sử tham gia sự kiện của tình nguyện viên
        [HttpGet("history/{maTNV}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetEventHistory(
            int maTNV, 
            [FromQuery] int? nam, 
            [FromQuery] int? thang, 
            [FromQuery] bool? hoanThanh, 
            [FromQuery] bool? coGiayChungNhan)
        {
            try
            {
                var filter = new EventHistoryFilterDto
                {
                    Nam = nam,
                    Thang = thang,
                    HoanThanh = hoanThanh,
                    CoGiayChungNhan = coGiayChungNhan
                };

                var result = await _service.GetEventHistoryAsync(maTNV, filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Thống kê tham gia sự kiện của tình nguyện viên
        [HttpGet("history/{maTNV}/stats")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetEventHistoryStats(int maTNV)
        {
            try
            {
                var result = await _service.GetEventHistoryStatsAsync(maTNV);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

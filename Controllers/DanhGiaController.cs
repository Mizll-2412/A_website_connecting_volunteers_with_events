using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DanhGiaController : ControllerBase
    {
        private readonly IDanhGiaService _service;
        private readonly ILogger<DanhGiaController> _logger;

        public DanhGiaController(IDanhGiaService service, ILogger<DanhGiaController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> TaoMoiDanhGia([FromBody] CreateDanhGiaDto createDto)
        {
            try
            {
                var result = await _service.TaoMoiDanhGiaAsync(createDto);
                return Ok(new { message = "Đánh giá thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{maDanhGia}")]
        public async Task<IActionResult> CapNhatDanhGia(int maDanhGia, [FromBody] UpdateDanhGiaDto updateDto)
        {
            try
            {
                var result = await _service.CapNhatDanhGiaAsync(maDanhGia, updateDto);
                return Ok(new { message = "Cập nhật thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{maDanhGia}")]
        public async Task<IActionResult> GetDanhGia(int maDanhGia)
        {
            try
            {
                var result = await _service.GetDanhGiaAsync(maDanhGia);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("user/{mauser}")]
        public async Task<IActionResult> GetDanhGiaCuaNguoi(int mauser)
        {
            try
            {
                var result = await _service.GetDanhGiaCuaNguoiAsync(mauser);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("thongke/{mauser}")]
        public async Task<IActionResult> GetThongKe(int mauser)
        {
            try
            {
                var result = await _service.GetThongKeDanhGiaAsync(mauser);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{maDanhGia}")]
        public async Task<IActionResult> XoaDanhGia(int maDanhGia)
        {
            try
            {
                await _service.XoaDanhGiaAsync(maDanhGia);
                return Ok(new { message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

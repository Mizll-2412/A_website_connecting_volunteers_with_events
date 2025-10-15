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
    public class GiayToPhapLyController : ControllerBase
    {
        private readonly ILegalDocumentService _service;
        private readonly ILogger<GiayToPhapLyController> _logger;

        public GiayToPhapLyController(ILegalDocumentService service, ILogger<GiayToPhapLyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// Tổ chức upload giấy tờ pháp lý
        [HttpPost("upload")]
        public async Task<IActionResult> UploadGiayTo([FromForm] UploadDocument uploadDto)
        {
            try
            {
                var result = await _service.UploadGiayToAsync(uploadDto);
                return Ok(new { message = "Upload giấy tờ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Lấy danh sách giấy tờ của tổ chức
        [HttpGet("tochuc/{maToChuc}")]
        public async Task<IActionResult> GetGiayToByToChuc(int maToChuc)
        {
            try
            {
                var result = await _service.GetGiayToByToChucAsync(maToChuc);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Xóa giấy tờ
        [HttpDelete("{maGiayTo}")]
        public async Task<IActionResult> DeleteGiayTo(int maGiayTo)
        {
            try
            {
                await _service.DeleteGiayToAsync(maGiayTo);
                return Ok(new { message = "Xóa giấy tờ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Admin: Lấy danh sách tổ chức chờ xác minh
        [HttpGet("cho-xac-minh")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDanhSachChoXacMinh()
        {
            try
            {
                var result = await _service.GetDanhSachChoXacMinhAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Admin: Xác minh/từ chối tổ chức
        [HttpPut("xac-minh/{maToChuc}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> XacMinhToChuc(int maToChuc, [FromBody] XacMinhToChucDto xacMinhDto)
        {
            try
            {
                var result = await _service.XacMinhToChucAsync(maToChuc, xacMinhDto);
                return Ok(new { message = "Xác minh thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Lấy thông tin xác minh của tổ chức
        [HttpGet("thong-tin/{maToChuc}")]
        public async Task<IActionResult> GetThongTinXacMinh(int maToChuc)
        {
            try
            {
                var result = await _service.GetThongTinXacMinhAsync(maToChuc);
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
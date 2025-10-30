using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuKienController : ControllerBase
    {
        private readonly IEventService _service;
        private readonly ILogger<SuKienController> _logger;

        public SuKienController(IEventService service, ILogger<SuKienController> logger)
        {
            _service = service;
            _logger = logger;
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

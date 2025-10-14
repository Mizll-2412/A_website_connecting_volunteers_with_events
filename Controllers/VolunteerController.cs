using System;
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
    public class TinhNguyenVienController : ControllerBase
    {
        private readonly ITinhNguyenVienService _service;
        private readonly ILogger<TinhNguyenVienController> _logger;

        public TinhNguyenVienController(ITinhNguyenVienService service, ILogger<TinhNguyenVienController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{maTNV}")]
        public async Task<IActionResult> GetTinhNguyenVien(int maTNV)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.GetTinhNguyenVienAsync(maTNV);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{maTNV}")]
        public async Task<IActionResult> UpdateTinhNguyenVien(int maTNV, [FromForm] UpdateTinhNguyenVienDto updateDto,IFormFile anhFile )
        {

            try
            {
                string? imagePath = null;
                if (anhFile != null)
                {
                    imagePath = await _service.UploadAnhDaiDienAsync(maTNV, anhFile);
                    updateDto.AnhDaiDien = imagePath;

                }
                var result = await _service.UpdateTinhNguyenVienAsync(maTNV, updateDto);
                return Ok(new { message = "Cập nhật thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            
        }
        [HttpPost("{maTNV}/upload-avatar")]
        public async Task<IActionResult> UploadAvatar(int maTNV, [FromForm] IFormFile anhFile)
        {
            try
            {
                var imagePath = await _service.UploadAnhDaiDienAsync(maTNV, anhFile);
                return Ok(new
                {
                    message = "Upload ảnh thành công",
                    imagePath = imagePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
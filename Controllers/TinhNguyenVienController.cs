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
    public class TinhNguyenVienController : ControllerBase
    {
        private readonly ITinhNguyenVienService _service;
        private readonly ILogger<TinhNguyenVienController> _logger;

        public TinhNguyenVienController(ITinhNguyenVienService service, ILogger<TinhNguyenVienController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// Tạo mới tình nguyện viên
        [HttpPost]
        public async Task<IActionResult> CreateTinhNguyenVien([FromBody] CreateTinhNguyenVienDto createDto)
        {
            try
            {
                var result = await _service.CreateTinhNguyenVienAsync(createDto);
                return CreatedAtAction(nameof(GetTinhNguyenVien), new { maTNV = result.MaTNV }, 
                    new { message = "Tạo hồ sơ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Lấy thông tin tình nguyện viên
        [HttpGet("{maTNV}")]
        public async Task<IActionResult> GetTinhNguyenVien(int maTNV)
        {
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

        /// Lấy danh sách tất cả tình nguyện viên
        [HttpGet]
        public async Task<IActionResult> GetAllTinhNguyenVien()
        {
            try
            {
                var result = await _service.GetAllTinhNguyenVienAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Cập nhật thông tin tình nguyện viên
        [HttpPut("{maTNV}")]
        public async Task<IActionResult> UpdateTinhNguyenVien(int maTNV, [FromForm] UpdateTinhNguyenVienDto updateDto, IFormFile? anhFile)
        {
            try
            {
                if (anhFile != null)
                {
                    var imagePath = await _service.UploadAnhDaiDienAsync(maTNV, anhFile);
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

        /// Xóa tình nguyện viên
        [HttpDelete("{maTNV}")]
        public async Task<IActionResult> DeleteTinhNguyenVien(int maTNV)
        {
            try
            {
                await _service.DeleteTinhNguyenVienAsync(maTNV);
                return Ok(new { message = "Xóa tình nguyện viên thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// Upload ảnh đại diện
        [HttpPost("{maTNV}/upload-avatar")]
        public async Task<IActionResult> UploadAvatar(int maTNV, [FromForm] IFormFile anhFile)
        {
            try
            {
                var imagePath = await _service.UploadAnhDaiDienAsync(maTNV, anhFile);
                return Ok(new { message = "Upload ảnh thành công", imagePath = imagePath });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

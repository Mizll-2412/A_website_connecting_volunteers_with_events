using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using khoaluantotnghiep.Models;
using khoaluantotnghiep.Services;
using khoaluantotnghiep.DTOs;
using Microsoft.Extensions.Logging;

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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTinhNguyenVien()
        {
            try
            {
                var result = await _service.GetAllTinhNguyenVienAsync();
                return Ok(new { success = true, message = "Lấy danh sách tình nguyện viên thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{maTNV}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTinhNguyenVien(int maTNV)
        {
            try
            {
                var result = await _service.GetTinhNguyenVienAsync(maTNV);
                return Ok(new { success = true, message = "Lấy thông tin tình nguyện viên thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                if (ex.Message.Contains("không tồn tại"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-account/{maTaiKhoan}")]
        public async Task<IActionResult> GetTinhNguyenVienByMaTaiKhoan(int maTaiKhoan)
        {
            try
            {
                var result = await _service.GetTinhNguyenVienByMaTaiKhoanAsync(maTaiKhoan);
                return Ok(new { success = true, message = "Lấy thông tin tình nguyện viên thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                if (ex.Message.Contains("không tồn tại"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedVolunteers()
        {
            try
            {
                var result = await _service.GetFeaturedTinhNguyenVienAsync();
                return Ok(new { success = true, message = "Lấy danh sách tình nguyện viên nổi bật thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateTinhNguyenVien([FromBody] CreateTNVDto createDto)
        {
            try
            {
                var result = await _service.CreateTinhNguyenVienAsync(createDto);
                return CreatedAtAction(nameof(GetTinhNguyenVien), new { maTNV = result.MaTNV },
                    new { success = true, message = "Tạo hồ sơ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{maTNV}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UpdateTinhNguyenVien(int maTNV, [FromForm] UpdateTNVDto updateDto, IFormFile? anhFile = null)
        {
            try
            {
                if (anhFile != null)
                {
                    var imagePath = await _service.UploadAnhDaiDienAsync(maTNV, anhFile);
                    updateDto.AnhDaiDien = imagePath;
                }

                var result = await _service.UpdateTinhNguyenVienAsync(maTNV, updateDto);
                return Ok(new { success = true, message = "Cập nhật thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{maTNV}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTinhNguyenVien(int maTNV)
        {
            try
            {
                await _service.DeleteTinhNguyenVienAsync(maTNV);
                return Ok(new { success = true, message = "Xóa tình nguyện viên thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{maTNV}/upload-avatar")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UploadAvatar(int maTNV, [FromForm] IFormFile anhFile)
        {
            try
            {
                var imagePath = await _service.UploadAnhDaiDienAsync(maTNV, anhFile);
                return Ok(new
                {
                    success = true,
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

        [HttpGet("{maTNV}/skill-fields")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSkillsAndFields(int maTNV)
        {
            try
            {
                var skills = await _service.GetVolunteerSkillsAsync(maTNV);
                var fields = await _service.GetVolunteerFieldsAsync(maTNV);

                return Ok(new
                {
                    success = true,
                    message = "Lấy kỹ năng và lĩnh vực thành công",
                    data = new
                    {
                        skills,
                        fields
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
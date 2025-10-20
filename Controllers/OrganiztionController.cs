using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _service;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(IOrganizationService service, ILogger<OrganizationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // Tạo mới tổ chức
        [HttpPost]
        [Authorize(Roles = "Organization, Admin")]
        public async Task<IActionResult> CreateTochuc([FromBody] CreateToChucDto createDto)
        {
            try
            {
                var result = await _service.CreateToChucAsync(createDto);
                return CreatedAtAction(nameof(GetToChuc), new { maToChuc = result.MaToChuc },
                    new { message = "Tạo hồ sơ thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{maToChuc}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteToChuc(int maToChuc)
        {
            try
            {
                await _service.DeleteToChucAsync(maToChuc);
                return Ok(new { message = "Xóa tổ chức thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
        // Lấy tất cả tổ chức
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllToChuc()
        {
            try
            {
                var result = await _service.GetAllToChucAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("{maToChuc}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetToChuc(int maToChuc)
        {
            try
            {
                var result = await _service.GetToChucAsync(maToChuc);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPut("{maToChuc}")]
        [Authorize(Roles = "Organization, Admin")]
        public async Task<IActionResult> UpdateToChuc(int maToChuc, [FromForm] UpdateToChucDto updateDto, IFormFile? anhFile)
        {
            try
            {
                if (anhFile != null)
                {
                    var imagePath = await _service.UploadAnhDaiDienAsync(maToChuc, anhFile);
                    updateDto.AnhDaiDien = imagePath;
                }

                var result = await _service.UpdateToChucAsync(maToChuc, updateDto);
                return Ok(new { message = "Cập nhật thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{maToChuc}/upload-avatar")]
        [Authorize(Roles = "Organization, Admin")]
        public async Task<IActionResult> UploadAvatar(int maToChuc, [FromForm] IFormFile anhFile)
        {
            try
            {
                var imagePath = await _service.UploadAnhDaiDienAsync(maToChuc, anhFile);
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
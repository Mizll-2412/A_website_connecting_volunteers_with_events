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
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _service;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(IOrganizationService service, ILogger<OrganizationController> logger)
        {
            _service = service;
            _logger = logger;
        }

       
        [HttpGet("{maToChuc}")]
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
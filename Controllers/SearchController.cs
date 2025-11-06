using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using khoaluantotnghiep.Services;
using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Tìm kiếm tất cả (sự kiện, tình nguyện viên, tổ chức)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchAll([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return BadRequest(new { message = "Từ khóa tìm kiếm không được để trống" });
                }

                var result = await _searchService.SearchAllAsync(keyword, page, pageSize);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tìm kiếm: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm sự kiện nâng cao
        /// </summary>
        [HttpPost("events")]
        public async Task<IActionResult> SearchEvents([FromBody] EventSearchFilterDto filter)
        {
            try
            {
                var result = await _searchService.SearchEventsAsync(filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tìm kiếm sự kiện: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm tình nguyện viên nâng cao
        /// </summary>
        [HttpPost("volunteers")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> SearchVolunteers([FromBody] VolunteerSearchFilterDto filter)
        {
            try
            {
                var result = await _searchService.SearchVolunteersAsync(filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tìm kiếm tình nguyện viên: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm tổ chức nâng cao
        /// </summary>
        [HttpPost("organizations")]
        public async Task<IActionResult> SearchOrganizations([FromBody] OrganizationSearchFilterDto filter)
        {
            try
            {
                var result = await _searchService.SearchOrganizationsAsync(filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tìm kiếm tổ chức: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

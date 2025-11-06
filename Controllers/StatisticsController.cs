using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using khoaluantotnghiep.Services;
using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thống kê tổng quan cho dashboard
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin,Organization")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                var result = await _statisticsService.GetDashboardStatisticsAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê dashboard: {ex.Message}");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy thống kê sự kiện
        /// </summary>
        [HttpGet("events")]
        [Authorize(Roles = "Admin,Organization")]
        public async Task<IActionResult> GetEventStatistics([FromQuery] StatisticFilterDto filter)
        {
            try
            {
                var result = await _statisticsService.GetEventStatisticsAsync(filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê sự kiện: {ex.Message}");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy thống kê tình nguyện viên
        /// </summary>
        [HttpGet("volunteers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVolunteerStatistics([FromQuery] StatisticFilterDto filter)
        {
            try
            {
                var result = await _statisticsService.GetVolunteerStatisticsAsync(filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê tình nguyện viên: {ex.Message}");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy thống kê tổ chức
        /// </summary>
        [HttpGet("organizations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrganizationStatistics([FromQuery] StatisticFilterDto filter)
        {
            try
            {
                var result = await _statisticsService.GetOrganizationStatisticsAsync(filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê tổ chức: {ex.Message}");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy thống kê tổng quan
        /// </summary>
        [HttpGet("overall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverallStatistics()
        {
            try
            {
                var result = await _statisticsService.GetOverallStatisticsAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê tổng quan: {ex.Message}");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy thống kê đánh giá
        /// </summary>
        [HttpGet("ratings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRatingStatistics()
        {
            try
            {
                var result = await _statisticsService.GetRatingStatisticsAsync();
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy thống kê đánh giá: {ex.Message}");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

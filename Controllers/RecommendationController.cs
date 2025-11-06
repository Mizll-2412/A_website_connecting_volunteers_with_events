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
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _service;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(IRecommendationService service, ILogger<RecommendationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách sự kiện gợi ý cho tình nguyện viên
        /// </summary>
        [HttpGet("volunteer/{maTNV}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetRecommendedEvents(
            int maTNV, 
            [FromQuery] int? maxResults = 10, 
            [FromQuery] double? locationWeight = 0.3,
            [FromQuery] double? skillWeight = 0.4,
            [FromQuery] double? interestWeight = 0.3)
        {
            try
            {
                var request = new RecommendationRequestDto
                {
                    MaTNV = maTNV,
                    MaxResults = maxResults,
                    LocationWeight = locationWeight,
                    SkillWeight = skillWeight,
                    InterestWeight = interestWeight
                };

                var result = await _service.GetRecommendedEventsAsync(request);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy sự kiện gợi ý: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy điểm phù hợp giữa tình nguyện viên và sự kiện
        /// </summary>
        [HttpGet("match-score")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetMatchScore(
            [FromQuery] int maSuKien,
            [FromQuery] int maTNV,
            [FromQuery] double? locationWeight = 0.3,
            [FromQuery] double? skillWeight = 0.4,
            [FromQuery] double? interestWeight = 0.3)
        {
            try
            {
                var score = await _service.CalculateMatchScoreAsync(
                    maSuKien, 
                    maTNV, 
                    locationWeight, 
                    skillWeight, 
                    interestWeight);
                
                return Ok(new { score });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tính điểm phù hợp: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Lấy danh sách sự kiện gợi ý cho tình nguyện viên theo lĩnh vực
        /// </summary>
        [HttpPost("volunteer/{maTNV}/field-preferences")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetRecommendedEventsByFields(
            int maTNV, 
            [FromBody] List<int> linhVucIds,
            [FromQuery] int? maxResults = 10)
        {
            try
            {
                var request = new RecommendationRequestDto
                {
                    MaTNV = maTNV,
                    MaxResults = maxResults,
                    LinhVucPreferences = linhVucIds
                };

                var result = await _service.GetRecommendedEventsAsync(request);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy sự kiện gợi ý theo lĩnh vực: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

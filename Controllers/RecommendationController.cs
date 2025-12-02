using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using khoaluantotnghiep.DTOs;
using Newtonsoft.Json;
using khoaluantotnghiep.Services;
using System.Text;

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
        [HttpGet("python/{maTNV}")]
        public IActionResult GetModelRecommendations(int maTNV)
        {
            try
            {
                string pythonExe = @"D:\WebsiteKetNoiTinhNguyen\rcm\venv\Scripts\python.exe";
                string scriptPath = @"D:\WebsiteKetNoiTinhNguyen\rcm\recommend_api.py";

                var psi = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = $"\"{scriptPath}\" {maTNV}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                };

                var process = Process.Start(psi);

                string output = process!.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                    return BadRequest(new { error });

                var data = JsonConvert.DeserializeObject<List<Recommendation>>(output);

                return Ok(new
                {
                    status = "success",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
    }
}

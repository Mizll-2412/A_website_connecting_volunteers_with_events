using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;

namespace khoaluantotnghiep.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ICertificateGeneratorService _generatorService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CertificateController> _logger;

        public CertificateController(
            ICertificateService certificateService, 
            ICertificateGeneratorService generatorService,
            INotificationService notificationService, 
            ILogger<CertificateController> logger)
        {
            _certificateService = certificateService;
            _generatorService = generatorService;
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Mẫu giấy chứng nhận

        /// <summary>
        /// Tạo mẫu giấy chứng nhận mới
        /// </summary>
        [HttpPost("samples")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> CreateCertificateSample([FromForm] CreateCertificateSampleDto createDto)
        {
            try
            {
                var result = await _certificateService.CreateCertificateSampleAsync(createDto);
                return CreatedAtAction(nameof(GetCertificateSampleById), new { maMau = result.MaMau }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo mẫu giấy chứng nhận: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin mẫu giấy chứng nhận theo ID
        /// </summary>
        [HttpGet("samples/{maMau}")]
        [Authorize]
        public async Task<IActionResult> GetCertificateSampleById(int maMau)
        {
            try
            {
                var result = await _certificateService.GetCertificateSampleByIdAsync(maMau);
                return Ok(new { data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy mẫu giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả mẫu giấy chứng nhận (mặc định + của sự kiện)
        /// </summary>
        [HttpGet("samples")]
        [Authorize]
        public async Task<IActionResult> GetAllCertificateSamples()
        {
            try
            {
                var allSamples = await _certificateService.GetAllCertificateSamplesAsync();
                return Ok(new { data = allSamples });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách tất cả mẫu giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách mẫu giấy chứng nhận theo sự kiện
        /// </summary>
        [HttpGet("samples/events/{maSuKien}")]
        [Authorize]
        public async Task<IActionResult> GetCertificateSamplesByEvent(int maSuKien)
        {
            try
            {
                var result = await _certificateService.GetCertificateSamplesByEventAsync(maSuKien);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách mẫu giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa mẫu giấy chứng nhận
        /// </summary>
        [HttpDelete("samples/{maMau}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> DeleteCertificateSample(int maMau)
        {
            try
            {
                var result = await _certificateService.DeleteCertificateSampleAsync(maMau);
                if (result)
                    return Ok(new { message = "Xóa mẫu giấy chứng nhận thành công" });
                else
                    return NotFound(new { message = "Không tìm thấy mẫu giấy chứng nhận" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa mẫu giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đặt mẫu giấy chứng nhận làm mặc định
        /// </summary>
        [HttpPost("samples/{maMau}/set-default")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> SetDefaultCertificateSample(int maMau)
        {
            try
            {
                var result = await _certificateService.SetDefaultCertificateSampleAsync(maMau);
                return Ok(new { message = "Đặt mẫu mặc định thành công", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi đặt mẫu mặc định: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion

        #region Giấy chứng nhận

        /// <summary>
        /// Phát hành giấy chứng nhận cho tình nguyện viên
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> IssueCertificate([FromForm] IssueCertificateDto issueDto)
        {
            try
            {
                var result = await _certificateService.IssueCertificateAsync(issueDto);
                
                // Gửi thông báo cho TNV
                try
                {
                    await _notificationService.SendCertificateIssuedNotificationAsync(result.MaGiayChungNhan, issueDto.MaTNV);
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning($"Không thể gửi thông báo phát chứng nhận: {notifEx.Message}");
                }
                
                return CreatedAtAction(nameof(GetCertificateById), new { maGiayChungNhan = result.MaGiayChungNhan }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi phát hành giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin giấy chứng nhận theo ID (Public - cho phép share link)
        /// </summary>
        [HttpGet("{maGiayChungNhan}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCertificateById(int maGiayChungNhan)
        {
            try
            {
                var result = await _certificateService.GetCertificateByIdAsync(maGiayChungNhan);
                return Ok(new { data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách giấy chứng nhận theo tình nguyện viên
        /// </summary>
        [HttpGet("volunteers/{maTNV}")]
        [Authorize]
        public async Task<IActionResult> GetCertificatesByVolunteer(int maTNV)
        {
            try
            {
                var result = await _certificateService.GetCertificatesByVolunteerAsync(maTNV);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách giấy chứng nhận theo tình nguyện viên: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách giấy chứng nhận theo sự kiện
        /// </summary>
        [HttpGet("events/{maSuKien}")]
        [Authorize]
        public async Task<IActionResult> GetCertificatesByEvent(int maSuKien)
        {
            try
            {
                var result = await _certificateService.GetCertificatesByEventAsync(maSuKien);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách giấy chứng nhận theo sự kiện: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Phát hành giấy chứng nhận hàng loạt cho TNV đã được duyệt của sự kiện
        /// </summary>
        [HttpPost("events/{maSuKien}/issue-bulk/{maMau}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> IssueCertificatesBulk(int maSuKien, int maMau)
        {
            try
            {
                var result = await _certificateService.IssueCertificatesToEventParticipantsAsync(maSuKien, maMau);
                
                // Gửi thông báo cho tất cả TNV nhận được chứng nhận
                foreach (var cert in result)
                {
                    try
                    {
                        await _notificationService.SendCertificateIssuedNotificationAsync(cert.MaGiayChungNhan, cert.MaTNV);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogWarning($"Không thể gửi thông báo phát chứng nhận cho TNV {cert.MaTNV}: {notifEx.Message}");
                    }
                }
                
                return Ok(new { message = $"Đã phát hành {result.Count} giấy chứng nhận", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi phát hành hàng loạt: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách giấy chứng nhận theo các điều kiện lọc
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCertificatesWithFilter([FromQuery] CertificateFilterDto filter)
        {
            try
            {
                var result = await _certificateService.GetCertificatesWithFilterAsync(filter);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy danh sách giấy chứng nhận theo filter: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa giấy chứng nhận
        /// </summary>
        [HttpDelete("{maGiayChungNhan}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> DeleteCertificate(int maGiayChungNhan)
        {
            try
            {
                var result = await _certificateService.DeleteCertificateAsync(maGiayChungNhan);
                if (result)
                    return Ok(new { message = "Xóa giấy chứng nhận thành công" });
                else
                    return NotFound(new { message = "Không tìm thấy giấy chứng nhận" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Phát hành giấy chứng nhận cho tất cả tình nguyện viên tham gia sự kiện
        /// </summary>
        [HttpPost("batch-issue/{maSuKien}/{maMau}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> IssueCertificatesToEventParticipants(int maSuKien, int maMau)
        {
            try
            {
                var result = await _certificateService.IssueCertificatesToEventParticipantsAsync(maSuKien, maMau);
                
                // Gửi thông báo cho tất cả TNV nhận được chứng nhận
                foreach (var cert in result)
                {
                    try
                    {
                        await _notificationService.SendCertificateIssuedNotificationAsync(cert.MaGiayChungNhan, cert.MaTNV);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogWarning($"Không thể gửi thông báo phát chứng nhận cho TNV {cert.MaTNV}: {notifEx.Message}");
                    }
                }
                
                return Ok(new { message = $"Đã phát hành {result.Count} giấy chứng nhận", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi phát hành hàng loạt giấy chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion

        #region Certificate Generation & Download

        /// <summary>
        /// Lấy preview ảnh chứng nhận (base64) (Public - cho phép share link)
        /// </summary>
        [HttpGet("{maGiayChungNhan}/preview")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCertificatePreview(int maGiayChungNhan)
        {
            try
            {
                var base64Image = await _generatorService.GeneratePreviewImageAsync(maGiayChungNhan);
                return Ok(new { data = $"data:image/png;base64,{base64Image}" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo preview chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tải về chứng nhận (image hoặc PDF) (Public - cho phép share link)
        /// </summary>
        [HttpGet("{maGiayChungNhan}/download")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadCertificate(int maGiayChungNhan, [FromQuery] string format = "image")
        {
            try
            {
                var certData = await _generatorService.GetCertificateDataAsync(maGiayChungNhan);
                
                if (format.ToLower() == "pdf")
                {
                    var pdfBytes = await _generatorService.GenerateCertificatePdfAsync(maGiayChungNhan);
                    return File(pdfBytes, "application/pdf", $"ChungNhan_{certData.MaChungNhan}.pdf");
                }
                else
                {
                    var imageBytes = await _generatorService.GenerateCertificateImageAsync(maGiayChungNhan);
                    return File(imageBytes, "image/png", $"ChungNhan_{certData.MaChungNhan}.png");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tải chứng nhận: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lưu cấu hình template cho mẫu chứng nhận
        /// </summary>
        [HttpPost("samples/{maMau}/config")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> SaveTemplateConfig(int maMau, [FromBody] TemplateConfigDto configDto)
        {
            try
            {
                var result = await _certificateService.UpdateTemplateConfigAsync(maMau, configDto);
                return Ok(new { message = "Lưu cấu hình thành công", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lưu cấu hình template: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy cấu hình template của mẫu chứng nhận
        /// </summary>
        [HttpGet("samples/{maMau}/config")]
        [Authorize]
        public async Task<IActionResult> GetTemplateConfig(int maMau)
        {
            try
            {
                var result = await _certificateService.GetTemplateConfigAsync(maMau);
                return Ok(new { data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy cấu hình template: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion
    }
}

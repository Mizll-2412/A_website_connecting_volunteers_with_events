using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var respone = await _authService.LoginAsync(request);
            if (!respone.Success)
            {
                return Unauthorized(respone);
            }
            return Ok(respone);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register ([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _authService.RegisterAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }
                
                var result = await _authService.LogoutAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(result); // Luôn trả về OK để không tiết lộ thông tin về email tồn tại hay không
        }
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.ResetPasswordAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        [HttpPost("change-email/request")]
        [Authorize]
        public async Task<IActionResult> RequestChangeEmail([FromBody] ChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng" });
            }
            var result = await _authService.RequestChangeEmailAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("change-email/confirm")]
        public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.ConfirmChangeEmailAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
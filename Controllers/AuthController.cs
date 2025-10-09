using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Services;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/controller")]
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
    }
}
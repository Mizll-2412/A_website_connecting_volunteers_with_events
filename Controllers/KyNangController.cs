using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using khoaluantotnghiep.Services;
using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KyNangController : ControllerBase
    {
        private readonly IKyNangService _kynangService;

        public KyNangController(IKyNangService kyNangService)
        {
            _kynangService = kyNangService;
        }

        // GET: api/kynang
        // Tất cả user đều xem được
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var kyNang = await _kynangService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách lĩnh vực thành công",
                    data = kyNang
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách kỹ năng",
                    error = ex.Message
                });
            }
        }

        // GET: api/kynang/5
        // Tất cả user đều xem được
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var linhVuc = await _kynangService.GetByIdAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin kỹ năng thành công",
                    data = linhVuc
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thông tin kỹ năng",
                    error = ex.Message
                });
            }
        }

        // POST: api/kynang
        // Chỉ Admin mới tạo được
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateKyNangRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                var kyNang = await _kynangService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = kyNang.MaKyNang }, new
                {
                    success = true,
                    message = "Tạo kỹ năng thành công",
                    data = kyNang
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tạo lĩnh vực",
                    error = ex.Message
                });
            }
        }

        // PUT: api/kynang/5
        // Chỉ Admin mới sửa được
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateKyNangRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                var kyNang = await _kynangService.UpdateAsync(id, request);
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật kỹ năng thành công",
                    data = kyNang
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật kỹ năng",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/kynang/5
        // Chỉ Admin mới xóa được
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _kynangService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Xóa kỹ năng thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xóa kỹ năng",
                    error = ex.Message
                });
            }
        }
    }
}

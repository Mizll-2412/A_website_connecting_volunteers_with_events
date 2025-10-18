using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using khoaluantotnghiep.Services;
using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinhVucController : ControllerBase
    {
        private readonly ILinhVucService _linhVucService;

        public LinhVucController(ILinhVucService linhVucService)
        {
            _linhVucService = linhVucService;
        }

        // GET: api/linhvuc
        // Tất cả user đều xem được
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var linhVucs = await _linhVucService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách lĩnh vực thành công",
                    data = linhVucs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách lĩnh vực",
                    error = ex.Message
                });
            }
        }

        // GET: api/linhvuc/5
        // Tất cả user đều xem được
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var linhVuc = await _linhVucService.GetByIdAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin lĩnh vực thành công",
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
                    message = "Có lỗi xảy ra khi lấy thông tin lĩnh vực",
                    error = ex.Message
                });
            }
        }

        // POST: api/linhvuc
        // Chỉ Admin mới tạo được
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLinhVucRequest request)
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
                var linhVuc = await _linhVucService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = linhVuc.MaLinhVuc }, new
                {
                    success = true,
                    message = "Tạo lĩnh vực thành công",
                    data = linhVuc
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

        // PUT: api/linhvuc/5
        // Chỉ Admin mới sửa được
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLinhVucRequest request)
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
                var linhVuc = await _linhVucService.UpdateAsync(id, request);
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật lĩnh vực thành công",
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
                    message = "Có lỗi xảy ra khi cập nhật lĩnh vực",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/linhvuc/5
        // Chỉ Admin mới xóa được
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _linhVucService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Xóa lĩnh vực thành công"
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
                    message = "Có lỗi xảy ra khi xóa lĩnh vực",
                    error = ex.Message
                });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using khoaluantotnghiep.Models;
using khoaluantotnghiep.Data;

namespace khoaluantotnghiep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_context.User.ToList());
        }

        [HttpPost]
        public IActionResult AddUser(TaiKhoan user)
        {
            _context.User.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }
    }
}
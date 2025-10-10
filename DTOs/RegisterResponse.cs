using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.DTOs
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
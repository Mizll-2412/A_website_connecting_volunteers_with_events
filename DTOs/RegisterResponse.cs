using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.DTOs
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo UserInfo { get; set; } = new();
        public int? MaToChuc { get; set; }
        public int? MaTNV { get; set; }  
        public int? MaAdmin { get; set; }
    }
}
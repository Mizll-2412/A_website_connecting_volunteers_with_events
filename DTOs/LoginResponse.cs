namespace khoaluantotnghiep.DTOs
{
    public class LoginRespone
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserInfo UserInfo { get; set; } = new();
    }
    public class UserInfo
    {
        public int MaTaiKhoan { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        
    }
}
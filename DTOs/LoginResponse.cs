namespace khoaluantotnghiep.DTOs
{
    public class LoginRespone
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserInfo UserInfo { get; set; }
    }
    public class UserInfo
    {
        public int MaTaiKhoan { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string VaiTro { get; set; }
        
    }
}
using System;

namespace khoaluantotnghiep.DTOs
{
    public class AdminUserDto
    {
        public int MaTaiKhoan { get; set; }
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? LanDangNhapCuoi { get; set; }
        public AdminUserVolunteerDto? Volunteer { get; set; }
        public AdminUserOrganizationDto? Organization { get; set; }
    }

    public class AdminUserVolunteerDto
    {
        public int MaTNV { get; set; }
        public string? HoTen { get; set; }
        public string? AnhDaiDien { get; set; }
    }

    public class AdminUserOrganizationDto
    {
        public int MaToChuc { get; set; }
        public string? TenToChuc { get; set; }
        public string? AnhDaiDien { get; set; }
    }
}


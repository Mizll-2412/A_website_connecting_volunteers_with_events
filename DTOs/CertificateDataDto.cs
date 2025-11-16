namespace khoaluantotnghiep.DTOs
{
    public class CertificateDataDto
    {
        public string TenTNV { get; set; } = string.Empty;
        public string TenSuKien { get; set; } = string.Empty;
        public string TenToChuc { get; set; } = string.Empty;
        public DateTime NgayCap { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? DiaChi { get; set; }
        public int? SoGioThamGia { get; set; }
        public string MaChungNhan { get; set; } = string.Empty;
        public string? LogoToChuc { get; set; }
    }

    public class TemplateFieldConfig
    {
        public string Key { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int FontSize { get; set; } = 24;
        public string FontFamily { get; set; } = "Arial";
        public string Color { get; set; } = "#000000";
        public string Align { get; set; } = "center";
        public string FontWeight { get; set; } = "normal";
    }

    public class TemplateConfig
    {
        public List<TemplateFieldConfig> Fields { get; set; } = new List<TemplateFieldConfig>();
    }
}


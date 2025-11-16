using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("MauGiayChungNhan")]
    public class MauGiayChungNhan
    {
        [Key]
        [Column("MaMau")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaMau { get; set; }

        [Column("MaSuKien")]
        public int? MaSuKien { get; set; }

        [StringLength(255)]
        [Column("TenMau")]
        public string? TenMau { get; set; }

        [StringLength(500)]
        [Column("MoTa")]
        public string? MoTa { get; set; }

        [Column("IsDefault")]
        public bool IsDefault { get; set; }

        [Column("NgayGui")]
        public DateTime? NgayGui { get; set; }

        [StringLength(255)]
        [Column("File")]
        public string? File { get; set; }

        [Column("TemplateConfig", TypeName = "nvarchar(max)")]
        public string? TemplateConfig { get; set; }

        [StringLength(255)]
        [Column("BackgroundImage")]
        public string? BackgroundImage { get; set; }

        [Column("Width")]
        public int Width { get; set; } = 1200;

        [Column("Height")]
        public int Height { get; set; } = 800;

        [ForeignKey("MaSuKien")]
        public virtual SuKien? SuKien { get; set; }
    }
}

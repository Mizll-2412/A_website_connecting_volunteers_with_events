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

        [Required]
        [Column("MaSuKien")]
        public int MaSuKien { get; set; }

        [Column("NgayGui")]
        public DateTime? NgayGui { get; set; }

        [StringLength(255)]
        [Column("File")]
        public string? File { get; set; }

        [ForeignKey("MaSuKien")]
        public virtual SuKien SuKien { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("GiayToPhapLy")]
    public class GiayToPhapLy
    {
        [Key]
        [Column("MaGiayTo")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGiayTo { get; set; }

        [Required]
        [Column("MaToChuc")]
        public int MaToChuc { get; set; }

        [StringLength(200)]
        [Column("TenGiayTo")]
        public string? TenGiayTo { get; set; }

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(255)]
        [Column("File")]
        public string? File { get; set; }

        [StringLength(500)]
        [Column("MoTa")]
        public string? MoTa { get; set; }

        [ForeignKey("MaToChuc")]
        public virtual ToChuc ToChuc { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace khoaluantotnghiep.Models
{
    [Table("LegalDocument")]
    public class LegalDocument
    {
        [Key]
        [Column("MaGiayTo")]
        public int MaGiayTo { get; set; }

        [Required]
        [StringLength(100)]
        [Column("TenGiayTo")]
        public string TenGiayTo { get; set; }
    
        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Column("File")]
        public string File { get; set; }

    }
}
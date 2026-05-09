using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirFikrimVar.Core.Entities
{
    public class FikirDosyasi
    {
        [Key]
        public int Id { get; set; }

        public int FikirId { get; set; }
        [ForeignKey("FikirId")]
        public Fikir Fikir { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string OrijinalDosyaAdi { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string KayitliDosyaYolu { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Uzanti { get; set; } = null!;

        public long DosyaBoyutu { get; set; } // Byte cinsinden

        public DateTime YuklenmeTarihi { get; set; } = DateTime.UtcNow;
    }
}

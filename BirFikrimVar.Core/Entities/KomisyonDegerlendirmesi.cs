using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirFikrimVar.Core.Entities
{
    public class KomisyonDegerlendirmesi
    {
        [Key]
        public int Id { get; set; }

        public int FikirId { get; set; }
        [ForeignKey("FikirId")]
        public Fikir Fikir { get; set; } = null!;

        [Required]
        public string DegerlendiriciId { get; set; } = null!;

        [ForeignKey("DegerlendiriciId")]
        public virtual Kullanici Degerlendirici { get; set; } = null!;

        [Range(1, 10)]
        public int Puan { get; set; }

        public DateTime DegerlendirmeTarihi { get; set; } = DateTime.UtcNow;
    }
}

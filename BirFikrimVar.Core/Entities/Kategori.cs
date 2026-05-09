using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Core.Entities
{
    public class Kategori
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ad { get; set; } = null!;

        public bool AktifMi { get; set; } = true;
    }
}

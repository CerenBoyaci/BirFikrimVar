using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Core.Dtos.Admin
{
    public class KategoriDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string Ad { get; set; }
        public bool AktifMi { get; set; }
    }

    public class OverrideStatusDto
    {
        [Required]
        public int YeniDurum { get; set; }

        [Required(ErrorMessage = "Müdahale gerekçesi (log için) zorunludur.")]
        public string Aciklama { get; set; }
    }
}

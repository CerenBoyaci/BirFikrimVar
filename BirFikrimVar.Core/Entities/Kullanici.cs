using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;


namespace BirFikrimVar.Core.Entities
{
    public class Kullanici : IdentityUser<string>
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public bool AktifMi { get; set; } = true;


        public ICollection<Fikir> Fikirler { get; set; }
        public ICollection<Degerlendirme> Degerlendirmeler { get; set; }
    }
}

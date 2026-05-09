using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;


namespace BirFikrimVar.Core.Entities
{
    public class Kullanici : IdentityUser<string>
    {
        //bunu kullanma sebebim standart IdentityUser kullanıldığında arkada id üretiyor. bu metot sayesinde kullanıcı için benzersiz id üreteceğiz
        public Kullanici()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Ad { get; set; }
        public string Soyad { get; set; }
        public bool AktifMi { get; set; } = true;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenEndDate { get; set; }

        public ICollection<Fikir> Fikirler { get; set; }
        public ICollection<Degerlendirme> Degerlendirmeler { get; set; }
    }
}

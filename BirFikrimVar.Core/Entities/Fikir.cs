using System;
using System.Collections.Generic;
using System.Text;
using BirFikrimVar.Core.Enums;

namespace BirFikrimVar.Core.Entities
{
    public class Fikir
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public string Ozet { get; set; }
        public string Kapsam { get; set; }
        public string YenilikciYan { get; set; }

        // SWOT analizi alanları
        public string GucluYonler { get; set; }
        public string ZayifYonler { get; set; }
        public string Firsatlar { get; set; }
        public string Tehditler { get; set; }

        public FikirDurumu Durum { get; set; } = FikirDurumu.Taslak;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? GuncellemeTarihi { get; set; }

        public string BasvuruSahibiId { get; set; }
        public Kullanici BasvuruSahibi { get; set; }

        public ICollection<DosyaEki> EkDosyalar { get; set; }
        public ICollection<Degerlendirme> Degerlendirmeler { get; set; }
        public ICollection<FikirDurumGecmisi> DurumGecmisleri { get; set; }
    }
}

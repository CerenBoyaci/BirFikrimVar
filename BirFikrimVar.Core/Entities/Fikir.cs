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
        public string GucluYonler { get; set; }
        public string ZayifYonler { get; set; }
        public string Firsatlar { get; set; }
        public string Tehditler { get; set; }

        public FikirDurumu Durum { get; set; } = FikirDurumu.Taslak;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? GuncellemeTarihi { get; set; }

        public string BasvuruSahibiId { get; set; }
        public Kullanici BasvuruSahibi { get; set; }

        // --- NAVIGATION PROPERTIES (HER BİRİNDEN SADECE BİR TANE OLMALI) ---
        public virtual ICollection<FikirDosyasi> FikirDosyalari { get; set; } = new List<FikirDosyasi>();
        public virtual ICollection<OnOnayDegerlendirmesi> OnOnayDegerlendirmeleri { get; set; } = new List<OnOnayDegerlendirmesi>();
        public virtual ICollection<KomisyonDegerlendirmesi> KomisyonDegerlendirmeleri { get; set; } = new List<KomisyonDegerlendirmesi>();

        // Eğer veritabanında bu tablolar varsa kalsın, yoksa hata verebilir:
        public virtual ICollection<FikirDurumGecmisi> FikirDurumGecmisleri { get; set; } = new List<FikirDurumGecmisi>();
    }
}

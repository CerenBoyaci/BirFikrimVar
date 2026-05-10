using System;
using System.Collections.Generic;
using System.Text;

namespace BirFikrimVar.Core.Dtos.Fikir
{
    public class FikirDetayDto
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
        public string Durum { get; set; }
        public string BasvuruSahibiAdSoyad { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public List<FikirDosyaDto> Dosyalar { get; set; } = new List<FikirDosyaDto>();
    }

    public class FikirDosyaDto
    {
        public int Id { get; set; }
        public string DosyaAdi { get; set; }
    }

}

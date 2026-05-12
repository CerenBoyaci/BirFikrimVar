using System;
using System.Collections.Generic;
using System.Text;

namespace BirFikrimVar.Core.Dtos.Fikir
{
    public class FikirListeDto
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public string Durum { get; set; }
        public string BasvuruSahibiAdSoyad { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public List<PuanBilgiDto> OnOnayPuanlari { get; set; } = new();
        public List<PuanBilgiDto> KomisyonPuanlari { get; set; } = new();
    }
    public class PuanBilgiDto
    {
        public string Isim { get; set; }
        public double Puan { get; set; }
        public string Rol { get; set; }
    }
}

using BirFikrimVar.Core.Enums;
using BirFikrimVar.Core.Enums.BirFikrimVar.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BirFikrimVar.Core.Entities
{
    public class Degerlendirme
    {
        public int Id { get; set; }
        public int FikirId { get; set; }
        public Fikir Fikir { get; set; }

        public string DegerlendiriciId { get; set; }
        public Kullanici Degerlendirici { get; set; }

        public DegerlendirmeTuru Tur { get; set; }

       
        public int StratejikUyumPuani { get; set; }
        public int YenilikcilikPuani { get; set; }
        public int UygulanabilirlikPuani { get; set; }
        public int BeklenenFaydaPuani { get; set; }
        public int RiskVeSurdurulebilirlikPuani { get; set; }
        public int KaynakIhtiyaciPuani { get; set; }

        public string EkNot { get; set; }
        public DateTime DegerlendirmeTarihi { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BirFikrimVar.Core.Entities
{
    public class DosyaEki
    {
        public int Id { get; set; }
        public int FikirId { get; set; }
        public Fikir Fikir { get; set; }

        public string OrijinalAd { get; set; }
        public string GuvenliAd { get; set; } 
        public string DosyaYolu { get; set; }
        public string Uzanti { get; set; }
        public string MimeTuru { get; set; }
        public long BoyutBayt { get; set; }
        public DateTime YuklenmeTarihi { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BirFikrimVar.Core.Entities
{
    public class AdminLog
    {
        public int Id { get; set; }

        public string AdminId { get; set; }
        public Kullanici Admin { get; set; } 

        public string IslemTipi { get; set; }

        public string Aciklama { get; set; }

        public DateTime IslemTarihi { get; set; } = DateTime.UtcNow;
    }
}

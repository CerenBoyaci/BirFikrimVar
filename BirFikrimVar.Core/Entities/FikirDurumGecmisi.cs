using BirFikrimVar.Core.Entities;
using BirFikrimVar.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

public class FikirDurumGecmisi
{
    public int Id { get; set; }
    public int FikirId { get; set; }
    public Fikir Fikir { get; set; }

    public FikirDurumu EskiDurum { get; set; }
    public FikirDurumu YeniDurum { get; set; }

    public string IslemYapanKullaniciId { get; set; }
    public Kullanici IslemYapanKullanici { get; set; }

    public string Aciklama { get; set; } //admin manuel değiştirdi veya ön onaydan geçti gibi
    public DateTime IslemTarihi { get; set; } = DateTime.UtcNow;
}

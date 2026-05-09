// BirFikrimVar.Service.Services.AdminService.cs
using BirFikrimVar.Core.Dtos.Admin;
using BirFikrimVar.Core.Entities;
using BirFikrimVar.Core.Enums;
using BirFikrimVar.Data.Context;
using BirFikrimVar.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BirFikrimVar.Service.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

  

        public async Task<IEnumerable<KategoriDto>> GetCategoriesAsync()
        {
            return await _context.Kategoriler
                .Select(k => new KategoriDto
                {
                    Id = k.Id,
                    Ad = k.Ad,
                    AktifMi = k.AktifMi
                }).ToListAsync();
        }

        public async Task<string> CreateCategoryAsync(KategoriDto model)
        {
            var kategori = new Kategori { Ad = model.Ad, AktifMi = model.AktifMi };
            _context.Kategoriler.Add(kategori);
            await _context.SaveChangesAsync();
            return "Basarili";
        }

        public async Task<string> UpdateCategoryAsync(int id, KategoriDto model)
        {
            var kategori = await _context.Kategoriler.FindAsync(id);
            if (kategori == null) return "Bulunamadi";

            kategori.Ad = model.Ad;
            kategori.AktifMi = model.AktifMi;
            await _context.SaveChangesAsync();
            return "Basarili";
        }

     

        public async Task<string> OverrideIdeaStatusAsync(int fikirId, OverrideStatusDto model, string adminId)
        {
            var fikir = await _context.Fikirler.FindAsync(fikirId);
            if (fikir == null) return "Bulunamadi";

            var eskiDurum = fikir.Durum;
            fikir.Durum = (FikirDurumu)model.YeniDurum;
            fikir.GuncellemeTarihi = DateTime.UtcNow;


            _context.FikirDurumGecmisleri.Add(new FikirDurumGecmisi
            {
                FikirId = fikir.Id,
                EskiDurum = eskiDurum,
                YeniDurum = fikir.Durum,
                IslemYapanKullaniciId = adminId,
                IslemTarihi = DateTime.UtcNow,
                Aciklama = $"[ADMIN MÜDAHALESİ - OVERRIDE] {model.Aciklama}"
            });

            await _context.SaveChangesAsync();
            return "Basarili";
        }


        public async Task<object> GetIdeaMonitoringDataAsync(int fikirId)
        {
            var fikir = await _context.Fikirler
                .Include(f => f.BasvuruSahibi)
                .FirstOrDefaultAsync(f => f.Id == fikirId);

            if (fikir == null) return null;

            var durumGecmisi = await _context.FikirDurumGecmisleri
                .Include(g => g.IslemYapanKullanici)
                .Where(g => g.FikirId == fikirId)
                .OrderByDescending(g => g.IslemTarihi)
                .Select(g => new
                {
                    EskiDurum = g.EskiDurum.ToString(),
                    YeniDurum = g.YeniDurum.ToString(),
                    IslemYapan = g.IslemYapanKullanici.Ad + " " + g.IslemYapanKullanici.Soyad,
                    Tarih = g.IslemTarihi,
                    Aciklama = g.Aciklama
                }).ToListAsync();

            var onOnaylar = await _context.OnOnayDegerlendirmeleri
                .Include(o => o.Kategori)
                .Where(o => o.FikirId == fikirId)
                .Select(o => new
                {
                    KategoriAd = o.Kategori.Ad,
                    Puan = o.Puan,
                    Tarih = o.DegerlendirmeTarihi
                })
                .ToListAsync();

            var komisyonlar = await _context.KomisyonDegerlendirmeleri
                .Where(k => k.FikirId == fikirId)
                .Select(k => new
                {
                    Puan = k.Puan,
                    Tarih = k.DegerlendirmeTarihi
                })
                .ToListAsync();

            return new
            {
                FikirBilgisi = new { fikir.Baslik, Durum = fikir.Durum.ToString(), Sahibi = fikir.BasvuruSahibi.Ad + " " + fikir.BasvuruSahibi.Soyad },
                DurumGecmisi = durumGecmisi,
                OnOnayDegerlendirmeleri = onOnaylar,
                KomisyonDegerlendirmeleri = komisyonlar
            };
        }
    }
}
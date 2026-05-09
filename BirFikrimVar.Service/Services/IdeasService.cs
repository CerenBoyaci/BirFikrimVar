using BirFikrimVar.Core.Dtos.Fikir;
using BirFikrimVar.Core.Entities;
using BirFikrimVar.Core.Enums;
using BirFikrimVar.Data.Context;
using BirFikrimVar.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BirFikrimVar.Service.Services
{
    public class IdeasService : IIdeasService
    {
        private readonly AppDbContext _context;

        public IdeasService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateIdeaAsync(FikirOlusturDto model, string userId)
        {
            var yeniFikir = new Fikir
            {
                Baslik = model.Baslik,
                Ozet = model.Ozet,
                Kapsam = model.Kapsam,
                YenilikciYan = model.YenilikciYan,
                GucluYonler = model.GucluYonler,
                ZayifYonler = model.ZayifYonler,
                Firsatlar = model.Firsatlar,
                Tehditler = model.Tehditler,
                BasvuruSahibiId = userId,
                Durum = FikirDurumu.Taslak,
                OlusturmaTarihi = DateTime.UtcNow
            };

            _context.Fikirler.Add(yeniFikir);
            await _context.SaveChangesAsync();

            _context.FikirDurumGecmisleri.Add(new FikirDurumGecmisi
            {
                FikirId = yeniFikir.Id,
                EskiDurum = FikirDurumu.Taslak,
                YeniDurum = FikirDurumu.Taslak,
                IslemYapanKullaniciId = userId,
                Aciklama = "Fikir taslak olarak oluşturuldu.",
                IslemTarihi = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return yeniFikir.Id;
        }

        public async Task<IEnumerable<FikirListeDto>> GetIdeasAsync(string userId, IList<string> userRoles)
        {
            var query = _context.Fikirler.Include(f => f.BasvuruSahibi).AsQueryable();

            if (userRoles.Contains("Admin"))
            {
               //admin için filtreye gerek yok çünkü o tüm fikirleri zaten görmeli
            }
            else if (userRoles.Contains("OnOnayci"))
            {
                query = query.Where(f => f.Durum == FikirDurumu.OnOnayBekliyor);
            }
            else if (userRoles.Contains("KomisyonUyesi"))
            {
                query = query.Where(f => f.Durum == FikirDurumu.KomisyonOnayiBekliyor);
            }
            else
            {
                //standart kullanıcı sadece kendi fikirlerini görsün
                query = query.Where(f => f.BasvuruSahibiId == userId);
            }

            return await query.Select(f => new FikirListeDto
            {
                Id = f.Id,
                Baslik = f.Baslik,
                Durum = f.Durum.ToString(),
                BasvuruSahibiAdSoyad = f.BasvuruSahibi.Ad + " " + f.BasvuruSahibi.Soyad,
                OlusturmaTarihi = f.OlusturmaTarihi
            }).ToListAsync();
        }

        public async Task<FikirDetayDto> GetIdeaDetailAsync(int id, string userId, IList<string> userRoles)
        {
            var fikir = await _context.Fikirler.Include(f => f.BasvuruSahibi).FirstOrDefaultAsync(f => f.Id == id);

            if (fikir == null) return null;

            if (!userRoles.Contains("Admin") && !userRoles.Contains("OnOnayci") && !userRoles.Contains("KomisyonUyesi"))
            {
                if (fikir.BasvuruSahibiId != userId) return null;
            }

            return new FikirDetayDto
            {
                Id = fikir.Id,
                Baslik = fikir.Baslik,
                Ozet = fikir.Ozet,
                Kapsam = fikir.Kapsam,
                YenilikciYan = fikir.YenilikciYan,
                GucluYonler = fikir.GucluYonler,
                ZayifYonler = fikir.ZayifYonler,
                Firsatlar = fikir.Firsatlar,
                Tehditler = fikir.Tehditler,
                Durum = fikir.Durum.ToString(),
                BasvuruSahibiAdSoyad = fikir.BasvuruSahibi.Ad + " " + fikir.BasvuruSahibi.Soyad,
                OlusturmaTarihi = fikir.OlusturmaTarihi
            };
        }

        public async Task<string> UpdateIdeaAsync(int id, FikirGuncelleDto model, string userId)
        {
            var fikir = await _context.Fikirler.FindAsync(id);
            if (fikir == null) return "Bulunamadi";

            if (fikir.BasvuruSahibiId != userId) return "Yetkisiz";

            if (fikir.Durum != FikirDurumu.Taslak)
                return "Sadece taslak durumundaki fikirler güncellenebilir.";

            fikir.Baslik = model.Baslik;
            fikir.Ozet = model.Ozet;
            fikir.Kapsam = model.Kapsam;
            fikir.YenilikciYan = model.YenilikciYan;
            fikir.GucluYonler = model.GucluYonler;
            fikir.ZayifYonler = model.ZayifYonler;
            fikir.Firsatlar = model.Firsatlar;
            fikir.Tehditler = model.Tehditler;
            fikir.GuncellemeTarihi = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return "Basarili";
        }

        public async Task<string> SubmitForPreApprovalAsync(int id, string userId)
        {
            var fikir = await _context.Fikirler.FindAsync(id);
            if (fikir == null) return "Bulunamadi";

            if (fikir.BasvuruSahibiId != userId) return "Yetkisiz";

            if (fikir.Durum != FikirDurumu.Taslak)
                return "Fikir zaten ön onaya gönderilmiş veya farklı bir durumda.";

            fikir.Durum = FikirDurumu.OnOnayBekliyor;
            fikir.GuncellemeTarihi = DateTime.UtcNow;

            _context.FikirDurumGecmisleri.Add(new FikirDurumGecmisi
            {
                FikirId = fikir.Id,
                EskiDurum = FikirDurumu.Taslak,
                YeniDurum = FikirDurumu.OnOnayBekliyor,
                IslemYapanKullaniciId = userId,
                Aciklama = "Kullanıcı fikri ön onaya gönderdi.",
                IslemTarihi = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return "Basarili";
        }
    }
}

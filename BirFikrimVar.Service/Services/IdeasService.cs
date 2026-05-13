using BirFikrimVar.Core.Dtos.Admin;
using BirFikrimVar.Core.Dtos.Fikir;
using BirFikrimVar.Core.Entities;
using BirFikrimVar.Core.Enums;
using BirFikrimVar.Data.Context;
using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BirFikrimVar.Service.Services
{
    public class IdeasService : IIdeasService
    {
        private readonly AppDbContext _context;
        private readonly IStorageService _storageService;

        public IdeasService(AppDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
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
            var query = _context.Fikirler
                .Include(f => f.BasvuruSahibi)
                .Include(f => f.OnOnayDegerlendirmeleri)
                    .ThenInclude(od => od.Degerlendirici)
                .Include(f => f.KomisyonDegerlendirmeleri)
                    .ThenInclude(kd => kd.Degerlendirici)
                .AsQueryable();

            if (!userRoles.Contains("Admin") && !userRoles.Contains("OnOnayci") && !userRoles.Contains("KomisyonUyesi"))
            {
                query = query.Where(f => f.BasvuruSahibiId == userId);
            }

            var fikirler = await query.ToListAsync();

            return fikirler.Select(f => new FikirListeDto
            {
                Id = f.Id,
                Baslik = f.Baslik,
                Durum = f.Durum.ToString(),
                BasvuruSahibiAdSoyad = f.BasvuruSahibi.Ad + " " + f.BasvuruSahibi.Soyad,
                OlusturmaTarihi = f.OlusturmaTarihi,

              
                OnOnayPuanlari = f.OnOnayDegerlendirmeleri
                    .GroupBy(od => od.DegerlendiriciId)
                    .Select(g => {
                        var d = g.First().Degerlendirici;
                        return new PuanBilgiDto
                        {
                            Isim = d.Ad + " " + d.Soyad,
                            Puan = Math.Round(g.Average(x => x.Puan), 2),
                        
                            Rol = "Ön Onaycı"
                        };
                    }).ToList(),

                KomisyonPuanlari = f.KomisyonDegerlendirmeleri
                    .Select(kd => new PuanBilgiDto
                    {
                        Isim = kd.Degerlendirici.Ad + " " + kd.Degerlendirici.Soyad,
                        Puan = kd.Puan,
                        Rol = "Komisyon Üyesi"
                    }).ToList()
            });
        }

        public async Task<FikirDetayDto> GetIdeaDetailAsync(int id, string userId, IList<string> userRoles)
        {

            var fikir = await _context.Fikirler
                .Include(f => f.BasvuruSahibi)
                .Include(f => f.FikirDosyalari)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fikir == null) return null;


            if (!userRoles.Contains("Admin") && !userRoles.Contains("OnOnayci") && !userRoles.Contains("KomisyonUyesi"))
            {
                if (fikir.BasvuruSahibiId != userId) return null;
            }

            var puanlar = await _context.Set<OnOnayDegerlendirmesi>()
                .Where(d => d.FikirId == id && d.DegerlendiriciId == userId)
                .Select(d => new OnOnayPuanDetayDto
                {
                    KategoriId = d.KategoriId,
                    Puan = d.Puan
                }).ToListAsync();

            var komisyonPuani = await _context.Set<KomisyonDegerlendirmesi>()
    .FirstOrDefaultAsync(d => d.FikirId == id && d.DegerlendiriciId == userId);

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
                OlusturmaTarihi = fikir.OlusturmaTarihi,
                Dosyalar = fikir.FikirDosyalari.Select(d => new FikirDosyaDto
                {
                    Id = d.Id,
                    DosyaAdi = d.OrijinalDosyaAdi
                }).ToList(),

                MevcutOnOnayPuanlari = puanlar,
                MevcutKomisyonPuani = komisyonPuani?.Puan
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


        public async Task<string> SubmitPreApprovalEvaluationAsync(int fikirId, OnOnayPuanEkleDto model, string userId)
        {
            var fikir = await _context.Fikirler.FindAsync(fikirId);
            if (fikir == null) return "Bulunamadi";


            if (fikir.Durum != FikirDurumu.OnOnayBekliyor)
                return "Fikir şu an ön onay değerlendirmesine açık değil.";

            var mevcutMu = await _context.Set<OnOnayDegerlendirmesi>()
                .AnyAsync(d => d.FikirId == fikirId && d.DegerlendiriciId == userId);

            if (mevcutMu)
                return "Zaten puan verdiniz. Puanınızı değiştirmek için güncelleme işlemini kullanın.";

            double toplamPuan = 0;
            foreach (var item in model.KategoriPuanlari)
            {
                int gecerliPuan = (int)item.Puan; 
                if (gecerliPuan < 1) gecerliPuan = 1;
                if (gecerliPuan > 10) gecerliPuan = 10;

                _context.Set<OnOnayDegerlendirmesi>().Add(new OnOnayDegerlendirmesi
                {
                    FikirId = fikirId,
                    KategoriId = item.KategoriId,
                    DegerlendiriciId = userId,
                    Puan = gecerliPuan 
                });
                toplamPuan += gecerliPuan;
            }

            double ortalama = toplamPuan / model.KategoriPuanlari.Count;
            var eskiDurum = fikir.Durum;

            if (ortalama >= 6)
            {
                fikir.Durum = FikirDurumu.KomisyonOnayiBekliyor;
            }
            else
            {
                fikir.Durum = FikirDurumu.OnOnayRetli;
            }

            fikir.GuncellemeTarihi = DateTime.UtcNow;

            // 3. Durum Geçmişini Logla (Audit Log)
            _context.FikirDurumGecmisleri.Add(new FikirDurumGecmisi
            {
                FikirId = fikir.Id,
                EskiDurum = eskiDurum,
                YeniDurum = fikir.Durum,
                IslemYapanKullaniciId = userId,
                IslemTarihi = DateTime.UtcNow,
                Aciklama = $"Ön Onay Puanlaması yapıldı. Ortalama: {ortalama:F2}. Sonuç: {(ortalama > 6 ? "Onaylandı" : "Reddedildi")}"
            });

            await _context.SaveChangesAsync();
            return "Basarili";
        }

        public async Task<string> UpdatePreApprovalEvaluationAsync(int fikirId, OnOnayPuanEkleDto model, string userId)
        {
            var fikir = await _context.Fikirler.FindAsync(fikirId);
            if (fikir == null) return "Bulunamadi";


            var komisyonIslemiVarMi = await _context.Set<KomisyonDegerlendirmesi>()
                .AnyAsync(d => d.FikirId == fikirId);

            if (komisyonIslemiVarMi)
            {
                return "Bir komisyon üyesi değerlendirme yaptığı için ön onay puanlaması artık güncellenemez.";
            }

            // 2. Mevcut ön onay puanlarını bul
            var mevcutDegerlendirmeler = await _context.Set<OnOnayDegerlendirmesi>()
                .Where(d => d.FikirId == fikirId && d.DegerlendiriciId == userId)
                .ToListAsync();

            if (!mevcutDegerlendirmeler.Any())
                return "Henüz bir değerlendirmeniz bulunmuyor.";

            // 3. Eski puanları sil ve yenilerini ekle
            _context.Set<OnOnayDegerlendirmesi>().RemoveRange(mevcutDegerlendirmeler);

            double toplamPuan = 0;
            foreach (var item in model.KategoriPuanlari)
            {
                _context.Set<OnOnayDegerlendirmesi>().Add(new OnOnayDegerlendirmesi
                {
                    FikirId = fikirId,
                    KategoriId = item.KategoriId,
                    DegerlendiriciId = userId,
                    Puan = item.Puan
                });
                toplamPuan += item.Puan;
            }

            // 4. Durumu tekrar hesapla (Puan değiştiği için durum Ret'ten Onay'a dönebilir veya tersi)
            double ortalama = toplamPuan / model.KategoriPuanlari.Count;
            var eskiDurum = fikir.Durum;

            if (ortalama > 6) fikir.Durum = FikirDurumu.KomisyonOnayiBekliyor;
            else fikir.Durum = FikirDurumu.OnOnayRetli;

            fikir.GuncellemeTarihi = DateTime.UtcNow;

            _context.FikirDurumGecmisleri.Add(new FikirDurumGecmisi
            {
                FikirId = fikir.Id,
                EskiDurum = eskiDurum,
                YeniDurum = fikir.Durum,
                IslemYapanKullaniciId = userId,
                IslemTarihi = DateTime.UtcNow,
                Aciklama = $"Ön Onay Puanlaması güncellendi (Komisyon öncesi). Yeni Ortalama: {ortalama:F2}"
            });

            await _context.SaveChangesAsync();
            return "Basarili";
        }



        public async Task<string> SubmitCommissionEvaluationAsync(int fikirId, KomisyonPuanEkleDto model, string userId)
        {
            var fikir = await _context.Fikirler.FindAsync(fikirId);
            if (fikir == null) return "Bulunamadi";

            if (model.Puan < 1 || model.Puan > 10)
                return "Puan sadece 1 ile 10 arasında olabilir.";


            if (fikir.Durum != FikirDurumu.KomisyonOnayiBekliyor)
                return "Fikir şu an komisyon değerlendirmesine açık değil.";

   
            var mevcutMu = await _context.Set<KomisyonDegerlendirmesi>()
                .AnyAsync(d => d.FikirId == fikirId && d.DegerlendiriciId == userId);

            if (mevcutMu) return "Bu fikre zaten puan verdiniz. Güncellemek için PUT kullanın.";

            var toplamPuanSayisi = await _context.Set<KomisyonDegerlendirmesi>().CountAsync(d => d.FikirId == fikirId);
            if (toplamPuanSayisi >= 3) return "Bu fikir için 3 kişilik komisyon kotası dolmuştur.";

         
            _context.Set<KomisyonDegerlendirmesi>().Add(new KomisyonDegerlendirmesi
            {
                FikirId = fikirId,
                DegerlendiriciId = userId,
                Puan = model.Puan,
                DegerlendirmeTarihi = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var komisyonPuanlari = await _context.Set<KomisyonDegerlendirmesi>().Where(d => d.FikirId == fikirId).ToListAsync();
            if (komisyonPuanlari.Count == 3)
            {
                double ortalama = komisyonPuanlari.Average(d => d.Puan);
                fikir.Durum = ortalama >= 8 ? FikirDurumu.KomisyonOnayli : FikirDurumu.KomisyonOnayiRetli;
                fikir.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return "KomisyonTamamlandi";
            }

            return "Basarili";
        }

        public async Task<string> UpdateCommissionEvaluationAsync(int fikirId, KomisyonPuanEkleDto model, string userId)
        {
            if (model.Puan < 1 || model.Puan > 10)
                return "Puan sadece 1 ile 10 arasında olabilir.";

            var fikir = await _context.Fikirler.FindAsync(fikirId);
            if (fikir == null) return "Bulunamadi";

            var mevcutPuan = await _context.Set<KomisyonDegerlendirmesi>()
                .FirstOrDefaultAsync(d => d.FikirId == fikirId && d.DegerlendiriciId == userId);

            if (mevcutPuan == null) return "Henüz puan vermediniz. Önce POST işlemi yapmalısınız.";

        
            if (fikir.Durum == FikirDurumu.KomisyonOnayli || fikir.Durum == FikirDurumu.KomisyonOnayiRetli)
                return "Komisyon değerlendirmesi tamamlanmış ve nihai karar verilmiş. Puan güncellenemez.";

            mevcutPuan.Puan = model.Puan;
            mevcutPuan.DegerlendirmeTarihi = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return "Basarili";
        }


        public async Task<string> UploadIdeaAttachmentsAsync(int fikirId, List<IFormFile> files, string userId)
        {
            var fikir = await _context.Fikirler.FindAsync(fikirId);
            if (fikir == null) return "Bulunamadi";

            if (fikir.BasvuruSahibiId != userId) return "Yetkisiz";

            if (fikir.Durum != FikirDurumu.Taslak)
                return "Sadece taslak durumundaki fikirlere dosya eklenebilir.";

            var izinVerilenUzantilar = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
            long maxBoyut = 5 * 1024 * 1024; // 5 MB

            foreach (var file in files)
            {
                var uzanti = Path.GetExtension(file.FileName).ToLowerInvariant();

                //döküman kuralları
                if (!izinVerilenUzantilar.Contains(uzanti)) return $"Geçersiz dosya formatı: {file.FileName}";
                if (file.Length > maxBoyut) return $"Dosya boyutu çok büyük (Max 5MB): {file.FileName}";

                var kayitliYol = await _storageService.UploadFileAsync(file, $"Fikirler/{fikirId}");

                _context.Set<FikirDosyasi>().Add(new FikirDosyasi
                {
                    FikirId = fikirId,
                    OrijinalDosyaAdi = file.FileName,
                    KayitliDosyaYolu = kayitliYol,
                    Uzanti = uzanti,
                    DosyaBoyutu = file.Length
                });
            }

            await _context.SaveChangesAsync();
            return "Basarili";
        }

        public async Task<(string filePath, string fileName, string contentType)> GetAttachmentForDownloadAsync(int fikirId, int fileId, string userId, IList<string> userRoles)
        {
            var fikir = await _context.Fikirler
                .Include(f => f.FikirDosyalari) 
                .FirstOrDefaultAsync(f => f.Id == fikirId);

            if (fikir == null) return (null, null, null);

           
            if (!userRoles.Contains("Admin") && !userRoles.Contains("OnOnayci") && !userRoles.Contains("KomisyonUyesi"))
            {
                if (fikir.BasvuruSahibiId != userId) return (null, null, null); // Yetkisiz
            }

        
            var dosya = await _context.Set<FikirDosyasi>().FirstOrDefaultAsync(d => d.Id == fileId && d.FikirId == fikirId);

            if (dosya == null) return (null, null, null);

            
            var tamFizikselYol = Path.Combine(Directory.GetCurrentDirectory(), dosya.KayitliDosyaYolu);

      
            string contentType = GetContentType(dosya.Uzanti);

            return (tamFizikselYol, dosya.OrijinalDosyaAdi, contentType);
        }

        public async Task<IEnumerable<KategoriDto>> GetActiveCategoriesAsync()
        {
            return await _context.Kategoriler
                .Where(k => k.AktifMi == true)
                .Select(k => new KategoriDto
                {
                    Id = k.Id,
                    Ad = k.Ad,
                    AktifMi = k.AktifMi
                })
                .ToListAsync();
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
    {
        {".txt", "text/plain"},
        {".pdf", "application/pdf"},
        {".doc", "application/vnd.ms-word"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".png", "image/png"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"}
    };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }




        public async Task<string> DeleteIdeaAttachmentAsync(int fikirId, int fileId, string userId)
        {
            var fikir = await _context.Fikirler.FindAsync(fikirId);
            if (fikir == null) return "Bulunamadi";
            if (fikir.BasvuruSahibiId != userId) return "Yetkisiz";
            if (fikir.Durum != FikirDurumu.Taslak) return "Sadece taslak durumundaki fikirlerin dosyaları silinebilir.";

            var dosya = await _context.Set<FikirDosyasi>().FirstOrDefaultAsync(d => d.Id == fileId && d.FikirId == fikirId);
            if (dosya == null) return "DosyaBulunamadi";

          
            _storageService.DeleteFile(dosya.KayitliDosyaYolu);

       
            _context.Set<FikirDosyasi>().Remove(dosya);
            await _context.SaveChangesAsync();

            return "Basarili";
        }



    }


}

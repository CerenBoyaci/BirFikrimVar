using BirFikrimVar.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BirFikrimVar.Data.Context
{
    public class AppDbContext : IdentityDbContext<Kullanici, Microsoft.AspNetCore.Identity.IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Fikir> Fikirler { get; set; }
        public DbSet<FikirDurumGecmisi> FikirDurumGecmisleri { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }

      
        public DbSet<FikirDosyasi> FikirDosyalari { get; set; }

       
        public DbSet<OnOnayDegerlendirmesi> OnOnayDegerlendirmeleri { get; set; }
        public DbSet<KomisyonDegerlendirmesi> KomisyonDegerlendirmeleri { get; set; }

      
        public DbSet<Degerlendirme> Degerlendirmeler { get; set; }
        public DbSet<AdminLog> AdminLoglari { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Kullanici>().ToTable("Kullanicilar");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roller");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("KullaniciRolleri");

            builder.Entity<Fikir>()
                .HasOne(f => f.BasvuruSahibi)
                .WithMany(k => k.Fikirler)
                .HasForeignKey(f => f.BasvuruSahibiId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- BURASI DEĞİŞTİ (İsim çakışmasını engellemek için) ---
            builder.Entity<Degerlendirme>()
                .HasOne(d => d.Degerlendirici)
                .WithMany() // Kullanici tarafında koleksiyon yoksa boş bırakabilirsin
                .HasForeignKey(d => d.DegerlendiriciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FikirDosyasi>()
                .HasOne(d => d.Fikir)
                .WithMany(f => f.FikirDosyalari)
                .HasForeignKey(d => d.FikirId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- BURASI GÜNCELLENDİ: Degerlendirmeler yerine OnOnayDegerlendirmeleri ---
            // Eğer OnOnayDegerlendirmesi kullanıyorsan:
            builder.Entity<OnOnayDegerlendirmesi>()
                .HasOne(d => d.Fikir)
                .WithMany(f => f.OnOnayDegerlendirmeleri)
                .HasForeignKey(d => d.FikirId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- BURASI GÜNCELLENDİ: DurumGecmisleri yerine FikirDurumGecmisleri ---
            builder.Entity<FikirDurumGecmisi>()
                .HasOne(fd => fd.Fikir)
                .WithMany(f => f.FikirDurumGecmisleri) // Fikir.cs içindeki yeni isim bu
                .HasForeignKey(fd => fd.FikirId)
                .OnDelete(DeleteBehavior.Cascade);

            // Komisyon için de ekleyelim:
            builder.Entity<KomisyonDegerlendirmesi>()
                .HasOne(d => d.Fikir)
                .WithMany(f => f.KomisyonDegerlendirmeleri)
                .HasForeignKey(d => d.FikirId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Kategori>().HasData(
                new Kategori { Id = 1, Ad = "Stratejik Uyum", AktifMi = true },
                new Kategori { Id = 2, Ad = "Yenilikçilik", AktifMi = true },
                new Kategori { Id = 3, Ad = "Uygulanabilirlik", AktifMi = true },
                new Kategori { Id = 4, Ad = "Beklenen Fayda", AktifMi = true },
                new Kategori { Id = 5, Ad = "Risk ve Sürdürülebilirlik", AktifMi = true },
                new Kategori { Id = 6, Ad = "Kaynak İhtiyacı", AktifMi = true }
            );
        }
    }
}
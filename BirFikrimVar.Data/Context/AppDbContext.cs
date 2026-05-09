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

        // GÜNCELLEME 1: DosyaEki tamamen kaldırıldı, sadece FikirDosyalari var.
        public DbSet<FikirDosyasi> FikirDosyalari { get; set; }

        // GÜNCELLEME 2: Service katmanında kullandığın yeni değerlendirme tabloları eklendi.
        public DbSet<OnOnayDegerlendirmesi> OnOnayDegerlendirmeleri { get; set; }
        public DbSet<KomisyonDegerlendirmesi> KomisyonDegerlendirmeleri { get; set; }

        // (Eski tip Degerlendirme tablosunu sistemden tamamen silmediysen diye burada tutuyorum)
        public DbSet<Degerlendirme> Degerlendirmeler { get; set; }

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

            builder.Entity<Degerlendirme>()
                .HasOne(d => d.Degerlendirici)
                .WithMany(k => k.Degerlendirmeler)
                .HasForeignKey(d => d.DegerlendiriciId)
                .OnDelete(DeleteBehavior.Restrict);

            // GÜNCELLEME 3: İlişki DosyaEki yerine FikirDosyasi üzerinden kuruldu.
            // (Fikir silinirse ekleri de silinmeli)
            builder.Entity<FikirDosyasi>()
                .HasOne(d => d.Fikir)
                .WithMany(f => f.FikirDosyalari)
                .HasForeignKey(d => d.FikirId)
                .OnDelete(DeleteBehavior.Cascade);

            // fikir ve Değerlendirmeler ilişkisi (fikir silinirse değerlendirmeleri de silinmeli)
            builder.Entity<Degerlendirme>()
                .HasOne(d => d.Fikir)
                .WithMany(f => f.Degerlendirmeler)
                .HasForeignKey(d => d.FikirId)
                .OnDelete(DeleteBehavior.Cascade);

            // fikir ve durum geçmişi ilişkisi (fikir silinirse durum geçmişleri de silinmeli)
            builder.Entity<FikirDurumGecmisi>()
                .HasOne(fd => fd.Fikir)
                .WithMany(f => f.DurumGecmisleri)
                .HasForeignKey(fd => fd.FikirId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
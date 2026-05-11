using BirFikrimVar.Core.Entities;
using BirFikrimVar.Data.Context;
using BirFikrimVar.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BirFikrimVar.Service.Services
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _context;

        public LogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAdminActionAsync(string adminId, string islemTipi, string aciklama)
        {
            var log = new AdminLog
            {
                AdminId = adminId,
                IslemTipi = islemTipi,
                Aciklama = aciklama,
                IslemTarihi = DateTime.UtcNow
            };

            _context.AdminLoglari.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<object>> GetAdminLogsAsync()
        {
            return await _context.AdminLoglari
                .Include(l => l.Admin)
                .OrderByDescending(l => l.IslemTarihi)
                .Select(l => new
                {
                    l.Id,
                    AdminAdSoyad = l.Admin != null ? l.Admin.Ad + " " + l.Admin.Soyad : "Bilinmeyen",
                    l.IslemTipi,
                    l.Aciklama,
                    l.IslemTarihi
                })
                .ToListAsync();
        }
    }
}

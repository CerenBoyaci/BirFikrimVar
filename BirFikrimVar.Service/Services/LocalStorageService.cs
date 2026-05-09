using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BirFikrimVar.Service.Services
{
    public class LocalStorageService : IStorageService
    {
        public async Task<string> UploadFileAsync(IFormFile file, string subDirectory)
        {
            if (file == null || file.Length == 0) return string.Empty;

            // DİKKAT: wwwroot yerine ana dizinde SecureUploads adında private bir klasör kullanıyoruz.
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads", subDirectory);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Veritabanına kaydetmek üzere, uygulamanın kök dizinine göre göreceli (relative) bir yol dönüyoruz.
            return Path.Combine("SecureUploads", subDirectory, uniqueFileName).Replace("\\", "/");
        }

        public void DeleteFile(string filePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
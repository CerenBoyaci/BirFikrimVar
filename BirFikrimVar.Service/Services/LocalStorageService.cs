using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BirFikrimVar.Service.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _basePath = "SecureUploads";

        public async Task<string> UploadFileAsync(IFormFile file, string subDirectory)
        {
            if (file == null || file.Length == 0) return string.Empty;


            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), _basePath, subDirectory);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

         
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

      
            return Path.Combine(_basePath, subDirectory, uniqueFileName).Replace("\\", "/");
        }

        public async Task<(Stream stream, string contentType)> GetFileStreamAsync(string filePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

            if (!File.Exists(fullPath))
                return (null, null);

       
            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;

        
            var contentType = GetContentType(filePath);

            return (memory, contentType);
        }

        public void DeleteFile(string filePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
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
    }
}
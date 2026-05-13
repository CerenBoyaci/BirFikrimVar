using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace BirFikrimVar.Service.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string subDirectory);
        Task<(Stream stream, string contentType)> GetFileStreamAsync(string filePath);
        void DeleteFile(string filePath);

    }
}

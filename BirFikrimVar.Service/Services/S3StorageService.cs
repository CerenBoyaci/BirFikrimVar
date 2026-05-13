using Amazon.S3;
using Amazon.S3.Transfer;
using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BirFikrimVar.Service.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private const string BucketName = "birfikrimvar-uploads";

        public S3StorageService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subDirectory)
        {
            var fileKey = $"{subDirectory}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}".Replace("\\", "/");

            using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileKey,
                BucketName = BucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return fileKey;
        }

        public async Task<(Stream stream, string contentType)> GetFileStreamAsync(string filePath)
        {
            var response = await _s3Client.GetObjectAsync(BucketName, filePath);
            return (response.ResponseStream, response.Headers.ContentType);
        }

        public void DeleteFile(string filePath)
        {
            _s3Client.DeleteObjectAsync(BucketName, filePath).Wait();
        }
    }
}

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BirFikrimVar.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BirFikrimVar.Service.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["Minio:BucketName"] ?? "birfikrimvar-uploads";
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subDirectory)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            await EnsureBucketExistsAsync();

            var fileKey = $"{subDirectory}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"
                .Replace("\\", "/");

            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileKey,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return fileKey;
        }

        public async Task<(Stream stream, string contentType)> GetFileStreamAsync(string filePath)
        {
            var response = await _s3Client.GetObjectAsync(_bucketName, filePath);
            return (response.ResponseStream, response.Headers.ContentType);
        }

        public void DeleteFile(string filePath)
        {
            _s3Client.DeleteObjectAsync(_bucketName, filePath).Wait();
        }

        private async Task EnsureBucketExistsAsync()
        {
            var bucketsResponse = await _s3Client.ListBucketsAsync();

            var buckets = bucketsResponse.Buckets ?? new List<S3Bucket>();

            var exists = buckets.Any(b => b.BucketName == _bucketName);

            if (!exists)
            {
                try
                {
                    await _s3Client.PutBucketAsync(new PutBucketRequest
                    {
                        BucketName = _bucketName
                    });
                }
                catch (AmazonS3Exception ex) when (
                    ex.ErrorCode == "BucketAlreadyOwnedByYou" ||
                    ex.ErrorCode == "BucketAlreadyExists")
                {
                    // Bucket zaten varsa sorun değil.
                }
            }
        }
    }
}
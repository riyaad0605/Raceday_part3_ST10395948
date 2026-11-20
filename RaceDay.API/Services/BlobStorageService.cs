using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    /// <summary>
    /// Handles uploading and deleting files in Azure Blob Storage.
    /// </summary>
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName = "images");
        Task DeleteFileAsync(string fileUrl, string containerName = "images");
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName = "images")
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Generate unique blob name to avoid collisions
            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl, string containerName = "images")
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}

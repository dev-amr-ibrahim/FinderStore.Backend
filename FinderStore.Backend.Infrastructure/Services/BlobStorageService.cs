using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FinderStore.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FinderStore.Backend.Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var accountName = configuration["AzureStorage:AccountName"];
            var containerName = configuration["AzureStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new InvalidOperationException(
                    "AzureStorage:AccountName is not configured.");
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "AzureStorage:ContainerName is not configured.");
            }

            var serviceUri =
            new Uri($"https://{accountName}.blob.core.windows.net");

            var credential = new DefaultAzureCredential();

            var blobServiceClient =
                new BlobServiceClient(serviceUri, credential);

            _containerClient =
                blobServiceClient.GetBlobContainerClient(containerName);

        }
        public async Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException(
                    "The provided stream is not readable.",
                    nameof(stream));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException(
                    "File name is required.",
                    nameof(fileName));

            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/octet-stream";

            await _containerClient.CreateIfNotExistsAsync(
                cancellationToken: cancellationToken);

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(
                stream,
                uploadOptions,
                cancellationToken);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots,
               cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            var response = await blobClient.ExistsAsync(
                cancellationToken);

            return response.Value;
        }

    }
}

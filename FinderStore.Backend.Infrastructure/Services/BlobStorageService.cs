using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using FinderStore.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FinderStore.Backend.Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private BlobContainerClient _containerClient;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            var accountName = configuration["AzureStorage:AccountName"];
            var containerName = configuration["AzureStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new InvalidOperationException(
                    "AzureStorage:AccountName is not configured.");
            }

            //if (string.IsNullOrWhiteSpace(containerName))
            //{
            //    throw new InvalidOperationException(
            //        "AzureStorage:ContainerName is not configured.");
            //}

            //var serviceUri = new Uri($"https://{accountName}.blob.core.windows.net");

            //var credential = new DefaultAzureCredential();

            _blobServiceClient = blobServiceClient;

            //_containerClient =
            //    blobServiceClient.GetBlobContainerClient(containerName);
        }

        public string? GetPublicUrl(string? fileName, string containerName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return fileName;

            if (Uri.TryCreate(fileName, UriKind.Absolute, out _))
                return fileName;

            return _blobServiceClient
                .GetBlobContainerClient(containerName)
                .GetBlobClient(fileName)
                .Uri
                .ToString();
        }

        public async Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            string containerName,
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

            _containerClient =
               _blobServiceClient.GetBlobContainerClient(containerName);

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
            string containerName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName) || 
                string.IsNullOrWhiteSpace(containerName))
                return;

            var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient =
                containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots,
               cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            string fileName,
            string containerName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(containerName))
                return false;

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient =
                containerClient.GetBlobClient(fileName);

            var response = await blobClient.ExistsAsync(
                cancellationToken);

            return response.Value;
        }

        public async Task<string> GenerateReadUrlAsync(
            string fileName,
            TimeSpan expiresIn,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException(
                    "File name is required.",
                    nameof(fileName));

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync(cancellationToken))
                throw new FileNotFoundException(
                    $"Blob '{fileName}' was not found.");

            var startsOn = DateTimeOffset.UtcNow.AddMinutes(-1);

            var expiresOn = DateTimeOffset.UtcNow.Add(expiresIn);

            var userDelegationKey =
                await _blobServiceClient
                    .GetUserDelegationKeyAsync(
                        startsOn,
                        expiresOn,
                        cancellationToken);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = fileName,
                Resource = "b",
                StartsOn = startsOn,
                ExpiresOn = expiresOn
            };

            sasBuilder.SetPermissions(
                BlobSasPermissions.Read);

            var sas =
                sasBuilder.ToSasQueryParameters(
                    userDelegationKey.Value,
                    _blobServiceClient.AccountName);

            return $"{blobClient.Uri}?{sas}";
        }
    }
}

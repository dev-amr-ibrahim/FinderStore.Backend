using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.Common.Interfaces
{
    public interface IBlobStorageService
    {
        string? GetPublicUrl(string? fileName, string containerName);

        Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string containerName,
        CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string fileName,
            string containerName,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
        string fileName,
        string containerName,
        CancellationToken cancellationToken = default);

        Task<string> GenerateReadUrlAsync(
            string fileName,
            TimeSpan expiresIn,
            CancellationToken cancellationToken = default);
    }
}

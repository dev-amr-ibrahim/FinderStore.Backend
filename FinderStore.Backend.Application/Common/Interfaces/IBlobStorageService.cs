using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.Common.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string fileName,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
        string fileName,
        CancellationToken cancellationToken = default);
    }
}

using DentalERP.SharedKernel.Interfaces;

namespace DentalERP.SharedKernel.Infrastructure;

public sealed class NullFileStorageService : IFileStorageService
{
    public Task<string> UploadAsync(string bucket, string key, Stream data, string contentType, CancellationToken ct = default)
        => Task.FromResult(key);

    public Task<string> GetPresignedUrlAsync(string bucket, string key, int expirySeconds = 3600, CancellationToken ct = default)
        => Task.FromResult(string.Empty);

    public Task DeleteAsync(string bucket, string key, CancellationToken ct = default)
        => Task.CompletedTask;
}

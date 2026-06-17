namespace DentalERP.SharedKernel.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(string bucket, string key, Stream data, string contentType, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string bucket, string key, int expirySeconds = 3600, CancellationToken ct = default);
    Task DeleteAsync(string bucket, string key, CancellationToken ct = default);
}

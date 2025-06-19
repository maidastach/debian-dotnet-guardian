
using Guardian.Domain.Abstractions;
using Guardian.Domain.Dto;

namespace Guardian.Infrastructure.Interfaces
{
    public interface IDriveProviderService
    {
        Task<Result<CleanFilesDto>> CleanFilesAsync(CancellationToken cancellationToken);
        Task<Result<string>> UploadFileAsync(FileInfo record, string mimeType, CancellationToken cancellationToken);
    }
}
using Guardian.Domain.Abstractions;
using Guardian.Domain.Dto;
using Guardian.Domain.Entities.Records;

namespace Guardian.Application.Interfaces
{
    public interface IUploaderService
    {
        Task<Result<IEnumerable<UploadFilesDto>>> UploadMissingFilesAsync(CancellationToken cancellationToken);
        Task<Result<string>> UploadFileAsync(FileInfo fileInfo, CancellationToken cancellationToken);
        Task<Result<CleanFilesDto>> CleanFilesAsync(CancellationToken cancellationToken);
        Task MarkFileAsUploadedAsync(Record record, string fileIdOnDrive, CancellationToken cancellationToken);
        Task MarkFileAsDeletedAsync(Record record, Exception ex, CancellationToken cancellationToken);
        Task SaveToLogAsync(Record record, Exception ex, CancellationToken cancellationToken);
    }
}
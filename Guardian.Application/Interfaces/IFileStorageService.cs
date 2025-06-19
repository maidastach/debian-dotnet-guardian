using Guardian.Domain.Entities.Records;

namespace Guardian.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task ProcessFileAsync(FileInfo fileInfo, CancellationToken cancellationToken);
    }
}
using Guardian.Application.Interfaces;
using Guardian.Domain.DependencyInjection;
using Guardian.Domain.Entities.Records;
using Guardian.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Guardian.Application.Services
{
    [ServiceLifetime(ServiceLifetime.Singleton)]
    public sealed class FileStorageService(IScopeFactoryService scopeFactoryService, ILogger<FileStorageService> logger) : IFileStorageService
    {
        private readonly IScopeFactoryService _scopeFactoryService = scopeFactoryService;
        private readonly ILogger<FileStorageService> _logger = logger;

        public async Task ProcessFileAsync(FileInfo fileInfo, CancellationToken cancellationToken)
        {
            _logger.LogInformation("File {fileName} is stable and ready for processing", fileInfo.FullName);
            var record = await CreateRecordAsync(fileInfo, cancellationToken);
            if (record is null)
            {
                return;
            }
            await UploadRecordAsync(record, fileInfo, cancellationToken);
        }

        private async Task<Record?> CreateRecordAsync(FileInfo fileInfo, CancellationToken cancellationToken)
        {
            var newFile = new Record(fileInfo.Name, fileInfo.FullName);
            try
            {
                return await _scopeFactoryService.ExecuteInScopeAsync<IRepository<Record>, Record>(async service =>
                {
                    return await service.CreateAsync(newFile, cancellationToken);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {fileName} into Db", fileInfo.FullName);
                await _scopeFactoryService.ExecuteInScopeAsync<IUploaderService, Task>(async service =>
                {
                    await service.SaveToLogAsync(newFile, ex, cancellationToken);
                    return Task.CompletedTask;
                });
                return null;
            }
        }

        private async Task UploadRecordAsync(Record record, FileInfo fileInfo, CancellationToken cancellationToken)
        {
            await _scopeFactoryService.ExecuteInScopeAsync<IUploaderService, Task>(async service =>
            {
                try
                {
                    var uploaderResult = await service.UploadFileAsync(fileInfo, cancellationToken);

                    if (uploaderResult.IsSuccess)
                    {
                        await service.MarkFileAsUploadedAsync(record, uploaderResult.Data!, cancellationToken);
                        return Task.CompletedTask;
                    }

                    if (uploaderResult.Error.Data is FileNotFoundException) // TODO Error or Error.Data ?
                    {
                        await service.MarkFileAsDeletedAsync(record, uploaderResult.Error.Data as FileNotFoundException, cancellationToken);
                        return Task.CompletedTask;
                    }

                    await service.SaveToLogAsync(record, uploaderResult.Error.Data as Exception, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName} to Drive", record.FileName);
                    await service.SaveToLogAsync(record, ex, cancellationToken);
                }
                return Task.CompletedTask;
            });
        }
    }
}
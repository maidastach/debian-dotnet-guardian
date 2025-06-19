using System.Diagnostics;
using Guardian.Application.Interfaces;
using Guardian.Domain.Abstractions;
using Guardian.Domain.Configs;
using Guardian.Domain.DependencyInjection;
using Guardian.Domain.Dto;
using Guardian.Domain.Entities.Records;
using Guardian.Domain.Errors;
using Guardian.Infrastructure.Interfaces;
using Guardian.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guardian.Application.Services
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    public class UploaderService(IRepository<Record> repository, IDriveProviderService driveService, IScopeFactoryService scopeFactoryService, IOptions<GuardianConfig> configs, ILogger<UploaderService> logger) : IUploaderService
    {
        private readonly IRepository<Record> _repository = repository;
        private readonly IDriveProviderService _driveService = driveService;
        private readonly ILogger<UploaderService> _logger = logger;
        private readonly IScopeFactoryService _scopeFactoryService = scopeFactoryService;

        public async Task<Result<IEnumerable<UploadFilesDto>>> UploadMissingFilesAsync(CancellationToken cancellationToken)
        {
            var recordsToUpload = await GetFilesToUploadAsync(cancellationToken);

            if (!recordsToUpload.IsSuccess)
            {
                return Result<IEnumerable<UploadFilesDto>>.Failure(GuardianErrors.HandleException(recordsToUpload.Error.Data as Exception));
            }

            var totalRecords = recordsToUpload.Data!.Count();
            int currentRecord = 1;
            IEnumerable<UploadFilesDto> result = [];
            foreach (var record in recordsToUpload.Data!)
            {
                try
                {
                    _logger.LogInformation("Uploading {CurrentRecordCount} of {TotalRecordsCount}, {FileName}", currentRecord, totalRecords, record.FileName);
                    var fileInfo = new FileInfo(record.FullPath);
                    var upload = await UploadFileAsync(fileInfo, cancellationToken);

                    if (upload.IsSuccess)
                    {
                        await MarkFileAsUploadedAsync(record, upload.Data!, cancellationToken);
                        continue;
                    }

                    if (upload.Error.Data is FileNotFoundException) // TODO Error or Error.Data ?
                    {
                        await MarkFileAsDeletedAsync(record, upload.Error.Data as FileNotFoundException, cancellationToken);
                        result = result.Append(new(record.FileName, upload.Error.Message, true));
                        continue;
                    }

                    await SaveToLogAsync(record, upload.Error.Data as Exception, cancellationToken);
                    result = result.Append(new(record.FileName, upload.Error.Message));
                    _logger.LogError(upload.Error.Data as Exception, "Error uploading file {FileName}. Error: {Error}", record.FileName, upload.Error.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName}. Error: {Error}", record.FileName, ex.Message);
                    result = result.Append(new(record.FileName, ex.Message));
                }
                finally
                {
                    currentRecord++;
                }
            }
            return Result<IEnumerable<UploadFilesDto>>.Success(result);
        }

        public async Task<Result<string>> UploadFileAsync(FileInfo fileInfo, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Uploading {fileName}...", fileInfo.Name);
            try
            {
                var mimeType = await GetMimeTypeAsync(fileInfo) ?? "video/x-msvideo";
                return await _driveService.UploadFileAsync(fileInfo, mimeType, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(GuardianErrors.HandleException(ex));
            }
        }

        public async Task<Result<CleanFilesDto>> CleanFilesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _driveService.CleanFilesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning files: {Error}", ex.Message);
                return Result<CleanFilesDto>.Failure(GuardianErrors.HandleException(ex));
            }
        }

        public async Task MarkFileAsUploadedAsync(Record record, string fileIdOnDrive, CancellationToken cancellationToken)
        {
            record.UploadCompleted(fileIdOnDrive);
            await _repository.UpdateAsync(record, cancellationToken);
            _logger.LogInformation("Successfully uploaded {FileName}. Id: {FileIdOnDrive}", record.FileName, fileIdOnDrive);

            File.Move(record.FullPath, $"{configs.Value.LocalDrivePaths.Uploaded}/{record.FileName}");
        }

        public async Task MarkFileAsDeletedAsync(Record record, Exception ex, CancellationToken cancellationToken)
        {
            record.AddLog(new RecordLog(ex.ToString(), true));
            record.Delete();
            await _repository.UpdateAsync(record, cancellationToken);
            _logger.LogWarning("File {fileName} does not exist. Record {recordName} deleted", record.FullPath, record.FileName);
        }

        public async Task SaveToLogAsync(Record record, Exception ex, CancellationToken cancellationToken)
        {
            record.AddLog(new RecordLog(ex.Message, true));
            await _repository.UpdateAsync(record, cancellationToken);
        }

        private async Task<Result<IEnumerable<Record>>> GetFilesToUploadAsync(CancellationToken cancellationToken)
        {
            try
            {
                var recordsToUpload = await _repository.GetAllAsync(x => !x.IsUploaded && !x.IsDeleted, cancellationToken);
                if (!recordsToUpload.Any())
                {
                    _logger.LogInformation("Nothing to upload");
                }
                return Result<IEnumerable<Record>>.Success(recordsToUpload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get records to upload");
                return Result<IEnumerable<Record>>.Failure(GuardianErrors.HandleException(ex));
            }
        }

        private async Task<string?> GetMimeTypeAsync(FileInfo fileInfo)
        {
            try
            {
                var process = _scopeFactoryService.ExecuteInScope<ICommandService, Result<Process>>(service =>
                {
                    return service.Execute($"file --mime-type -b {fileInfo.FullName}", dispose: false);
                });
                if (!process.IsSuccess)
                {
                    return null;
                }
                var mimeType = await process.Data!.StandardOutput.ReadToEndAsync();
                process.Data.Dispose();
                return mimeType?.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to get mimeType of {FileName}. {Error}", fileInfo.Name, ex.Message);
                return null;
            }
        }
    }
}
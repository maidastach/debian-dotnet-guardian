using Guardian.Domain.Entities.Records;
using Google.Apis.Drive.v3;
using static Google.Apis.Services.BaseClientService;
using Google.Apis.Upload;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Guardian.Domain.Configs;
using Guardian.Infrastructure.Interfaces;
using Guardian.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Guardian.Domain.Dto;
using Guardian.Domain.Abstractions;
using Google.Apis.Auth.OAuth2;
using Guardian.Domain.Errors;

namespace Guardian.Infrastructure.Services
{
    [ServiceLifetime(ServiceLifetime.Singleton)]
    public sealed class GoogleDriveService : IDriveProviderService
    {
        private readonly DriveService _driveService;
        private readonly ILogger<GoogleDriveService> _logger;
        private readonly GoogleDrive _googleDriveConfig;
        private string? _parentFolderId;

        public GoogleDriveService(IOptions<GuardianConfig> configs, ILogger<GoogleDriveService> logger)
        {
            _logger = logger;
            _googleDriveConfig = configs.Value.GoogleDrive;
            _driveService = Init();
            _logger.LogInformation("Connected to Google Drive");
        }

        private DriveService Init()
        {
            return new DriveService(new Initializer
            {
                ApplicationName = _googleDriveConfig.ApplicationName,
                HttpClientInitializer = GetCredential()
            });
        }

        private GoogleCredential GetCredential()
        {
            using var stream = new FileStream(_googleDriveConfig.Secret, FileMode.Open, FileAccess.Read);
            return GoogleCredential.FromStream(stream).CreateScoped([DriveService.Scope.Drive]);
        }

        public async Task<Result<string>> UploadFileAsync(FileInfo file, string mimeType, CancellationToken cancellationToken)
        {
            try
            {
                var fileMetaData = new Google.Apis.Drive.v3.Data.File
                {
                    Name = file.Name,
                    Parents = [await GetOrCreateFolderAsync(cancellationToken)],
                    MimeType = mimeType,
                };

                using var stream = new FileStream(file.FullName, FileMode.Open);
                var request = _driveService.Files.Create(fileMetaData, stream, mimeType);
                request.Fields = "*";
                var result = await request.UploadAsync(cancellationToken);

                if (result.Status == UploadStatus.Completed)
                {
                    return Result<string>.Success(request.ResponseBody?.Id);
                }

                if (result.Status == UploadStatus.Failed)
                {
                    _logger.LogError("Failed to upload {FileName}. Error: {errorMessage}", file.Name, result.Exception.Message);
                    return Result<string>.Failure(GuardianErrors.HandleException(result.Exception));
                }

                _logger.LogError("Failed to upload {FileName} - {Status)}. {Error}", file.Name, nameof(result.Status), result.Exception?.ToString());
                return Result<string>.Failure(GuardianErrors.HandleException(result?.Exception));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when trying to upload {fileName}", file.Name);
                return Result<string>.Failure(GuardianErrors.HandleException(ex));
            }
        }

        public async Task<Result<CleanFilesDto>> CleanFilesAsync(CancellationToken cancellationToken)
        {
            var files = await GetAllFiles(cancellationToken);
            CleanFilesDto result = new(files.Select(f => $"{f.Name} - {f.Id}"));

            var filteredFiles = files.Where(f => !IsMainFolder(f));
            _logger.LogWarning("Files to delete: {Count}", filteredFiles.Count());
            foreach (var file in filteredFiles)
            {
                foreach (var parentId in file.Parents)
                {
                    var parent = files.FirstOrDefault(f => f.Id == parentId);
                    if (parent is null)
                    {
                        var deleted = await _driveService.Files.Delete(file.Id).ExecuteAsync(cancellationToken);
                        result.Deleted = result.Deleted.Append($"{file.Name} - {file.Id}");
                        _logger.LogWarning("Deleted {FileName}", file.Name);
                        // TODO: set as deleted
                    }
                }
            }
            await _driveService.Files.EmptyTrash().ExecuteAsync(cancellationToken);
            result.Trashed = true;
            _logger.LogWarning("Cleaned Files");
            return Result<CleanFilesDto>.Success(result);
        }

        private async Task<IList<Google.Apis.Drive.v3.Data.File>> GetAllFiles(CancellationToken cancellationToken)
        {
            var query = _driveService.Files.List();
            query.Fields = "files(id, parents, name)";
            return (await query.ExecuteAsync(cancellationToken)).Files;
        }

        private async Task<Google.Apis.Drive.v3.Data.File> GetFileById(string fileId, CancellationToken cancellationToken, bool includeParents = false)
        {
            var file = _driveService.Files.Get(fileId);
            if (includeParents)
            {
                file.Fields = "parents, name";
            }

            return await file.ExecuteAsync(cancellationToken);
        }

        private bool IsMainFolder(Google.Apis.Drive.v3.Data.File file)
        {
            var folderName = _googleDriveConfig.FolderNameOnDrive;
            return file.Id == "" /*TODO: get main folder ID*/ && file.Name == folderName;
        }

        private async Task<string> CreateFolderAsync(string folderName, string mimeType, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Folder not found on Drive. Creating one...");
            var fileMetaData = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = mimeType
            };

            var folder = await _driveService.Files.Create(fileMetaData).ExecuteAsync(cancellationToken);
            return folder.Id;
        }

        private async Task<string> GetOrCreateFolderAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_parentFolderId))
            {
                _logger.LogInformation("Using local folderId: {folderId}.", _parentFolderId);
                return _parentFolderId;
            }
            _logger.LogInformation("Folder does not exist locally. Fetching it...");

            var folderName = _googleDriveConfig.FolderNameOnDrive;
            var folderMimeType = _googleDriveConfig.FolderMimeType;
            var request = _driveService.Files.List();
            request.Q = $"mimeType = '{folderMimeType}' and trashed = false and name = '{folderName}'";

            var result = await request.ExecuteAsync(cancellationToken);
            _parentFolderId = result.Files[0].Id ?? await CreateFolderAsync(folderName, folderMimeType, cancellationToken);

            _logger.LogInformation("Using remote folderId: {folderId}.", _parentFolderId);
            return _parentFolderId;
        }
    }
}

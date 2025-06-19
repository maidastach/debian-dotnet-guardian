using Guardian.Application.Interfaces;
using Guardian.Domain.Abstractions;
using Guardian.Domain.Configs;
using Guardian.Domain.DependencyInjection;
using Guardian.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guardian.Application.Services
{
    [ServiceLifetime(ServiceLifetime.Singleton)]
    public sealed class FileMonitorService(ILogger<FileMonitorService> logger, IScopeFactoryService scopeFactoryService, IOptions<GuardianConfig> configs) : IFileMonitorService
    {
        private readonly ILogger<FileMonitorService> _logger = logger;
        private readonly IScopeFactoryService _scopeFactoryService = scopeFactoryService;
        private IEnumerable<FileSystemWatcher> _fileWatchers = [];

        public Result<string> Watch(string? watchPath, CancellationToken cancellationToken)
        {
            watchPath ??= configs.Value.LocalDrivePaths.Monitor;
            if (_fileWatchers.Any(fw => fw.Path == watchPath))
            {
                return Result<string>.Success($"FileWatchers: {string.Join(", ", _fileWatchers.Select(fw => fw.Path))}");
            }

            try
            {
                FileSystemWatcher fileWatcher = new()
                {
                    Path = watchPath,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
                    Filter = "*.*",
                    EnableRaisingEvents = true
                };
                fileWatcher.Created += OnFileCreated;
                fileWatcher.Error += OnError;

                _fileWatchers = [.. _fileWatchers.Append(fileWatcher)];

                _logger.LogInformation("Watching folder {FolderName}", watchPath);
                return Result<string>.Success($"FileWatchers: {string.Join(", ", _fileWatchers.Select(fw => fw.Path))}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while watching {FolderName}", watchPath);
                return Result<string>.Failure(GuardianErrors.HandleException(ex));
            }
        }

        public Result<string> Stop(string? watchPath)
        {
            watchPath ??= configs.Value.LocalDrivePaths.Monitor;
            FileSystemWatcher? _fileWatcher = _fileWatchers.FirstOrDefault(fw => fw.Path == watchPath);
            string message;
            if (_fileWatcher is null)
            {
                message = _fileWatchers.Any()
                   ? $"Filewatcher already disposed. FileWatchers: {string.Join(", ", _fileWatchers.Select(fw => fw.Path))}"
                   : "Filewatcher already disposed. No active FileWatchers.";
                return Result<string>.Failure(GuardianErrors.InvalidAction(message, _fileWatchers));
            }
            _fileWatcher.Dispose();
            _fileWatchers = [.. _fileWatchers.Where(fw => fw.Path != watchPath)];
            message = _fileWatchers.Any()
                   ? $"{watchPath} disposed. FileWatchers: {string.Join(", ", _fileWatchers.Select(fw => fw.Path))}"
                   : $"{watchPath} disposed. No active FileWatchers.";
            return Result<string>.Success(message);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("New file created {fileName}", e.Name);
            Task.Run(async () =>
            {
                await _scopeFactoryService.ExecuteInScopeAsync<IFileStabilityCheckerService>(async service =>
                {
                    await service.CheckFileSizeStabilityAsync(e.FullPath);
                });
            });
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "Filewatcher error {Error}", e.GetException().Message);
        }
    }
}
using Guardian.Application.Interfaces;
using Guardian.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Guardian.Application.Services
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    public sealed class FileStabilityCheckerService(IFileStorageService fileStorageService, ILogger<FileStabilityCheckerService> logger) : IFileStabilityCheckerService
    {
        private readonly TimeSpan _stabilityCheckInterval = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _stabilityDuration = TimeSpan.FromSeconds(60);
        private readonly ILogger<FileStabilityCheckerService> _logger = logger;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        public async Task CheckFileSizeStabilityAsync(string filePath)
        {
            CancellationToken cancellationToken = new CancellationTokenSource().Token;
            long lastFileSize = -1;
            DateTime lastChangeTime = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_stabilityCheckInterval, cancellationToken);
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var currentFileSize = fileInfo.Length;

                    if (currentFileSize != lastFileSize)
                    {
                        lastFileSize = currentFileSize;
                        lastChangeTime = DateTime.Now;
                    }
                    else if (DateTime.Now - lastChangeTime > _stabilityDuration)
                    {
                        await _fileStorageService.ProcessFileAsync(fileInfo, cancellationToken);
                        break;
                    }
                }
                catch (FileNotFoundException ex)
                {
                    _logger.LogWarning(ex, "File {filePath} does not exist.", filePath);
                    break;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error accessing file {filePath}", filePath);
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogWarning(ex, "File stability check for {filePath} was canceled.", filePath);
                    break;
                }
            }
        }
    }
}
using Guardian.Application.Interfaces;
using Guardian.Domain.Configs;
using Microsoft.Extensions.Options;

namespace Guardian.Application.Services
{
    public sealed class BackgroundTasksHandlerService(IScopeFactoryService scopeFactoryService, IOptions<GuardianConfig> configs) : IBackgroundTasksHandlerService
    {
        private readonly BackgroundTasksInterval _cleanFilesInterval = configs.Value.BackgroundTasks.CleanFiles;
        private readonly BackgroundTasksInterval _uploadFilesInterval = configs.Value.BackgroundTasks.UploadFiles;
        private readonly BackgroundTasksInterval _amIOnlineInterval = configs.Value.BackgroundTasks.AmIOnline;
        private readonly string _amIOnlinePath = configs.Value.LocalDrivePaths.AmIOnline;
        private readonly string _appStopped = configs.Value.LocalDrivePaths.AppStopped;
        private readonly IScopeFactoryService _scopeFactoryService = scopeFactoryService;
        private Timer? _amIOnlineTimer = null;
        private Timer? _cleanFilesTimer = null;
        private Timer? _uploadFilesTimer = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _amIOnlineTimer = new Timer(async _ => await AmIOnlineAsync(cancellationToken), null, TimeSpan.FromHours(_amIOnlineInterval.DueTime_H), TimeSpan.FromHours(_amIOnlineInterval.Interval_H));
            _cleanFilesTimer = new Timer(async _ => await CleanFilesAsync(cancellationToken), null, TimeSpan.FromHours(_cleanFilesInterval.DueTime_H), TimeSpan.FromHours(_cleanFilesInterval.Interval_H));
            _uploadFilesTimer = new Timer(async _ => await UploadMissingFilesAsync(cancellationToken), null, TimeSpan.FromHours(_uploadFilesInterval.DueTime_H), TimeSpan.FromHours(_uploadFilesInterval.Interval_H));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            File.WriteAllText(_appStopped, DateTime.Now.ToString());

            _amIOnlineTimer?.Change(Timeout.Infinite, 0);
            _cleanFilesTimer?.Change(Timeout.Infinite, 0);
            _uploadFilesTimer?.Change(Timeout.Infinite, 0);

            await CleanFilesAsync(cancellationToken);
            await AppStoppedAsync(cancellationToken);
        }

        private async Task AmIOnlineAsync(CancellationToken cancellationToken)
        {
            File.WriteAllText(_amIOnlinePath, DateTime.Now.ToString());
            await Task.Run(async () =>
            {
                await _scopeFactoryService.ExecuteInScopeAsync<IUploaderService>(async service =>
                {
                    await service.UploadFileAsync(new(_amIOnlinePath), cancellationToken);
                });
            }, cancellationToken);
        }

        private async Task CleanFilesAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                await _scopeFactoryService.ExecuteInScopeAsync<IUploaderService>(async service =>
                {
                    await service.CleanFilesAsync(cancellationToken);
                });
            }, cancellationToken);
        }

        private async Task UploadMissingFilesAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                await _scopeFactoryService.ExecuteInScopeAsync<IUploaderService>(async service =>
                {
                    await service.UploadMissingFilesAsync(cancellationToken);
                });
            }, cancellationToken);
        }

        private async Task AppStoppedAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                await _scopeFactoryService.ExecuteInScopeAsync<IUploaderService>(async service =>
                {
                    await service.UploadFileAsync(new(_appStopped), cancellationToken);
                });
            }, cancellationToken);
        }

        public void Dispose()
        {
            _amIOnlineTimer?.Dispose();
            _cleanFilesTimer?.Dispose();
            _uploadFilesTimer?.Dispose();
        }
    }
}

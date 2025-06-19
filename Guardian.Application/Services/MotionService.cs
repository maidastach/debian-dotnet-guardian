using Guardian.Application.Interfaces;
using Guardian.Domain.Abstractions;
using Guardian.Domain.DependencyInjection;
using Guardian.Domain.Dto;
using Guardian.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Guardian.Application.Services
{
    [ServiceLifetime(ServiceLifetime.Singleton)]
    public sealed class MotionService(IScopeFactoryService scopeFactoryService, ILogger<CommandService> logger) : IMotionService
    {
        private bool _isRunning = false;
        private Process? _process;
        private readonly IScopeFactoryService _scopeFactoryService = scopeFactoryService;
        private readonly ILogger<CommandService> _logger = logger;
        private void SetStatus(bool isRunning) => _isRunning = isRunning;

        public Result<MotionDto> GetStatus()
        {
            try
            {
                return Result<MotionDto>.Success(MotionDto.BuildMotionDto(_process, _isRunning));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error gettign motion status; Error: {Error}", ex.Message);
                return Result<MotionDto>.Failure(GuardianErrors.Exception(ex));
            }
        }

        public Result<MotionDto> Start(bool shouldMountDrive)
        {
            if (_process is not null)
            {
                return Result<MotionDto>.Failure(
                    GuardianErrors.InvalidAction(GuardianErrors.MotionAlreadyRunningMsg, MotionDto.BuildMotionDto(_process, _isRunning))
                );
            }
            try
            {
                KillExisting();
                if (shouldMountDrive)
                {
                    var mountDrive = _scopeFactoryService.ExecuteInScope<IDriveService, Result<Process>>(service =>
                    {
                        return service.Mount();
                    });
                    if (!mountDrive.IsSuccess)
                    {
                        _logger.LogError("Unable to mount drive. Error: {Error}", mountDrive.Error.Message);
                        return Result<MotionDto>.Failure(GuardianErrors.InvalidAction(mountDrive.Error.Message, MotionDto.BuildMotionDto(_process, _isRunning)));
                    }
                }

                var process = _scopeFactoryService.ExecuteInScope<ICommandService, Result<Process>>(service =>
                {
                    return service.Execute("motion", true);

                });
                if (!process.IsSuccess)
                {
                    _logger.LogError("Unable to start motion. Error: {Error}", process.Error.Message);
                    return Result<MotionDto>.Failure(GuardianErrors.InvalidAction(process.Error.Message, _process));
                }

                _process = process.Data;
                SetStatus(true);
                _logger.LogInformation("Motion started");
                return Result<MotionDto>.Success(MotionDto.BuildMotionDto(_process, _isRunning));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to start motion. Error: {Error}", ex.Message);
                return Result<MotionDto>.Failure(GuardianErrors.Exception(ex));
            }
        }

        public Result<MotionDto> Stop()
        {
            if (_process is null)
            {
                return Result<MotionDto>.Failure(
                    GuardianErrors.InvalidAction(GuardianErrors.MotionAlreadyStoppedMsg, MotionDto.BuildMotionDto(_process, _isRunning))
                );
            }
            try
            {
                _process.Kill();
                _process.WaitForExit();
                SetStatus(false);
                var result = MotionDto.BuildMotionDto(_process, _isRunning);
                _process.Dispose();
                _process = null;
                _logger.LogInformation("Motion stopped");
                return Result<MotionDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while stopping motion; Error: {Error}", ex.Message);
                return Result<MotionDto>.Failure(GuardianErrors.Exception(ex));
            }
        }

        private void KillExisting()
        {
            foreach (Process process in Process.GetProcessesByName("motion"))
            {
                _logger.LogWarning("Existing process not killed before, {process}", process.StartTime);
                process.Kill();
                process.WaitForExit();
                process.Dispose();
            }
        }
    }
}
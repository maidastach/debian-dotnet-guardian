using System.Diagnostics;
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
    [ServiceLifetime(ServiceLifetime.Scoped)]
    public sealed class DriveService(IOptions<GuardianConfig> configs, ICommandService commandService, ILogger<DriveService> logger) : IDriveService
    {
        private readonly ICommandService _commandService = commandService;
        private readonly ILogger<DriveService> _logger = logger;
        private readonly string _drivePath = configs.Value.LocalDrivePaths.Drive;
        private readonly string _mountPath = configs.Value.LocalDrivePaths.Mount;

        public Result<Process> Mount()
        {
            try
            {
                List<string> commands = [];
                CreateMountPoint(commands);
                MountDrive(commands);

                if (commands.Count == 0)
                {
                    return Result<Process>.Success(new());
                }

                foreach (var command in commands)
                {
                    var result = _commandService.Execute(command, isSudo: true);
                    if (!result.IsSuccess)
                    {
                        return result;
                    }
                }

                return RevokeSudo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mounting {drive} at {mount}; Error: {error}", _drivePath, _mountPath, ex.Message);
                return Result<Process>.Failure(GuardianErrors.Exception(ex));
            }
        }

        public Result<Process> Unmount()
        {
            try
            {
                var unmountDrive = _commandService.Execute($"umount {_mountPath}", isSudo: true);
                if (!unmountDrive.IsSuccess)
                {
                    return unmountDrive;
                }

                return RevokeSudo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unmounting {drive} at {mount}; Error: {error}", _drivePath, _mountPath, ex.Message);
                return Result<Process>.Failure(GuardianErrors.Exception(ex));
            }
        }

        private void CreateMountPoint(List<string> commands)
        {
            if (Directory.Exists(_mountPath))
            {
                return;
            }
            commands.Add($"mkdir -p {_mountPath}");
        }

        private void MountDrive(List<string> commands)
        {
            if (IsDriveMounted())
            {
                return;
            }
            commands.Add($"mount {_drivePath} {_mountPath}");
        }

        private Result<Process> RevokeSudo()
        {
            return _commandService.RevokeSudo();
        }

        private bool IsDriveMounted()
        {
            using var procMounts = File.OpenText("/proc/mounts");
            string? line;
            while ((line = procMounts.ReadLine()) != null)
            {
                if (line.Contains(_drivePath) && line.Contains(_mountPath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
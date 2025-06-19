using Guardian.Application.Interfaces;
using Guardian.Domain.Abstractions;
using Guardian.Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Guardian.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuardianController(IFileMonitorService fileMonitorService, IMotionService motionService, IServiceScopeFactory serviceScopeFactory) : ControllerBase
    {
        private readonly IFileMonitorService _fileMonitorService = fileMonitorService;
        private readonly IMotionService _motionService = motionService;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        [HttpPost]
        [Route("start")]
        public IActionResult StartApp([FromBody] StartAppRequestDto? body, CancellationToken cancellationToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var driveService = scope.ServiceProvider.GetRequiredService<IDriveService>();
                var mountDrive = driveService.Mount();
                if (!mountDrive.IsSuccess)
                {
                    return BadRequest(mountDrive.Error);
                }
            }

            var watchFolder = _fileMonitorService.Watch(body?.WatchPath, cancellationToken);

            if (!watchFolder.IsSuccess)
            {
                return BadRequest(watchFolder.Error);
            }

            var startMotion = _motionService.Start();

            if (!startMotion.IsSuccess)
            {
                return BadRequest(startMotion.Error);
            }

            return Ok(Result<StartAppDto>.Success(new StartAppDto(new(watchFolder.Data!), startMotion.Data!)).Data);
        }

        [HttpPost]
        [Route("stop")]
        public IActionResult StopApp([FromBody] StopAppRequestDto? body)
        {
            var motionResult = _motionService.Stop();

            if (!motionResult.IsSuccess)
            {
                return BadRequest(motionResult.Error);
            }

            var stopWatchFolder = _fileMonitorService.Stop(body?.WatchPath);

            if (!stopWatchFolder.IsSuccess)
            {
                return BadRequest(stopWatchFolder.Error);
            }

            string? mountDriveResult = null;
            if (body is not null && body.Unmount)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var driveService = scope.ServiceProvider.GetRequiredService<IDriveService>();
                var mountDrive = driveService.Unmount();
                mountDriveResult = mountDrive.IsSuccess ? null : mountDrive.Error.Message;
            }

            return Ok(Result<StartAppDto>.Success(new StartAppDto(new(stopWatchFolder.Data!), motionResult.Data!, new(mountDriveResult, mountDriveResult is null))).Data);
        }
    }
}
namespace Guardian.Domain.Dto
{
    public sealed record StartAppDto(FileMonitorDto FileMonitor, MotionDto Motion, DriveDto? UnmountDrive = null);
    public sealed record StartAppRequestDto(string WatchPath);
    public sealed record StopAppRequestDto(bool Unmount, string? WatchPath);
}
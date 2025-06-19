using Guardian.Domain.Abstractions;

namespace Guardian.Application.Interfaces
{
    public interface IFileMonitorService
    {
        Result<string> Watch(string? watchPath, CancellationToken cancellationToken);
        Result<string> Stop(string? watchPath);
    }
}
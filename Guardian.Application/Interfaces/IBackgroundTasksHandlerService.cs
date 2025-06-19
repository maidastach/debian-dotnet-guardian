using Microsoft.Extensions.Hosting;

namespace Guardian.Application.Interfaces
{
    public interface IBackgroundTasksHandlerService : IHostedService, IDisposable
    {
    }
}
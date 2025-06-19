using Guardian.Application.Interfaces;
using Guardian.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Guardian.Application.Services
{
    [ServiceLifetime(ServiceLifetime.Singleton)]
    public sealed class ScopeFactoryService(IServiceScopeFactory serviceScopeFactory) : IScopeFactoryService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        public async Task<TResult> ExecuteInScopeAsync<TService, TResult>(Func<TService, Task<TResult>> action) where TService : class
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return await action(service);
        }

        public async Task ExecuteInScopeAsync<TService>(Func<TService, Task> action) where TService : class
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await action(service);
        }

        public TResult ExecuteInScope<TService, TResult>(Func<TService, TResult> action) where TService : class
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return action(service);
        }
    }
}
namespace Guardian.Application.Interfaces
{
    public interface IScopeFactoryService
    {
        Task<TResult> ExecuteInScopeAsync<TService, TResult>(Func<TService, Task<TResult>> action) where TService : class;
        Task ExecuteInScopeAsync<TService>(Func<TService, Task> action) where TService : class;
        TResult ExecuteInScope<TService, TResult>(Func<TService, TResult> action) where TService : class;
    }
}
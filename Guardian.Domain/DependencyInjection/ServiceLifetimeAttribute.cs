using Microsoft.Extensions.DependencyInjection;

namespace Guardian.Domain.DependencyInjection
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ServiceLifetimeAttribute(ServiceLifetime lifetime) : Attribute
    {
        public ServiceLifetime Lifetime { get; } = lifetime;
    }
}
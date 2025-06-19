using System.Reflection;

namespace Guardian.Infrastructure.DependencyInjection
{
    public class InfrastructureAssemblyReference
    {
        public static readonly Assembly Assembly = typeof(InfrastructureAssemblyReference).Assembly;
    }
}
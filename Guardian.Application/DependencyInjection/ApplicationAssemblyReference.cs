using System.Reflection;

namespace Guardian.Application.DependencyInjection
{
    public class ApplicationAssemblyReference
    {
        public static readonly Assembly Assembly = typeof(ApplicationAssemblyReference).Assembly;
    }
}
namespace Guardian.Api.Extensions
{
    internal static class Cors
    {
        internal static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            var policyName = configuration.GetSection("Cors:Policy").Get<string>() ?? "";
            services.AddCors(
                options =>
                {
                    options.AddPolicy(policyName,
                        builder =>
                        {
                            builder.WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                        }
                    );
                }
            );
        }
    }
}
namespace WebApp.Platform;

internal static class WebHostEnvironmentExtensions
{
    internal static bool IsLocalEnv(this IWebHostEnvironment environment) =>
        environment.IsEnvironment("Local");
}

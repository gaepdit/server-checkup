using JetBrains.Annotations;
using ServerCheckupLibrary.Checks;
using System.Reflection;

namespace WebApp.Platform;

internal static class AppSettings
{
    public static CheckEmailOptions CheckEmailOptions { get; } = new();
    public static CheckDatabaseOptions CheckDatabaseOptions { get; } = new();
    public static CheckDatabaseEmailOptions CheckDatabaseEmailOptions { get; } = new();
    public static CheckExternalServiceOptions CheckExternalServiceOptions { get; } = new();
    public static CheckDotnetVersionOptions CheckDotnetVersionOptions { get; } = new();
    public static string ServerName { get; set; } = string.Empty;
    public static DevOptions DevOptions { get; } = new();

    public static string? Version { get; private set; }
    public static string? SimpleVersion => Version?.Split('+')[0];

    private static string Env { get; } = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown";
    public static string ShortEnv => Env switch { "Production" => "prod", "Staging" => "uat", _ => "dev" };
    public static DataDog DataDogSettings { get; } = new();

    public record DataDog
    {
        public string? ClientToken { get; [UsedImplicitly] init; }
        public string? ApplicationId { get; [UsedImplicitly] init; }
    }

    public static void BindAppSettings(this IHostApplicationBuilder builder)
    {
        // Set default timeout for regular expressions.
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices#use-time-out-values
        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));

        Version = GetVersion();
        ServerName = builder.Configuration.GetValue<string>(nameof(ServerName)) ?? "Unknown";

        builder.Configuration.GetSection(nameof(CheckEmailOptions)).Bind(CheckEmailOptions);
        builder.Configuration.GetSection(nameof(CheckDatabaseOptions)).Bind(CheckDatabaseOptions);
        builder.Configuration.GetSection(nameof(CheckDatabaseEmailOptions)).Bind(CheckDatabaseEmailOptions);
        builder.Configuration.GetSection(nameof(CheckExternalServiceOptions)).Bind(CheckExternalServiceOptions);
        builder.Configuration.GetSection(nameof(CheckDotnetVersionOptions)).Bind(CheckDotnetVersionOptions);
        builder.Configuration.GetSection(nameof(DevOptions)).Bind(DevOptions);
        builder.Configuration.GetSection(nameof(DataDogSettings)).Bind(DataDogSettings);
    }

    private static string GetVersion()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var segments = (entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? entryAssembly?.GetName().Version?.ToString() ?? "").Split('+');
        return segments[0] + (segments.Length > 0 ? $"+{segments[1][..Math.Min(7, segments[1].Length)]}" : "");
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class DevOptions
{
    public bool UseLocalAuth { get; init; }
    public string? AuthenticatedUser { get; init; }
}

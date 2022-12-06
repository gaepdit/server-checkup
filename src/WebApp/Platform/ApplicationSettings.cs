using CheckServerSetup.Checks;

namespace WebApp.Platform;

public static class ApplicationSettings
{
    public static CheckEmailOptions CheckEmailOptions { get; } = new();
    public static CheckDatabaseOptions CheckDatabaseOptions { get; } = new();
    public static CheckExternalServiceOptions CheckExternalServiceOptions { get; } = new();
    public static string ServerName { get; set; } = string.Empty;
}

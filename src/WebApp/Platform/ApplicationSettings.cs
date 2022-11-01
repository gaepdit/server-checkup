using CheckServerSetup.Checks;

namespace MyAppRoot.WebApp.Platform.Settings;

public static class ApplicationSettings
{
    public static CheckEmailOptions CheckEmailOptions { get; } = new();
    public static CheckDatabaseOptions CheckDatabaseOptions { get; } = new();
    public static CheckExternalServiceOptions CheckExternalServiceOptions { get; } = new();
}

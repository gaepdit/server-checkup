﻿using ServerCheckupLibrary.Checks;

namespace WebApp.Platform;

public static class ApplicationSettings
{
    public static CheckEmailOptions CheckEmailOptions { get; } = new();
    public static CheckDatabaseOptions CheckDatabaseOptions { get; } = new();
    public static CheckDatabaseEmailOptions CheckDatabaseEmailOptions { get; } = new();
    public static CheckExternalServiceOptions CheckExternalServiceOptions { get; } = new();
    public static CheckDotnetVersionOptions CheckDotnetVersionOptions { get; } = new();
    public static string ServerName { get; set; } = string.Empty;
    public static DevOptions DevOptions { get; } = new();

    // Raygun client settings
    public static RaygunClientSettings RaygunSettings { get; } = new();

    public class RaygunClientSettings
    {
        public string? ApiKey { get; init; }
    }
}

public class DevOptions
{
    public bool UseLocalAuth { get; init; }
    public string? AuthenticatedUser { get; init; }
}

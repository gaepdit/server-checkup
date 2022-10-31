using Microsoft.Extensions.Configuration;

namespace CheckServerSetup;

internal static class Program
{
    /// <param name="email">An email recipient to use with testing the email 
    /// setup (in addition to those in the appsettings.json file).</param>
    /// <param name="env">Optional environment name to load an alternate appsettings.{env}.json file.</param>
#pragma warning disable S1144
    // ReSharper disable once UnusedMember.Local
    private static async Task Main(string email, string env)
#pragma warning restore S1144
    {
        var checkDatabaseOptions = new CheckDatabaseOptions();
        var checkEmailOptions = new CheckEmailOptions();
        var checkExternalServicesOptions = new CheckExternalServicesOptions();

        try
        {
            var config = string.IsNullOrEmpty(env)
                ? new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
                : new ConfigurationBuilder().AddJsonFile($"appsettings.{env}.json").Build();

#pragma warning disable IL2026
            config.Bind(nameof(CheckDatabaseOptions), checkDatabaseOptions);
            config.Bind(nameof(CheckEmailOptions), checkEmailOptions);
            config.Bind(nameof(CheckExternalServicesOptions), checkExternalServicesOptions);
#pragma warning restore IL2026
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ExceptionMessage(ex, "configuration error"));
            return;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            checkEmailOptions.Recipients = checkEmailOptions.Recipients is null
                ? new[] { email }
                : checkEmailOptions.Recipients.Append(email).ToArray();
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule(Emoji.Known.Clipboard + $" Checking setup on {Environment.MachineName}.")
            .LeftAligned().RuleStyle("yellow bold"));

        await CheckDatabase.ExecuteAsync(checkDatabaseOptions);
        await CheckEmail.ExecuteAsync(checkEmailOptions);
        await CheckExternalServices.ExecuteAsync(checkExternalServicesOptions);

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Press any key to continue.");
        Console.ReadKey(true);
    }
}

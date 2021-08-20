using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using static CheckServerSetup.Messages;

namespace CheckServerSetup
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var _checkDatabaseOptions = new CheckDatabaseOptions();
            var _checkEmailOptions = new CheckEmailOptions();
            var _checkExternalServicesOptions = new CheckExternalServicesOptions();

            try
            {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                config.Bind(nameof(CheckDatabaseOptions), _checkDatabaseOptions);
                config.Bind(nameof(CheckEmailOptions), _checkEmailOptions);
                config.Bind(nameof(CheckExternalServicesOptions), _checkExternalServicesOptions);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ExceptionMessage(ex, "configuration error"));
                return;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Rule(Emoji.Known.Clipboard +
                $" Checking setup on {Environment.MachineName}.")
                .LeftAligned().RuleStyle("yellow bold"));

            await CheckDatabase.ExecuteAsync(_checkDatabaseOptions);
            await CheckEmail.ExecuteAsync(_checkEmailOptions);
            await CheckExternalServices.ExecuteAsync(_checkExternalServicesOptions);

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("Press any key to continue.");
            Console.ReadKey(true);
        }
    }
}

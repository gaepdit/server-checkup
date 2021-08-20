using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Spectre.Console;
using static CheckServerSetup.Messages;

namespace CheckServerSetup
{
    internal static class CheckExternalServices
    {
        public static async Task ExecuteAsync(CheckExternalServicesOptions _options)
        {
            AnsiConsole.WriteLine();

            if (_options.Skip())
            {
                AnsiConsole.Render(new Rule(Emoji.Known.StopSign +
                    " Skipping external services checks.")
                    .LeftAligned());
                return;
            }

            AnsiConsole.Render(new Rule(Emoji.Known.GlobeWithMeridians +
                " Checking connectivity to external services...")
                .LeftAligned().RuleStyle("blue"));
            AnsiConsole.WriteLine();

            var table = new Table().Collapse().Border(TableBorder.Rounded)
                .AddColumn("Destination").AddColumn("Result")
                .Caption("[blue][bold][slowblink]Working...[/][/][/]");

            await AnsiConsole.Live(table).StartAsync(async c =>
            {
                c.Refresh();

                foreach (var site in _options.ExternalServices)
                {
                    var tcpClient = new TcpClient();

                    try
                    {
                        var uri = new Uri(site);
                        await tcpClient.ConnectAsync(uri.Host, uri.Port);

                        if (tcpClient.Connected)
                        {
                            table.AddRow(site, $"[green]Connection successful.[/]");
                        }
                        else
                        {
                            table.AddRow(site, $"[blue]Host reached, but connection unsuccessful.[/]");
                        }
                    }
                    catch (Exception ex)
                    {
                        table.AddRow(site, ExceptionMessage(ex, "connection failed"));
                    }
                    finally
                    {
                        tcpClient.Close();
                    }

                    c.Refresh();
                }

                table.Caption("Finished checking external services.");
                c.Refresh();
            });
        }
    }

    internal class CheckExternalServicesOptions
    {
        public bool Enabled { get; set; }
        public string[] ExternalServices { get; set; }
        public bool Skip() =>
            !Enabled
            || ExternalServices == null
            || ExternalServices.Length == 0;

    }
}


using System.Net.Sockets;

namespace CheckServerSetup.Checks;

internal static class CheckExternalServices
{
    public static async Task ExecuteAsync(CheckExternalServicesOptions options)
    {
        AnsiConsole.WriteLine();

        if (options.Skip())
        {
            AnsiConsole.Write(new Rule(Emoji.Known.StopSign +
                " Skipping external services checks.")
                .LeftAligned());
            return;
        }

        AnsiConsole.Write(new Rule(Emoji.Known.GlobeWithMeridians +
            " Checking connectivity to external services...")
            .LeftAligned().RuleStyle("blue"));
        AnsiConsole.WriteLine();

        var table = new Table().Collapse().Border(TableBorder.Rounded)
            .AddColumn("Destination").AddColumn("Result")
            .Caption("[blue][bold][slowblink]Working...[/][/][/]");

        await AnsiConsole.Live(table).StartAsync(async c =>
        {
            c.Refresh();

            foreach (var site in options.ExternalServices)
            {
                var tcpClient = new TcpClient();

                try
                {
                    var uri = new Uri(site);
                    await tcpClient.ConnectAsync(uri.Host, uri.Port);

                    table.AddRow(site,
                        tcpClient.Connected
                            ? "[green]Connection successful.[/]"
                            : "[blue]Host reached, but connection unsuccessful.[/]");
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


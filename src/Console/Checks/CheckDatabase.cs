using System.Data.SqlClient;

namespace CheckServerSetup.Checks;

internal static class CheckDatabase
{
    public static async Task ExecuteAsync(CheckDatabaseOptions options)
    {
        AnsiConsole.WriteLine();

        if (options.Skip())
        {
            AnsiConsole.Write(new Rule(Emoji.Known.StopSign +
                " Skipping database connection checks.")
                .LeftAligned());
            return;
        }

        AnsiConsole.Write(new Rule(Emoji.Known.ClockwiseVerticalArrows +
            " Checking database connections...")
            .LeftAligned().RuleStyle("blue"));
        AnsiConsole.WriteLine();

        var table = new Table().Collapse().Border(TableBorder.Rounded)
            .AddColumn("Database connection").AddColumn("User").AddColumn("Result")
            .Caption("[blue][bold][slowblink]Working...[/][/][/]");

        await AnsiConsole.Live(table).StartAsync(async c =>
        {
            c.Refresh();

            foreach (var db in options.DatabaseConnections)
            {
                var builder = new SqlConnectionStringBuilder()
                {
                    DataSource = db.DataSource,
                    InitialCatalog = db.InitialCatalog,
                    IntegratedSecurity = false,
                    UserID = db.UserId,
                    Password = db.Password
                };

                try
                {
                    await using var connection = new SqlConnection(builder.ConnectionString);
                    const string query = "select 1";
                    await using var command = new SqlCommand(query, connection);
                    await connection.OpenAsync();
                    command.ExecuteScalar();
                    await connection.CloseAsync();

                    table.AddRow(db.DataSource, db.UserId, "[green]Connection successful.[/]");
                }
                catch (Exception ex)
                {
                    table.AddRow(db.DataSource, db.UserId, ExceptionMessage(ex, "connection failed"));
                }

                c.Refresh();
            }

            table.Caption("Finished checking database connections.");
            c.Refresh();
        });
    }
}

internal class CheckDatabaseOptions
{
    public bool Enabled { get; set; }
    public DatabaseConnections[] DatabaseConnections { get; set; }

    public bool Skip() =>
        !Enabled
        || DatabaseConnections is null
        || DatabaseConnections.Length == 0;
}

internal class DatabaseConnections
{
    public string DataSource { get; set; }
    public string InitialCatalog { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }
}


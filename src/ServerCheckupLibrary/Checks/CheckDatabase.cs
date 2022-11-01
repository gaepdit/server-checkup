using System.Data.SqlClient;

namespace CheckServerSetup.Checks;

public static class CheckDatabase
{
    private const string Query = "select 1";

    public static async Task<CheckResult> ExecuteAsync(CheckDatabaseOptions options)
    {
        var result = new CheckResult();

        if (!options.Enabled)
        {
            result.AddMessage(Context.Info, "Database checks are disabled.");
            return result;
        }

        if (options.DatabaseConnections is null || options.DatabaseConnections.Length == 0)
        {
            result.AddMessage(Context.Warning, "No database connections are configured.");
            return result;
        }

        foreach (var db in options.DatabaseConnections)
        {
            try
            {
                await CheckDatabaseConnection(db);
                result.AddMessage(Context.Success,
                    $"Successfully connected to \"{db.DataSource}\" as user \"{db.UserId}\".");
            }
            catch (Exception ex)
            {
                result.AddMessage(Context.Error,
                    $"Unable to connect to \"{db.DataSource}\" as user \"{db.UserId}\".",
                    ex.GetType().ToString());
            }
        }

        return result;
    }

    private static async Task CheckDatabaseConnection(DatabaseConnection db)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = db.DataSource,
            InitialCatalog = db.InitialCatalog,
            IntegratedSecurity = false,
            UserID = db.UserId,
            Password = db.Password,
        };

        await using var conn = new SqlConnection(builder.ConnectionString);
        await using var command = new SqlCommand(Query, conn);
        await conn.OpenAsync();
        command.ExecuteScalar();
        await conn.CloseAsync();
    }
}

public class CheckDatabaseOptions
{
    public bool Enabled { get; [UsedImplicitly] init; }
    public DatabaseConnection[]? DatabaseConnections { get; [UsedImplicitly] init; }
}

public class DatabaseConnection
{
    public string DataSource { get; [UsedImplicitly] init; } = "";
    public string InitialCatalog { get; [UsedImplicitly] init; } = "";
    public string UserId { get; [UsedImplicitly] init; } = "";
    public string Password { get; [UsedImplicitly] init; } = "";
}

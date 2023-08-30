using System.Data.SqlClient;

namespace CheckServerSetup.Checks;

public static class CheckDatabase
{
    private const string Query = "select sysdatetimeoffset();";

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
            await RecordDatabaseConnectionCheck(db, result);

        return result;
    }

    private static async Task RecordDatabaseConnectionCheck(DatabaseConnection db, CheckResult result)
    {
        try
        {
            var response = await CheckDatabaseConnection(db);
            if (string.IsNullOrEmpty(response))
            {
                result.AddMessage(Context.Warning,
                    db.TrustServerCertificate
                        ? $"Successfully connected to \"{db.DataSource}\" as user \"{db.UserId}\". (⚠️ May have used a self-signed certificate.)"
                        : $"Successfully made a secure connection to \"{db.DataSource}\" as user \"{db.UserId}\".",
                    string.IsNullOrEmpty(response)
                        ? "No data returned from server."
                        : $"Server datetime offset: {response}");
            }
            else
            {
                result.AddMessage(Context.Success,
                    db.TrustServerCertificate
                        ? $"Successfully connected to \"{db.DataSource}\" as user \"{db.UserId}\". (⚠️ May have used a self-signed certificate.)"
                        : $"Successfully made a secure connection to \"{db.DataSource}\" as user \"{db.UserId}\".",
                    $"Server datetime offset: {response}");
            }
        }
        catch (Exception ex)
        {
            result.AddMessage(Context.Error,
                db.TrustServerCertificate
                    ? $"Unable to connect to \"{db.DataSource}\" as user \"{db.UserId}\". [Self-signed certificate allowed.]"
                    : $"Unable to securely connect to \"{db.DataSource}\" as user \"{db.UserId}\".",
                ex.GetType().ToString());
        }
    }

    private static async Task<string?> CheckDatabaseConnection(DatabaseConnection db)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = db.DataSource,
            InitialCatalog = db.InitialCatalog,
            IntegratedSecurity = false,
            UserID = db.UserId,
            Password = db.Password,
            Encrypt = true,
            TrustServerCertificate = db.TrustServerCertificate,
        };

        await using var conn = new SqlConnection(builder.ConnectionString);
        await using var command = new SqlCommand(Query, conn);
        await conn.OpenAsync();
        var result = command.ExecuteScalar()?.ToString();
        await conn.CloseAsync();
        return result;
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
    public bool TrustServerCertificate { get; [UsedImplicitly] init; }
}

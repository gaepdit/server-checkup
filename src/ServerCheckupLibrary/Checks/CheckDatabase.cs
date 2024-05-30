using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

namespace ServerCheckupLibrary.Checks;

public static class CheckDatabase
{
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
        if (db.Vendor == "Oracle") return await CheckOracleDatabaseConnection(db);
        
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
        await using var command = new SqlCommand(cmdText: "select 1", conn);
        await conn.OpenAsync();
        var result = command.ExecuteScalar()?.ToString();
        await conn.CloseAsync();
        return result;
    }

    private static async Task<string?> CheckOracleDatabaseConnection(DatabaseConnection db)
    {
        var builder = new OracleConnectionStringBuilder
        {
            DataSource = db.DataSource,
            UserID = db.UserId,
            Password = db.Password,
        };

        await using var conn = new OracleConnection(builder.ConnectionString);
        await using var command = new OracleCommand(cmdText: "SELECT 1 FROM DUAL", conn);
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
    public string Vendor { get; [UsedImplicitly] init; } = string.Empty;
    public string DataSource { get; [UsedImplicitly] init; } = string.Empty;
    public string InitialCatalog { get; [UsedImplicitly] init; } = string.Empty;
    public string UserId { get; [UsedImplicitly] init; } = string.Empty;
    public string Password { get; [UsedImplicitly] init; } = string.Empty;
    public bool TrustServerCertificate { get; [UsedImplicitly] init; }
    public string DbEmailProfileName { get; [UsedImplicitly] init; } = string.Empty;
}

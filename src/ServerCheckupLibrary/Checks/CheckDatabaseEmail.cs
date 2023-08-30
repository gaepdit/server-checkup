using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;

namespace CheckServerSetup.Checks;

public static class CheckDatabaseEmail
{
    public static async Task<CheckResult> ExecuteAsync(CheckDatabaseEmailOptions options)
    {
        var result = new CheckResult();

        if (!options.Enabled)
        {
            result.AddMessage(Context.Info, "Database email checks are disabled.");
            return result;
        }

        if (options.DatabaseConnection is null ||
            string.IsNullOrEmpty(options.DbEmailProfileName) ||
            string.IsNullOrEmpty(options.Recipient))
        {
            result.AddMessage(Context.Warning, "Database email configuration is incomplete.");
            return result;
        }

        if (!MailAddress.TryCreate(options.Recipient, out var recipient))
        {
            result.AddMessage(Context.Warning,
                $"The configured recipient email is invalid: \"{options.Recipient}\"");
            return result;
        }

        await RecordDatabaseEmailCheck(options, recipient, result);

        return result;
    }

    private static async Task RecordDatabaseEmailCheck(CheckDatabaseEmailOptions options, MailAddress recipient,
        CheckResult result)
    {
        var db = options.DatabaseConnection!;

        try
        {
            await SendEmailFromDatabase(options, recipient);

            result.AddMessage(Context.Success,
                $"Successfully sent email from \"{db.DataSource}\" as user \"{db.UserId}\".",
                "(Check for receipt).");
        }
        catch (Exception ex)
        {
            result.AddMessage(Context.Error,
                $"Unable to send email from \"{db.DataSource}\" as user \"{db.UserId}\".",
                ex.GetType().ToString());
        }
    }

    private static async Task SendEmailFromDatabase(CheckDatabaseEmailOptions options, MailAddress recipient)
    {
        var db = options.DatabaseConnection!;
        const string spName = "msdb.dbo.sp_send_dbmail";

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
        await using var command = new SqlCommand(spName, conn);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@recipients", recipient.Address));
        command.Parameters.Add(new SqlParameter("@from_address", recipient.Address));
        command.Parameters.Add(new SqlParameter("@subject", $"Email test from database {db.DataSource}"));
        command.Parameters.Add(new SqlParameter("@body",
            $"This is a test email sent from database \"{db.DataSource}\" as user \"{db.UserId}\"."));
        command.Parameters.Add(new SqlParameter("@profile_name", options.DbEmailProfileName));

        await conn.OpenAsync();
        command.ExecuteNonQuery();
        await conn.CloseAsync();
    }
}

public class CheckDatabaseEmailOptions
{
    public bool Enabled { get; [UsedImplicitly] init; }
    public DatabaseConnection? DatabaseConnection { get; [UsedImplicitly] init; }
    public string DbEmailProfileName { get; [UsedImplicitly] init; } = "";
    public string Recipient { get; set; } = "";
    public string ServerName { get; set; } = string.Empty;
}

using System.Net.Mail;
using System.Text;

namespace CheckServerSetup.Checks;

public static class CheckEmail
{
    public static async Task<CheckResult> ExecuteAsync(CheckEmailOptions options)
    {
        var result = new CheckResult();
        var attempt = true;

        if (!options.Enabled)
        {
            result.AddMessage(Context.Info, "Email checks are disabled.");
            return result;
        }

        MailAddress? sender = null;

        if (options.SenderEmail == "")
        {
            result.AddMessage(Context.Warning, "No sender email is configured.");
            attempt = false;
        }
        else
        {
            try
            {
                sender = new MailAddress(options.SenderEmail);
            }
            catch (FormatException)
            {
                result.AddMessage(Context.Warning,
                    $"The configured sender email is invalid: \"{options.SenderEmail}\"");
                attempt = false;
            }
        }

        var recipients = new List<MailAddress>();

        if (options.Recipients == null)
        {
            result.AddMessage(Context.Warning, "No recipient emails are configured.");
            attempt = false;
        }
        else
        {
            foreach (var recipient in options.Recipients)
            {
                try
                {
                    recipients.Add(new MailAddress(recipient));
                }
                catch (FormatException)
                {
                    result.AddMessage(Context.Warning, $"A configured recipient email is invalid: \"{recipient}\"");
                }
            }
        }

        if (recipients.Count == 0)
        {
            result.AddMessage(Context.Warning, "Recipient email is missing.");
            attempt = false;
        }

        if (!attempt) return result;

        foreach (var recipient in recipients)
        {
            try
            {
                await SendTestEmail(sender!, recipient, options);
                result.AddMessage(Context.Success,
                    $"Successfully sent email to \"{recipient}\".", "(Check for receipt).");
            }
            catch (SmtpException ex)
            {
                result.AddMessage(Context.Error,
                    $"Unable to send email to \"{recipient}\".",
                    $"{ex.GetType()}; Status code: {ex.StatusCode.ToString()}");
            }
            catch (Exception ex)
            {
                result.AddMessage(Context.Error,
                    $"Unable to send email to \"{recipient}\".",
                    ex.GetType().ToString());
            }
        }

        if (!options.CheckSslEmail) return result;

        foreach (var recipient in recipients)
        {
            try
            {
                await SendTestEmail(sender!, recipient, options, true);
                result.AddMessage(Context.Success,
                    $"Successfully sent email to \"{recipient}\" using SSL.", "(Check for receipt).");
            }
            catch (SmtpException ex)
            {
                result.AddMessage(Context.Error,
                    $"Unable to send email to \"{recipient}\" using SSL.",
                    $"{ex.GetType()}; Status code: {ex.StatusCode.ToString()}");
            }
            catch (Exception ex)
            {
                result.AddMessage(Context.Error,
                    $"Unable to send email to \"{recipient}\" using SSL.",
                    ex.GetType().ToString());
            }
        }

        return result;
    }


    private static async Task SendTestEmail(
        MailAddress sender,
        MailAddress recipient,
        CheckEmailOptions options,
        bool enableSsl = false)
    {
        var serverName = string.IsNullOrWhiteSpace(options.ServerName)
            ? Environment.MachineName
            : $"{options.ServerName} ({Environment.MachineName})";

        using var message = new MailMessage(sender, recipient)
        {
            Subject = $"Email test from {serverName}{(enableSsl ? " (SSL enabled)" : string.Empty)}",
            Body =
                $"This is a test email sent from {serverName}{(enableSsl ? " (SSL enabled)" : string.Empty)}",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
        };

        using var client = new SmtpClient(options.SmtpHost, enableSsl ? options.SmtpSslPort : options.SmtpPort)
            { EnableSsl = enableSsl };
        await client.SendMailAsync(message);
    }
}

public class CheckEmailOptions
{
    public bool Enabled { get; [UsedImplicitly] init; }
    public string SenderEmail { get; [UsedImplicitly] init; } = "";
    public string SmtpHost { get; [UsedImplicitly] init; } = "";
    public int SmtpPort { get; [UsedImplicitly] init; }
    public bool CheckSslEmail { get; [UsedImplicitly] set; }
    public int SmtpSslPort { get; [UsedImplicitly] init; }
    public string[]? Recipients { get; set; }
    public string ServerName { get; set; } = string.Empty;
}

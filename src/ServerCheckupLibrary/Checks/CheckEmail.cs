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

        return result;
    }


    private static async Task SendTestEmail(MailAddress sender, MailAddress recipient, CheckEmailOptions options)
    {
        using var message = new MailMessage(sender, recipient)
        {
            Subject = $"Email test from {Environment.MachineName}",
            Body = $"This is a test email sent from {Environment.MachineName}",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
        };

        using var client = new SmtpClient(options.SmtpHost, options.SmtpPort) { EnableSsl = false };
        await client.SendMailAsync(message);
    }
}

public class CheckEmailOptions
{
    public bool Enabled { get; [UsedImplicitly] init; }
    public string SenderEmail { get; [UsedImplicitly] init; } = "";
    public string SmtpHost { get; [UsedImplicitly] init; } = "";
    public int SmtpPort { get; [UsedImplicitly] init; }
    public string[]? Recipients { get; [UsedImplicitly] set; }
}

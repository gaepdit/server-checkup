using Microsoft.AspNetCore.SignalR;
using ServerCheckupLibrary.Hubs;
using System.Net.Mail;
using System.Text;

namespace ServerCheckupLibrary.Checks;

public static class CheckEmail
{
    private static IHubContext<CheckHub>? _hubContext;

    public static async Task<CheckResult> ExecuteAsync(CheckEmailOptions options, IHubContext<CheckHub>? hubContext)
    {
        _hubContext = hubContext;
        var result = new CheckResult();

        if (!options.Enabled)
        {
            result.AddMessage(Context.Info, "Email checks are disabled.");
            return result;
        }

        if (string.IsNullOrEmpty(options.SenderEmail) || string.IsNullOrEmpty(options.Recipient))
        {
            result.AddMessage(Context.Warning, "Email configuration is incomplete.");
            return result;
        }

        if (!MailAddress.TryCreate(options.SenderEmail, out var sender))
        {
            result.AddMessage(Context.Warning,
                $"The configured sender email is invalid: \"{options.SenderEmail}\"");
            return result;
        }

        if (!MailAddress.TryCreate(options.Recipient, out var recipient))
        {
            result.AddMessage(Context.Warning,
                $"The configured recipient email is invalid: \"{options.Recipient}\"");
            return result;
        }

        await RecordEmailCheck(options, sender, recipient, result, false);

        if (options.CheckSslEmail)
            await RecordEmailCheck(options, sender, recipient, result, true);

        return result;
    }

    private static async Task RecordEmailCheck(CheckEmailOptions options, MailAddress sender, MailAddress recipient,
        CheckResult result, bool enableSsl)
    {
        var sslText = enableSsl ? " using SSL" : "";
        ResultMessage message;

        try
        {
            await SendTestEmail(sender, recipient, options, enableSsl);

            message = new ResultMessage(Context.Success,
                $"Successfully sent email to \"{recipient}\"{sslText}.",
                "(Check for receipt).");
        }
        catch (SmtpException ex)
        {
            message = new ResultMessage(Context.Error,
                $"Unable to send email to \"{recipient}\"{sslText}.",
                $"{ex.GetType()}; Status code: {ex.StatusCode.ToString()}");
        }
        catch (Exception ex)
        {
            message = new ResultMessage(Context.Error,
                $"Unable to send email to \"{recipient}\"{sslText}.",
                ex.GetType().ToString());
        }

        if (_hubContext != null)
        {
            await _hubContext.Clients.All.SendAsync(CheckHub.ReceiveCheckResult,
                "email-check", message.Text, message.Details, message.MessageContext.ToString());
        }

        result.AddMessage(message);
    }

    private static async Task SendTestEmail(MailAddress sender, MailAddress recipient, CheckEmailOptions options,
        bool enableSsl)
    {
        var serverName = string.IsNullOrWhiteSpace(options.ServerName)
            ? Environment.MachineName
            : $"{options.ServerName} ({Environment.MachineName})";

        using var message = new MailMessage(sender, recipient);
        message.Subject = $"Email test from {serverName}{(enableSsl ? " (SSL enabled)" : string.Empty)}";
        message.Body = $"This is a test email sent from {serverName}{(enableSsl ? " (SSL enabled)" : string.Empty)}";
        message.SubjectEncoding = Encoding.UTF8;
        message.BodyEncoding = Encoding.UTF8;

        using var client = new SmtpClient(options.SmtpHost, enableSsl ? options.SmtpSslPort : options.SmtpPort);
        client.EnableSsl = enableSsl;
        await client.SendMailAsync(message);
    }
}

public class CheckEmailOptions
{
    public bool Enabled { get; init; }
    public string SenderEmail { get; init; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public bool CheckSslEmail { get; set; }
    public int SmtpSslPort { get; init; }
}

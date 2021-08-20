using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using static CheckServerSetup.Messages;

namespace CheckServerSetup
{
    internal static class CheckEmail
    {
        public static async Task ExecuteAsync(CheckEmailOptions _options)
        {
            AnsiConsole.WriteLine();

            if (_options.Skip())
            {
                AnsiConsole.Render(new Rule(Emoji.Known.StopSign +
                    " Skipping email SMTP configuration checks.")
                    .LeftAligned());
                return;
            }

            AnsiConsole.Render(new Rule(Emoji.Known.ClosedMailboxWithRaisedFlag +
                " Checking email SMTP configuration...")
                .LeftAligned().RuleStyle("blue"));
            AnsiConsole.WriteLine();

            MailAddress _sender;
            MailAddress _recipient;

            try
            {
                _sender = new MailAddress(_options.SenderEmail);
            }
            catch (FormatException ex)
            {
                AnsiConsole.MarkupLine("[red]The configured sender email is invalid.[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
                return;
            }

            if (_options.Recipients is null || _options.Recipients.Length == 0)
            {
                var addRecipient = AnsiConsole.Prompt(new TextPrompt<string>(
                    "Enter email recipient (leave empty to skip):").AllowEmpty());

                if (string.IsNullOrWhiteSpace(addRecipient))
                {
                    AnsiConsole.WriteLine("Skipping email checks.");
                    return;
                }

                _options.Recipients = new string[] { addRecipient };
            }

            var table = new Table().Collapse().Border(TableBorder.Rounded)
                .AddColumn("Email recipient").AddColumn("Result")
                .Caption("[blue][bold][slowblink]Working...[/][/][/]");

            await AnsiConsole.Live(table).StartAsync(async c =>
            {
                c.Refresh();

                foreach (var recipient in _options.Recipients)
                {
                    try
                    {
                        _recipient = new MailAddress(recipient);
                    }
                    catch (FormatException ex)
                    {
                        table.AddRow(recipient, ExceptionMessage(ex, "recipient email is invalid"));
                        c.Refresh();
                        return;
                    }

                    try
                    {
                        using var message = new MailMessage(_sender, _recipient)
                        {
                            Subject = $"Email test from {Environment.MachineName}",
                            Body = $"This is a test email sent from {Environment.MachineName}",
                            SubjectEncoding = Encoding.UTF8,
                            BodyEncoding = Encoding.UTF8
                        };

                        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort) { EnableSsl = false };
                        await client.SendMailAsync(message);

                        table.AddRow(recipient, "[green]Email successfully sent.[/]\r\n[dim](Check for receipt of email.)[/]");
                    }
                    catch (Exception ex)
                    {
                        table.AddRow(recipient, ExceptionMessage(ex, "unable to send email"));
                    }

                    c.Refresh();
                }

                table.Caption("Finished checking email SMTP configuration.");
                c.Refresh();
            });
        }
    }

    internal class CheckEmailOptions
    {
        public bool Enabled { get; set; }
        public string SenderEmail { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string[] Recipients { get; set; }
        public bool Skip() =>
                !Enabled
                || string.IsNullOrEmpty(SenderEmail)
                || string.IsNullOrEmpty(SmtpHost)
                || SmtpPort == 0;
    }
}

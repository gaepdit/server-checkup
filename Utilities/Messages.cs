using System.Text;

namespace CheckServerSetup.Utilities;

internal static class Messages
{
    public static string ExceptionMessage(Exception ex, string description)
    {
        if (ex is null) return string.Empty;

        var message = new StringBuilder($"[red]Error: {description}.[/]\r\n{ex.Message}");

        if (ex.InnerException != null)
        {
            message.Append($"{message}\r\n[dim]{ex.InnerException.Message.EscapeMarkup()}[/]");
        }

        return message.ToString();
    }
}

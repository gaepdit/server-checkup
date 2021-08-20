using System;
using Spectre.Console;

namespace CheckServerSetup
{
    internal static class Messages
    {
        public static string ExceptionMessage(Exception ex, string description)
        {
            if (ex is null) return string.Empty;

            var message = $"[red]Error: {description}.[/]\r\n{ex.Message}";

            if (ex.InnerException != null)
            {
                message = $"{message}\r\n[dim]{ex.InnerException.Message.EscapeMarkup()}[/]";
            }

            return message;
        }
    }
}

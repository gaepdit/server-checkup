using System.Net.Sockets;

namespace ServerCheckupLibrary.Checks;

public static class CheckExternalService
{
    public static async Task<CheckResult> ExecuteAsync(CheckExternalServiceOptions options)
    {
        var result = new CheckResult();

        if (!options.Enabled)
        {
            result.AddMessage(Context.Info, "External service checks are disabled.");
            return result;
        }

        if (options.ExternalServices is null || options.ExternalServices.Length == 0)
        {
            result.AddMessage(Context.Warning, "No external services are configured.");
            return result;
        }

        foreach (var service in options.ExternalServices)
        {
            try
            {
                if (await CheckServiceConnection(service))
                {
                    result.AddMessage(Context.Success,
                        $"Successfully connected to \"{service}\".");
                }
                else
                {
                    result.AddMessage(Context.Warning,
                        $"Remote host \"{service}\" was reached, but connection was unsuccessful.");
                }
            }
            catch (Exception ex)
            {
                result.AddMessage(Context.Error,
                    $"Unable to connect to \"{service}\".",
                    ex.GetType().ToString());
            }
        }

        return result;
    }

    private static async Task<bool> CheckServiceConnection(string service)
    {
        using var tcpClient = new TcpClient();
        var uri = new Uri(service);
        await tcpClient.ConnectAsync(uri.Host, uri.Port);
        return tcpClient.Connected;
    }
}

public class CheckExternalServiceOptions
{
    public bool Enabled { get; [UsedImplicitly] init; }
    public string[]? ExternalServices { get; [UsedImplicitly] init; }
}

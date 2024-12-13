using Microsoft.AspNetCore.SignalR;
using ServerCheckupLibrary.Hubs;
using System.Net.Sockets;

namespace ServerCheckupLibrary.Checks;

public static class CheckExternalService
{
    private static IHubContext<CheckHub>? _hubContext;

    public static async Task<CheckResult> ExecuteAsync(CheckExternalServiceOptions options,
        IHubContext<CheckHub>? hubContext)
    {
        _hubContext = hubContext;
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
            await RecordExternalServiceCheck(service, result);

        return result;
    }

    private static async Task RecordExternalServiceCheck(string service, CheckResult result)
    {
        ResultMessage message;

        try
        {
            if (await CheckServiceConnection(service))
            {
                message = new ResultMessage(Context.Success,
                    $"Successfully connected to \"{service}\".");
            }
            else
            {
                message = new ResultMessage(Context.Warning,
                    $"Remote host \"{service}\" was reached, but connection was unsuccessful.");
            }
        }
        catch (Exception ex)
        {
            message = new ResultMessage(Context.Error,
                $"Unable to connect to \"{service}\".",
                ex.GetType().ToString());
        }

        if (_hubContext != null)
        {
            await _hubContext.Clients.All.SendAsync(CheckHub.ReceiveCheckResult,
                "service-check", message.Text, message.Details, message.MessageContext.ToString());
        }

        result.AddMessage(message);
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
    public bool Enabled { get; init; }
    public string[]? ExternalServices { get; init; }
}

using System.Net.Sockets;

namespace ServerCheckupLibrary.Checks;

public static class CheckExternalService
{
    private static IHubContext<CheckHub>? _hubContext;
    private static IHttpClientFactory _httpClientFactory = null!;

    public static async Task<CheckResult> ExecuteAsync(CheckExternalServiceOptions options,
        IHubContext<CheckHub>? hubContext, IHttpClientFactory httpClientFactory)
    {
        _hubContext = hubContext;
        _httpClientFactory = httpClientFactory;

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

    private static async Task RecordExternalServiceCheck(ExternalService service, CheckResult result)
    {
        ResultMessage message;

        try
        {
            if (await CheckServiceConnection(service))
            {
                message = new ResultMessage(Context.Success,
                    $"Successfully connected to \"{service.ServiceUri}\".");
            }
            else
            {
                message = service.Type switch
                {
                    "http" => new ResultMessage(Context.Warning,
                        $"Connected to remote host \"{service.ServiceUri}\" but it did not return a success status code."),
                    "tcp" => new ResultMessage(Context.Warning,
                        $"Remote host \"{service.ServiceUri}\" was reached, but connection was unsuccessful."),
                    _ => throw new ArgumentException("Invalid service type.", nameof(service)),
                };
            }
        }
        catch (Exception ex)
        {
            message = new ResultMessage(Context.Error,
                $"Unable to connect to \"{service.ServiceUri}\".",
                ex.GetType().ToString());
        }

        if (_hubContext != null)
        {
            await _hubContext.Clients.All.SendAsync(CheckHub.ReceiveCheckResult,
                "service-check", message.Text, message.Details, message.MessageContext.ToString());
        }

        result.AddMessage(message);
    }

    private static async Task<bool> CheckServiceConnection(ExternalService service) =>
        service.Type switch
        {
            "http" => await CheckHttpServiceConnection(service.ServiceUri),
            "tcp" => await CheckTcpServiceConnection(service.ServiceUri),
            _ => throw new ArgumentException("Invalid service type.", nameof(service)),
        };

    private static async Task<bool> CheckHttpServiceConnection(string serviceUri)
    {
        using var client = _httpClientFactory.CreateClient(nameof(CheckHttpServiceConnection));
        using var response = await client.GetAsync(serviceUri);
        return response.IsSuccessStatusCode;
    }

    private static async Task<bool> CheckTcpServiceConnection(string serviceUri)
    {
        using var tcpClient = new TcpClient();
        var uri = new Uri(serviceUri);
        await tcpClient.ConnectAsync(uri.Host, uri.Port);
        return tcpClient.Connected;
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public record CheckExternalServiceOptions
{
    public bool Enabled { get; init; }
    public ExternalService[]? ExternalServices { get; init; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public record ExternalService
{
    public required string ServiceUri { get; init; }
    public required string Type { get; init; } // Valid types are "http" and "tcp"
}

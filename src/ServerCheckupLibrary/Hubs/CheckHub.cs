namespace ServerCheckupLibrary.Hubs;

public class CheckHub : Hub
{
    public const string ReceiveCheckResult = nameof(ReceiveCheckResult);

    public Task SendCheckResult(string checkId, string message, string context) =>
        Clients.All.SendAsync(ReceiveCheckResult, checkId, message, context);
}

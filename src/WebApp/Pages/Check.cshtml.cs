using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using ServerCheckupLibrary;
using ServerCheckupLibrary.Checks;
using ServerCheckupLibrary.Hubs;
using WebApp.Platform;

namespace WebApp.Pages;

[Authorize]
public class CheckModel(IHubContext<CheckHub>? hubContext, IHttpClientFactory httpClientFactory) : PageModel
{
    public ICheckResult Result { get; private set; } = null!;

    public async Task OnGetEmailAsync()
    {
        AppSettings.CheckEmailOptions.Recipient = User.Identity?.Name ?? string.Empty;
        AppSettings.CheckEmailOptions.ServerName = AppSettings.ServerName;
        Result = await CheckEmail.ExecuteAsync(AppSettings.CheckEmailOptions, hubContext);
    }

    public async Task OnGetDatabaseAsync() =>
        Result = await CheckDatabase.ExecuteAsync(AppSettings.CheckDatabaseOptions, hubContext);

    public async Task OnGetDatabaseEmailAsync()
    {
        AppSettings.CheckDatabaseEmailOptions.Recipient = User.Identity?.Name ?? string.Empty;
        Result = await CheckDatabaseEmail.ExecuteAsync(AppSettings.CheckDatabaseEmailOptions, hubContext);
    }

    public async Task OnGetExternalServiceAsync() => Result =
        await CheckExternalService.ExecuteAsync(AppSettings.CheckExternalServiceOptions, hubContext, httpClientFactory);

    public void OnGetDotnetVersion() =>
        Result = CheckDotnetVersion.Execute(AppSettings.CheckDotnetVersionOptions);
}

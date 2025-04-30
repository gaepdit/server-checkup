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
        ApplicationSettings.CheckEmailOptions.Recipient = User.Identity?.Name ?? string.Empty;
        ApplicationSettings.CheckEmailOptions.ServerName = ApplicationSettings.ServerName;
        Result = await CheckEmail.ExecuteAsync(ApplicationSettings.CheckEmailOptions, hubContext);
    }

    public async Task OnGetDatabaseAsync() =>
        Result = await CheckDatabase.ExecuteAsync(ApplicationSettings.CheckDatabaseOptions, hubContext);

    public async Task OnGetDatabaseEmailAsync()
    {
        ApplicationSettings.CheckDatabaseEmailOptions.Recipient = User.Identity?.Name ?? string.Empty;
        Result = await CheckDatabaseEmail.ExecuteAsync(ApplicationSettings.CheckDatabaseEmailOptions, hubContext);
    }

    public async Task OnGetExternalServiceAsync() =>
        Result = await CheckExternalService.ExecuteAsync(ApplicationSettings.CheckExternalServiceOptions, hubContext,
            httpClientFactory);

    public void OnGetDotnetVersion() =>
        Result = CheckDotnetVersion.Execute(ApplicationSettings.CheckDotnetVersionOptions);
}

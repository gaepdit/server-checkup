﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCheckupLibrary;
using ServerCheckupLibrary.Checks;
using WebApp.Platform;

namespace WebApp.Pages;

[Authorize]
public class CheckModel : PageModel
{
    public ICheckResult Result { get; private set; } = null!;

    public async Task OnGetEmailAsync()
    {
        ApplicationSettings.CheckEmailOptions.Recipient = User.Identity?.Name ?? string.Empty;
        ApplicationSettings.CheckEmailOptions.ServerName = ApplicationSettings.ServerName;
        Result = await CheckEmail.ExecuteAsync(ApplicationSettings.CheckEmailOptions);
    }

    public async Task OnGetDatabaseAsync() =>
        Result = await CheckDatabase.ExecuteAsync(ApplicationSettings.CheckDatabaseOptions);

    public async Task OnGetDatabaseEmailAsync()
    {
        ApplicationSettings.CheckDatabaseEmailOptions.Recipient = User.Identity?.Name ?? string.Empty;
        Result = await CheckDatabaseEmail.ExecuteAsync(ApplicationSettings.CheckDatabaseEmailOptions);
    }

    public async Task OnGetExternalServiceAsync() =>
        Result = await CheckExternalService.ExecuteAsync(ApplicationSettings.CheckExternalServiceOptions);

    public void OnGetDotnetVersion() =>
        Result = CheckDotnetVersion.Execute(ApplicationSettings.CheckDotnetVersionOptions);
}

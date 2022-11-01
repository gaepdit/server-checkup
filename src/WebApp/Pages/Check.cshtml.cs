using CheckServerSetup.Checks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAppRoot.WebApp.Platform.Settings;

namespace WebApp.Pages;

public class CheckModel : PageModel
{
    public CheckResult Result { get; set; } = null!;

    public async Task OnGetEmailAsync() =>
        Result = await CheckEmail.ExecuteAsync(ApplicationSettings.CheckEmailOptions);

    public async Task OnGetDatabaseAsync() =>
        Result = await CheckDatabase.ExecuteAsync(ApplicationSettings.CheckDatabaseOptions);

    public async Task OnGetExternalServiceAsync() =>
        Result = await CheckExternalService.ExecuteAsync(ApplicationSettings.CheckExternalServiceOptions);
}

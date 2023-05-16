using CheckServerSetup.Checks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Platform;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    public ResultMessage? EmailCheckMessage { get; private set; }
    public ResultMessage? DatabaseCheckMessage { get; private set; }
    public ResultMessage? ExternalServiceCheckMessage { get; private set; }
    public ResultMessage? DotnetVersionCheckMessage { get; private set; }

    public string? InformationalVersion { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity is not { IsAuthenticated: true })
            return Challenge();

        if (!ApplicationSettings.CheckEmailOptions.Enabled)
        {
            EmailCheckMessage = new ResultMessage(Context.Info, "Email checks are disabled.");
        }
        else if (string.IsNullOrEmpty(User.Identity.Name))
        {
            EmailCheckMessage = new ResultMessage(Context.Warning, "No recipient email available.");
        }

        if (!ApplicationSettings.CheckDatabaseOptions.Enabled)
            DatabaseCheckMessage = new ResultMessage(Context.Info, "Database checks are disabled.");

        if (!ApplicationSettings.CheckExternalServiceOptions.Enabled)
            ExternalServiceCheckMessage =
                new ResultMessage(Context.Info, "External service checks are disabled.");

        if (!ApplicationSettings.CheckDotnetVersionOptions.Enabled)
            DotnetVersionCheckMessage = new ResultMessage(Context.Info, ".NET version checks are disabled.");

        InformationalVersion = typeof(IndexModel).Assembly.GetName().Version?.ToString(3);

        return Page();
    }
}

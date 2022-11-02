using CheckServerSetup.Checks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Platform;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    public CheckResult.Message? EmailCheckMessage { get; private set; }
    public CheckResult.Message? DatabaseCheckMessage { get; private set; }
    public CheckResult.Message? ExternalServiceCheckMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity is not { IsAuthenticated: true })
            return Challenge();

        if (!ApplicationSettings.CheckEmailOptions.Enabled)
        {
            EmailCheckMessage = new CheckResult.Message(CheckResult.Context.Info, "Email checks are disabled.");
        }
        else if (string.IsNullOrEmpty(User.Identity.Name))
        {
            EmailCheckMessage = new CheckResult.Message(CheckResult.Context.Warning, "No recipient email available.");
        }

        if (!ApplicationSettings.CheckDatabaseOptions.Enabled)
            DatabaseCheckMessage = new CheckResult.Message(CheckResult.Context.Info, "Database checks are disabled.");

        if (!ApplicationSettings.CheckExternalServiceOptions.Enabled)
            ExternalServiceCheckMessage =
                new CheckResult.Message(CheckResult.Context.Info, "External service checks are disabled.");

        return Page();
    }
}

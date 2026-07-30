using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCheckupLibrary;
using WebApp.Platform;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    public ResultMessage? EmailCheckMessage { get; private set; }
    public ResultMessage? DatabaseCheckMessage { get; private set; }
    public ResultMessage? DatabaseEmailCheckMessage { get; private set; }
    public ResultMessage? ExternalServiceCheckMessage { get; private set; }
    public ResultMessage? DotnetVersionCheckMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity is not { IsAuthenticated: true })
            return Challenge();

        if (!AppSettings.CheckEmailOptions.Enabled || string.IsNullOrEmpty(User.Identity.Name))
        {
            EmailCheckMessage = new ResultMessage(Context.Info, "Email checks are disabled.");
        }

        if (!AppSettings.CheckDatabaseOptions.Enabled)
        {
            DatabaseCheckMessage = new ResultMessage(Context.Info, "Database checks are disabled.");
        }

        if (!AppSettings.CheckDatabaseEmailOptions.Enabled || string.IsNullOrEmpty(User.Identity.Name))
        {
            DatabaseEmailCheckMessage = new ResultMessage(Context.Info, "Database email checks are disabled.");
        }

        if (!AppSettings.CheckExternalServiceOptions.Enabled)
        {
            ExternalServiceCheckMessage = new ResultMessage(Context.Info, "External service checks are disabled.");
        }

        if (!AppSettings.CheckDotnetVersionOptions.Enabled)
        {
            DotnetVersionCheckMessage = new ResultMessage(Context.Info, ".NET version checks are disabled.");
        }

        return Page();
    }
}

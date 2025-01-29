using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCheckupLibrary;
using System.Reflection;
using WebApp.Platform;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    public ResultMessage? EmailCheckMessage { get; private set; }
    public ResultMessage? DatabaseCheckMessage { get; private set; }
    public ResultMessage? DatabaseEmailCheckMessage { get; private set; }
    public ResultMessage? ExternalServiceCheckMessage { get; private set; }
    public ResultMessage? DotnetVersionCheckMessage { get; private set; }

    public string? Version { get; private set; }
    public string? Build { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity is not { IsAuthenticated: true })
            return Challenge();

        if (!ApplicationSettings.CheckEmailOptions.Enabled || string.IsNullOrEmpty(User.Identity.Name))
        {
            EmailCheckMessage = new ResultMessage(Context.Info, "Email checks are disabled.");
        }

        if (!ApplicationSettings.CheckDatabaseOptions.Enabled)
        {
            DatabaseCheckMessage = new ResultMessage(Context.Info, "Database checks are disabled.");
        }

        if (!ApplicationSettings.CheckDatabaseEmailOptions.Enabled || string.IsNullOrEmpty(User.Identity.Name))
        {
            DatabaseEmailCheckMessage = new ResultMessage(Context.Info, "Database email checks are disabled.");
        }

        if (!ApplicationSettings.CheckExternalServiceOptions.Enabled)
        {
            ExternalServiceCheckMessage = new ResultMessage(Context.Info, "External service checks are disabled.");
        }

        if (!ApplicationSettings.CheckDotnetVersionOptions.Enabled)
        {
            DotnetVersionCheckMessage = new ResultMessage(Context.Info, ".NET version checks are disabled.");
        }

        // Get app version.
        var entryAssembly = Assembly.GetEntryAssembly();
        var segments = (entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? entryAssembly?.GetName().Version?.ToString() ?? "").Split('+');

        Version = segments[0];
        if (segments.Length > 0) Build = segments[1][..Math.Min(7, segments[1].Length)];

        return Page();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;
using ZLogger;

namespace WebApp.Pages;

[AllowAnonymous]
[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don\'t access instance data should be static")]
public class TestErrorHandling : PageModel
{
    public void OnGetHandledAsync([FromServices] ILogger<TestErrorHandling> logger)
    {
        try
        {
            throw new TestException("Test handled exception");
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Handled exception message");
        }
    }

    public void OnGetUnhandled()
    {
        throw new TestException("Test unhandled exception");
    }

    public class TestException(string message) : Exception(message);
}

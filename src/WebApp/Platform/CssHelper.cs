using CheckServerSetup.Checks;

namespace WebApp.Platform;

public static class CssHelper
{
    public static string ContextSuffix(Context context) => context switch
    {
        Context.Success => "success",
        Context.Info => "info",
        Context.Warning => "warning",
        Context.Error => "danger",
        _ => string.Empty,
    };
}

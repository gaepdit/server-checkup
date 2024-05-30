using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ServerCheckupLibrary.Checks;

public static class CheckDotnetVersion
{
    public static InfoResult Execute(CheckDotnetVersionOptions options)
    {
        var result = new InfoResult();

        if (!options.Enabled)
        {
            result.AddMessage(".NET version checks are disabled.");
            return result;
        }

        result.AddMessage($".NET: {Environment.Version}");
        result.AddDotnetFrameworkVersionMessage();

        return result;
    }

    private static void AddDotnetFrameworkVersionMessage(this InfoResult result)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            result.AddMessage(".NET Framework is not available for this platform.", RuntimeInformation.OSDescription);
            return;
        }

        const string subKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
        using var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            .OpenSubKey(subKey);

        var releaseKey = (int?)ndpKey?.GetValue("Release");

        var versionMessage = releaseKey != null
            ? $".NET Framework: {CheckFor45PlusVersion(releaseKey.Value)}"
            : ".NET Framework version 4.5 or later is not detected.";

        result.AddMessage(versionMessage, $"(Release key: {releaseKey})");
    }

    // Checking the version using >= enables forward compatibility.
    private static string CheckFor45PlusVersion(int releaseKey)
    {
        return releaseKey switch
        {
            >= 533320 => "4.8.1 or later",
            >= 528040 => "4.8",
            >= 461808 => "4.7.2",
            >= 461308 => "4.7.1",
            >= 460798 => "4.7",
            >= 394802 => "4.6.2",
            >= 394254 => "4.6.1",
            >= 393295 => "4.6",
            >= 379893 => "4.5.2",
            >= 378675 => "4.5.1",
            >= 378389 => "4.5",
            // This code should never execute. A non-null release key should mean that 4.5 or later is installed.
            _ => "No 4.5 or later version detected.",
        };
    }
}

public class CheckDotnetVersionOptions
{
    public bool Enabled { get;  init; }
}

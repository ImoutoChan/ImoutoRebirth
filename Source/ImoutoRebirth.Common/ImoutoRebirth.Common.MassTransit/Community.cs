using HarmonyLib;
using MassTransit.Licensing;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
namespace ImoutoRebirth.Common.MassTransit;

public static class Community
{
    public static void Fix(ILogger? logger = null)
    {
        Environment.SetEnvironmentVariable("MT_LICENSE", "community");

        var harmony = new Harmony("app");

        try
        {
            var targetMethod2 = AccessTools.Method(
                typeof(LicenseReader),
                nameof(LicenseReader.Load),
                [typeof(string)]
            );

            harmony.Patch(
                targetMethod2,
                new HarmonyMethod(typeof(Community), nameof(Patch2)));
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Error patching");
            throw new InvalidOperationException("Fix was broken", ex);
        }
    }

    public static bool Patch2(ref LicenseInfo __result)
    {
        __result = new()
        {
            Customer = new() { Id = "X", Name = "X" },
            Expires = new DateTime(2099, 1, 1)
        };
        return false;
    }
}

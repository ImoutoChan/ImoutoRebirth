using System.Runtime.CompilerServices;

namespace ImoutoRebirth.Tori.Tests;

public static class VerifierStaticSettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.DontScrubGuids();

        UseProjectRelativeDirectory("Verified");
    }
}

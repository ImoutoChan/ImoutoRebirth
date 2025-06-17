using System.Runtime.CompilerServices;

namespace ImoutoRebirth.Arachne.Tests;

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

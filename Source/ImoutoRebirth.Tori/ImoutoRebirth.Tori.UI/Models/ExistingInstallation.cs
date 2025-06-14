using System.IO;

namespace ImoutoRebirth.Tori.UI.Models;

public record ExistingInstallation(DirectoryInfo InstallLocation, bool IsValid);
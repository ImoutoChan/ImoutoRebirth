using System.Drawing;

namespace ImoutoRebirth.Common.WindowsAssociationManager;

public class ApplicationIcon
{
    public ApplicationIcon(string iconlibrarypath, string iExtractIconGuid = null)
    {
        IconLibraryPath = iconlibrarypath;
        IconIndex = null;
        IExtractIconGuid = iExtractIconGuid;
    }

    public ApplicationIcon(string iconlibrarypath, int iconindex)
    {
        IconLibraryPath = iconlibrarypath;
        IconIndex = iconindex;
    }

    public string IconLibraryPath { get; }

    public int? IconIndex { get; }

    public string IExtractIconGuid { get; }

    public Icon GetIcon(IconManager.IconSize iconSize, string filename)
    {
        var iconManager = new IconManager(this);
        return iconManager.GetIcon(iconSize, filename);
    }
}

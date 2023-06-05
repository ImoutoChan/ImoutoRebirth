namespace ImoutoViewer.ViewModel.SettingsModels;

internal class ResizeTypeDescriptor
{
    #region Properties

    public string Name { get; private set; }
    public ResizeType Type { get; private set; }

    #endregion Properties

    #region Methods

    public override string ToString()
    {
        return Name;
    }

    #endregion Methods

    #region Static methods

    public static List<ResizeTypeDescriptor> GetList()
    {
        return new List<ResizeTypeDescriptor>
        {
            new ResizeTypeDescriptor { Name = "Fit to screen (downscale only)", Type = ResizeType.DownscaleToViewPort },
            new ResizeTypeDescriptor { Name = "Fit to screen (down & up scale)", Type = ResizeType.FitToViewPort },
            new ResizeTypeDescriptor { Name = "Fit to screen width (downscale only)", Type = ResizeType.DownscaleToViewPortWidth },
            new ResizeTypeDescriptor { Name = "Fit to screen width (down & up scale)", Type = ResizeType.FitToViewPortWidth },
            new ResizeTypeDescriptor { Name = "Original size (no resize)", Type = ResizeType.NoResize },
        };
    }

    #endregion Static methods
}
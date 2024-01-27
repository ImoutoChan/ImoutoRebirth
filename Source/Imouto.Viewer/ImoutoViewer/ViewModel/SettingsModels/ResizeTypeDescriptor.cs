namespace ImoutoViewer.ViewModel.SettingsModels;

internal class ResizeTypeDescriptor
{
    public required string Name { get; init; }
    public required ResizeType Type { get; init; }

    public override string ToString() => Name;

    public static List<ResizeTypeDescriptor> GetList()
    {
        return new List<ResizeTypeDescriptor>
        {
            new() { Name = "Fit to screen (downscale only)", Type = ResizeType.DownscaleToViewPort },
            new() { Name = "Fit to screen (down & up scale)", Type = ResizeType.FitToViewPort },
            new() { Name = "Fit to screen width (downscale only)", Type = ResizeType.DownscaleToViewPortWidth },
            new() { Name = "Fit to screen width (down & up scale)", Type = ResizeType.FitToViewPortWidth },
            new() { Name = "Original size (no resize)", Type = ResizeType.NoResize },
        };
    }
}

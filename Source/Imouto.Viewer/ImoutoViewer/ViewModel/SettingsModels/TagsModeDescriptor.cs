namespace ImoutoViewer.ViewModel.SettingsModels;

internal class TagsModeDescriptor
{
    public required string Name { get; init; }
    
    public required byte Value { get; init; }

    public override string ToString() => Name;

    public static List<TagsModeDescriptor> GetListForFiles()
    {
        return new List<TagsModeDescriptor>
        {
            new() { Name = "Show", Value = 1 },
            new() { Name = "Hide", Value = 0  },
        };
    }
}

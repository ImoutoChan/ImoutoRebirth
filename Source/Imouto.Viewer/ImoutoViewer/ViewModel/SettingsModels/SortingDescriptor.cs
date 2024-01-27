using ImoutoViewer.Model;

namespace ImoutoViewer.ViewModel.SettingsModels;

internal class SortingDescriptor
{
    public required string Name { get; set; }
    public required SortMethod Method { get; set; }

    public override string ToString() => Name;

    public static List<SortingDescriptor> GetListForFiles()
    {
        return
        [
            new SortingDescriptor { Name = "Name", Method = SortMethod.ByName },
            new SortingDescriptor { Name = "Date created", Method = SortMethod.ByCreateDate },
            new SortingDescriptor { Name = "Date modified", Method = SortMethod.ByUpdateDate },
            new SortingDescriptor { Name = "Size", Method = SortMethod.BySize }
        ];
    }

    public static List<SortingDescriptor> GetListForFolders()
    {
        return
        [
            new SortingDescriptor { Name = "Name", Method = SortMethod.ByName },
            new SortingDescriptor { Name = "Date created", Method = SortMethod.ByCreateDate },
            new SortingDescriptor { Name = "Date modified", Method = SortMethod.ByUpdateDate }
        ];
    }
}

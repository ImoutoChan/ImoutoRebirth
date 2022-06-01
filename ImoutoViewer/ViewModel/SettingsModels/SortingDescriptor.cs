using ImoutoViewer.Model;

namespace ImoutoViewer.ViewModel.SettingsModels;

internal class SortingDescriptor
{
    #region Properties

    public string Name { get; set; }
    public SortMethod Method { get; set; }

    #endregion Properties

    #region Methods

    public override string ToString()
    {
        return Name;
    }

    #endregion Methods

    #region Static methods

    public static List<SortingDescriptor> GetListForFiles()
    {
        return new List<SortingDescriptor>
        {
            new SortingDescriptor { Name = "Name", Method = SortMethod.ByName },
            new SortingDescriptor { Name = "Date created", Method = SortMethod.ByCreateDate  },
            new SortingDescriptor { Name = "Date modified", Method = SortMethod.ByUpdateDate },
            new SortingDescriptor { Name = "Size", Method = SortMethod.BySize }
        };
    }

    public static List<SortingDescriptor> GetListForFolders()
    {
        return new List<SortingDescriptor>
        {
            new SortingDescriptor { Name = "Name", Method = SortMethod.ByName },
            new SortingDescriptor { Name = "Date created", Method = SortMethod.ByCreateDate  },
            new SortingDescriptor { Name = "Date modified", Method = SortMethod.ByUpdateDate }
        };
    }

    #endregion Static methods
}
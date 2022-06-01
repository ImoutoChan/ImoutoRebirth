namespace ImoutoViewer.ViewModel.SettingsModels;

internal class TagsModeDescriptor
{
    #region Properties

    public string Name { get; set; }
    public byte Value { get; set; }

    #endregion Properties

    #region Methods

    public override string ToString()
    {
        return Name;
    }

    #endregion Methods

    #region Static methods

    public static List<TagsModeDescriptor> GetListForFiles()
    {
        return new List<TagsModeDescriptor>
        {
            new TagsModeDescriptor { Name = "Show", Value = 1 },
            new TagsModeDescriptor { Name = "Hide", Value = 0  },
        };
    }

    #endregion Static methods
}
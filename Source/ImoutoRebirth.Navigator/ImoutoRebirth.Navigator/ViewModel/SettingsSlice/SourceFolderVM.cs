using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice;

internal partial class SourceFolderVM : FolderVM
{
    [ObservableProperty]
    public partial bool CheckFormat { get; set; }

    [ObservableProperty]
    public partial bool CheckNameHash { get; set; }

    [ObservableProperty]
    public partial bool TagsFromSubfolder { get; set; }

    [ObservableProperty]
    public partial bool AddTagFromFileName { get; set; }
    public ObservableCollection<string> SupportedExtensionsRaw { get; }

    public SourceFolderVM(
        Guid? id, 
        string path, 
        bool checkFormat, 
        bool checkNameHash, 
        IEnumerable<string>? extensions, 
        bool tagsFromSubfolder, 
        bool addTagFromFilename) 
        : base(id, path)
    {
        CheckFormat = checkFormat;
        CheckNameHash = checkNameHash;
        TagsFromSubfolder = tagsFromSubfolder;
        AddTagFromFileName = addTagFromFilename;

        SupportedExtensionsRaw = extensions != null
            ? new ObservableCollection<string>(extensions) 
            : new ObservableCollection<string>();
    }

    public string SupportedExtensions
    {
        get => string.Join(";", SupportedExtensionsRaw);
        set
        {
            try
            {
                var list = value.Split(';').ToList();
                SupportedExtensionsRaw.Clear();
                foreach (var item in list)
                {
                    SupportedExtensionsRaw.Add(item);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}

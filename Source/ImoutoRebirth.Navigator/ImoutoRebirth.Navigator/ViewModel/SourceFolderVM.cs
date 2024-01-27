using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF.Commands;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class SourceFolderVM : FolderVM, IDataErrorInfo
{
    private bool _checkFormat;
    private bool _checkNameHash;
    private bool _tagsFromSubfolder;
    private bool _addTagFromFilename;

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

    public bool CheckFormat
    {
        get => _checkFormat;
        set => OnPropertyChanged(ref _checkFormat, value, () => CheckFormat);
    }

    public bool CheckNameHash
    {
        get => _checkNameHash;
        set => OnPropertyChanged(ref _checkNameHash, value, () => CheckNameHash);
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

    public ObservableCollection<string> SupportedExtensionsRaw { get; }

    public bool TagsFromSubfolder
    {
        get => _tagsFromSubfolder;
        set => OnPropertyChanged(ref _tagsFromSubfolder, value, () => TagsFromSubfolder);
    }

    public bool AddTagFromFileName
    {
        get => _addTagFromFilename;
        set => OnPropertyChanged(ref _addTagFromFilename, value, () => AddTagFromFileName);
    }

    public string this[string columnName]
    {
        get
        {
            string errorMessage = string.Empty;
            switch (columnName)
            {
                case "Path":
                    if (string.IsNullOrWhiteSpace(Path))
                    {
                        errorMessage = "Path can't be empty";
                    }
                    else
                    {
                        try
                        {
                            _ = new DirectoryInfo(Path);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Incorrect path format";
                        }
                    }
                    break;
            }
            return errorMessage;
        }
    }

    public override string Error => this["Path"];

    private ICommand? _saveCommand;

    public ICommand SaveCommand
        => _saveCommand ??= new RelayCommand(_ => OnSaveRequest(), _ => string.IsNullOrWhiteSpace(Error));
}

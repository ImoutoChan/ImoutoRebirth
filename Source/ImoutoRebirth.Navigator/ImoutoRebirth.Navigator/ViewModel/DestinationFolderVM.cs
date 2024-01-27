using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF.Commands;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class DestinationFolderVM : FolderVM, IDataErrorInfo
{
    private bool _needDevideImagesByHash;
    private bool _needRename;
    private string? _incorrectFormatSubpath;
    private string? _incorrectHashSubpath;
    private string? _nonHashSubpath;

    private ICommand? _removeCommand;
    private ICommand? _saveCommand;

    public bool NeedDevideImagesByHash
    {
        get => _needDevideImagesByHash;
        set { OnPropertyChanged(ref _needDevideImagesByHash, value, () => NeedDevideImagesByHash); }
    }

    public bool NeedRename
    {
        get => _needRename;
        set => OnPropertyChanged(ref _needRename, value, () => NeedRename);
    }

    public string? IncorrectFormatSubpath
    {
        get => _incorrectFormatSubpath;
        set => OnPropertyChanged(ref _incorrectFormatSubpath, value, () => IncorrectFormatSubpath);
    }

    public string? IncorrectHashSubpath
    {
        get => _incorrectHashSubpath;
        set => OnPropertyChanged(ref _incorrectHashSubpath, value, () => IncorrectHashSubpath);
    }

    public string? NonHashSubpath
    {
        get => _nonHashSubpath;
        set => OnPropertyChanged(ref _nonHashSubpath, value, () => NonHashSubpath);
    }

    public DestinationFolderVM(Guid? id,
        string path,
        bool needDevideImagesByHash,
        bool needRename,
        string incorrectFormatSubpath,
        string incorrectHashSubpath,
        string nonHashSubpath)
        : base(id, path)
    {
        _needDevideImagesByHash = needDevideImagesByHash;
        _needRename = needRename;
        _incorrectFormatSubpath = incorrectFormatSubpath;
        _incorrectHashSubpath = incorrectHashSubpath;
        _nonHashSubpath = nonHashSubpath;
    }

    public ICommand RemoveCommand => _removeCommand ??= new RelayCommand((s) => OnRemoveRequest());

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand((s) => OnSaveRequest(), (s) => string.IsNullOrWhiteSpace(Error));

    public event EventHandler? RemoveRequest;

    private void OnRemoveRequest() => RemoveRequest?.Invoke(this, EventArgs.Empty);

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
                            var di = new DirectoryInfo(Path);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Incorrect path format";
                        }
                    }
                    break;
                case "IncorrectFormatSubpath":
                    if (string.IsNullOrWhiteSpace(IncorrectFormatSubpath))
                    {
                        errorMessage = "Can't be empty";
                    }
                    else
                    {
                        try
                        {
                            var di = new DirectoryInfo("C:\\" + IncorrectFormatSubpath);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Incorrect path format";
                        }
                    }
                    break;
                case "IncorrectHashSubpath":
                    if (string.IsNullOrWhiteSpace(IncorrectHashSubpath))
                    {
                        errorMessage = "Can't be empty";
                    }
                    else
                    {
                        try
                        {
                            var di = new DirectoryInfo("C:\\" + IncorrectHashSubpath);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Incorrect path format";
                        }
                    }
                    break;
                case "NonHashSubpath":
                    if (string.IsNullOrWhiteSpace(NonHashSubpath))
                    {
                        errorMessage = "Can't be empty";
                    }
                    else
                    {
                        try
                        {
                            var di = new DirectoryInfo("C:\\" + NonHashSubpath);
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

    public override string Error =>
        this["Path"] + Environment.NewLine
                     + this["IncorrectFormatSubpath"] + Environment.NewLine
                     + this["IncorrectHashSubpath"] + Environment.NewLine
                     + this["NonHashSubpath"] + Environment.NewLine;
}

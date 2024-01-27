using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;

namespace ImoutoRebirth.Navigator.ViewModel;

internal abstract class FolderVM : VMBase
{
    private string _path;

    protected FolderVM(Guid? id, string path)
    {
        Id = id;
        _path = path;
    }

    public Guid? Id { get; }

    public string Path
    {
        get => _path;
        set { OnPropertyChanged(ref _path, value, () => Path); }
    }

    private ICommand? _resetCommand;

    public ICommand ResetCommand => _resetCommand ??= new RelayCommand(_ => OnResetRequest());

    public event EventHandler? ResetRequest;

    private void OnResetRequest()
    {
        var handler = ResetRequest;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SaveRequest;

    protected void OnSaveRequest()
    {
        var handler = SaveRequest;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public abstract string Error { get; }
}

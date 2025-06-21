using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common.WPF.ValidationAttributes;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice;

internal abstract partial class FolderVM : ObservableValidator
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter path to the folder")]
    [Directory("Please enter valid path to the folder")]
    private string _path;

    protected FolderVM(Guid? id, string path)
    {
        Id = id;
        _path = path;
    }

    public Guid? Id { get; }

    [RelayCommand]
    private void Reset()
    {
        var handler = ResetRequest;
        handler?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        var handler = SaveRequest;
        handler?.Invoke(this, EventArgs.Empty);
    }

    private bool CanSave() => !HasErrors;
    
    public event EventHandler? ResetRequest;

    public event EventHandler? SaveRequest;
}

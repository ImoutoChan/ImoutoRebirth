using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common.WPF.ValidationAttributes;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Models;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.ViewModels;
using ImoutoRebirth.Tori.UI.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImoutoRebirth.Tori.UI.Steps.Locations;

public partial class LocationsStepViewModel : ObservableValidator, IStep
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter a valid install location")]
    [Directory("Please enter a valid install location", onlyAbsolutePath: true)]
    private string? _installLocation;

    [ObservableProperty]
    private bool _installLocationEditable = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyDataErrorInfo]
    [Directory("Please enter valid folder name", onlyAbsolutePath: true)]
    private string? _favSaveLocation;

    public LocationsStepViewModel(IRegistryService registryService, IMessenger messenger)
    {
        _messenger = messenger;
        if (registryService.IsInstalled(out var installLocation))
        {
            _installLocation = installLocation.FullName;
            _installLocationEditable = false;
        }
    }

    public string Title =>  "Locations";

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext()
        => _messenger.Send(new NavigateTo(InstallerStep.Database));

    private bool CanGoNext() => !HasErrors;

    [RelayCommand]
    private void GoBack()
        => _messenger.Send(new NavigateTo(InstallerStep.Accounts));

    [RelayCommand]
    private void BrowseFavSaveLocation()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Title = "Select a folder",
            AllowNonFileSystemItems = false,
            Multiselect = false,
            EnsurePathExists = true,
            NavigateToShortcut = true
        };

        if (!string.IsNullOrWhiteSpace(FavSaveLocation))
        {
            dialog.InitialDirectory = FavSaveLocation;
        }

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            FavSaveLocation = dialog.FileName;
        }
    }

    [RelayCommand]
    private void BrowseInstallLocation()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Title = "Select a folder",
            AllowNonFileSystemItems = false,
            Multiselect = false,
            EnsurePathExists = true,
            NavigateToShortcut = true
        };

        if (!string.IsNullOrWhiteSpace(InstallLocation))
        {
            dialog.InitialDirectory = InstallLocation;
        }

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            InstallLocation = dialog.FileName;
        }
    }
}

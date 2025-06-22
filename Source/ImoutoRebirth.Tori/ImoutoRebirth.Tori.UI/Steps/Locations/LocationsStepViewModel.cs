using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common.WPF.ValidationAttributes;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImoutoRebirth.Tori.UI.Steps.Locations;

public partial class LocationsStepViewModel : ObservableValidator, IStep
{
    private readonly IMessenger _messenger;
    private readonly IConfigurationStorage _configurationStorage;

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

    public LocationsStepViewModel(IMessenger messenger, IConfigurationStorage configurationStorage)
    {
        _messenger = messenger;
        _configurationStorage = configurationStorage;

        _ = FillProperties();
    }

    public string Title =>  "Locations";

    public int State => 3;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext()
    {
        PrepareLocations();
        _messenger.Send(new NavigateTo(InstallerStep.Database));
    }

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

    private void PrepareLocations()
    {
        if (InstallLocationEditable)
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                InstallLocation = InstallLocation!
            });
		}

        _configurationStorage.UpdateConfiguration(x => x with
        {
            Harpy = x.Harpy with
            {
                SavePath = FavSaveLocation!.Replace(@"\", @"\\")
            }
        });
    }

    private async Task FillProperties()
    {
        await _configurationStorage.ConfigurationLoaded;

        FavSaveLocation = _configurationStorage.CurrentConfiguration.Harpy.SavePath.Replace(@"\\", @"\");
        InstallLocation = _configurationStorage.InstallLocation;
        InstallLocationEditable = _configurationStorage.InstallLocationEditable;
    }
}

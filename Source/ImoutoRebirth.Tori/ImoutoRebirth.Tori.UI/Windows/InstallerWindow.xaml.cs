using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;
using MahApps.Metro.Controls;

namespace ImoutoRebirth.Tori.UI.Windows;

public partial class InstallerWindow : MetroWindow
{
    private readonly IMessenger _messenger;

    public InstallerWindow(InstallerViewModel viewModel, IMessenger messenger)
    {
        _messenger = messenger;
        InitializeComponent();

        DataContext = viewModel;
    }

    private void InstallWizardStepsProgress_OnOnStateClicked(object? sender, int e)
    {
        _messenger.Send(new NavigateTo((InstallerStep)e));
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;

namespace ImoutoRebirth.Tori.UI.Steps.Accounts;

public partial class AccountsStepViewModel : ObservableObject, IStep
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private string? _danbooruLogin;

    [ObservableProperty]
    private string? _danbooruApiKey;

    [ObservableProperty]
    private string? _sankakuLogin;

    [ObservableProperty]
    private string? _sankakuPassword;

    [ObservableProperty]
    private string? _yandereLogin;

    [ObservableProperty]
    private string? _yandereApiKey;

    [ObservableProperty]
    private string? _exHentaiIpbMemberId;

    [ObservableProperty]
    private string? _exHentaiIpbPassHash;

    [ObservableProperty]
    private string? _exHentaiIgneous;

    [ObservableProperty]
    private string? _exHentaiUserAgent;

    public AccountsStepViewModel(IMessenger messenger)
        => _messenger = messenger;

    public string Title =>  "Accounts";

    [RelayCommand]
    private void GoNext()
        => _messenger.Send(new NavigateTo(InstallerStep.Locations));

    [RelayCommand]
    private void GoBack()
        => _messenger.Send(new NavigateTo(InstallerStep.Prerequisites));
}

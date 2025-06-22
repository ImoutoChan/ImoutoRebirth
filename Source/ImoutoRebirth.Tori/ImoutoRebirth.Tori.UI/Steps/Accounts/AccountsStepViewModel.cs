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
    [NotifyPropertyChangedFor(nameof(IsDanbooruFilled))]
    private string? _danbooruLogin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDanbooruFilled))]
    private string? _danbooruApiKey;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSankakuFilled))]
    private string? _sankakuLogin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSankakuFilled))]
    private string? _sankakuPassword;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYandereFilled))]
    private string? _yandereLogin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYandereFilled))]
    private string? _yandereApiKey;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string? _exHentaiIpbMemberId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string? _exHentaiIpbPassHash;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string? _exHentaiIgneous;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string? _exHentaiUserAgent;

    public bool IsDanbooruFilled => !string.IsNullOrWhiteSpace(DanbooruLogin) && !string.IsNullOrWhiteSpace(DanbooruApiKey);

    public bool IsSankakuFilled => !string.IsNullOrWhiteSpace(SankakuLogin) && !string.IsNullOrWhiteSpace(SankakuPassword);

    public bool IsYandereFilled => !string.IsNullOrWhiteSpace(YandereLogin) && !string.IsNullOrWhiteSpace(YandereApiKey);

    public bool IsExhentaiFilled
        => !string.IsNullOrWhiteSpace(ExHentaiIgneous)
           && !string.IsNullOrWhiteSpace(ExHentaiIpbMemberId)
           && !string.IsNullOrWhiteSpace(ExHentaiIpbPassHash)
           && !string.IsNullOrWhiteSpace(ExHentaiUserAgent);

    public AccountsStepViewModel(IMessenger messenger, ConfigurationToInstallStorage configurationToInstallStorage)
    {
        _messenger = messenger;

        var currentConfiguration = configurationToInstallStorage.CurrentConfiguration;

        if (currentConfiguration != null)
        {
            DanbooruApiKey = currentConfiguration.Api.DanbooruApiKey;
            DanbooruLogin = currentConfiguration.Api.DanbooruLogin;
            SankakuLogin = currentConfiguration.Api.SankakuLogin;
            SankakuPassword = currentConfiguration.Api.SankakuPassword;
            YandereLogin = currentConfiguration.Api.YandereLogin;
            YandereApiKey = currentConfiguration.Api.YandereApiKey;
            ExHentaiIpbMemberId = currentConfiguration.ExHentai.IpbMemberId;
            ExHentaiIpbPassHash = currentConfiguration.ExHentai.IpbPassHash;
            ExHentaiIgneous = currentConfiguration.ExHentai.Igneous;
            ExHentaiUserAgent = currentConfiguration.ExHentai.UserAgent;
		}
    }

    public string Title =>  "Accounts";

    public int State => 2;

    [RelayCommand]
    private void GoNext()
        => _messenger.Send(new NavigateTo(InstallerStep.Locations));

    [RelayCommand]
    private void GoBack()
        => _messenger.Send(new NavigateTo(InstallerStep.Prerequisites));
}

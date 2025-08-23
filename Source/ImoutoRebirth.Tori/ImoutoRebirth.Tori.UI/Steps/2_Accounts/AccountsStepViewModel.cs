using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Configuration;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;
using System.Diagnostics;

namespace ImoutoRebirth.Tori.UI.Steps;

public partial class AccountsStepViewModel : ObservableObject, IStep
{
    private readonly IMessenger _messenger;
    private readonly IConfigurationStorage _configurationStorage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDanbooruFilled))]
    private string _danbooruLogin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDanbooruFilled))]
    private string _danbooruApiKey;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSankakuFilled))]
    private string _sankakuLogin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSankakuFilled))]
    private string _sankakuPassword;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYandereFilled))]
    private string _yandereLogin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYandereFilled))]
    private string _yandereApiKey;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGelbooruFilled))]
    private string _gelbooruUserId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGelbooruFilled))]
    private string _gelbooruApiKey;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRule34Filled))]
    private string _rule34UserId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRule34Filled))]
    private string _rule34ApiKey;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string _exHentaiIpbMemberId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string?_exHentaiIpbPassHash;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string _exHentaiIgneous;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExhentaiFilled))]
    private string _exHentaiUserAgent;

    public bool IsDanbooruFilled => !string.IsNullOrWhiteSpace(DanbooruLogin) && !string.IsNullOrWhiteSpace(DanbooruApiKey);

    public bool IsSankakuFilled => !string.IsNullOrWhiteSpace(SankakuLogin) && !string.IsNullOrWhiteSpace(SankakuPassword);

    public bool IsYandereFilled => !string.IsNullOrWhiteSpace(YandereLogin) && !string.IsNullOrWhiteSpace(YandereApiKey);

    public bool IsGelbooruFilled => !string.IsNullOrWhiteSpace(GelbooruUserId) && !string.IsNullOrWhiteSpace(GelbooruApiKey);

    public bool IsRule34Filled => !string.IsNullOrWhiteSpace(Rule34UserId) && !string.IsNullOrWhiteSpace(Rule34ApiKey);

    public bool IsExhentaiFilled
        => !string.IsNullOrWhiteSpace(ExHentaiIgneous)
           && !string.IsNullOrWhiteSpace(ExHentaiIpbMemberId)
           && !string.IsNullOrWhiteSpace(ExHentaiIpbPassHash)
           && !string.IsNullOrWhiteSpace(ExHentaiUserAgent);

    public AccountsStepViewModel(IMessenger messenger, IConfigurationStorage configurationStorage)
    {
        _messenger = messenger;
        _configurationStorage = configurationStorage;

        var currentConfiguration = configurationStorage.CurrentConfiguration;

        DanbooruApiKey = currentConfiguration.Api.DanbooruApiKey;
        DanbooruLogin = currentConfiguration.Api.DanbooruLogin;
        SankakuLogin = currentConfiguration.Api.SankakuLogin;
        SankakuPassword = currentConfiguration.Api.SankakuPassword;
        YandereLogin = currentConfiguration.Api.YandereLogin;
        YandereApiKey = currentConfiguration.Api.YandereApiKey;
        GelbooruApiKey = currentConfiguration.Api.GelbooruApiKey;
        GelbooruUserId = currentConfiguration.Api.GelbooruUserId;
        Rule34UserId = currentConfiguration.Api.Rule34UserId;
        Rule34ApiKey = currentConfiguration.Api.Rule34ApiKey;
        ExHentaiIpbMemberId = currentConfiguration.ExHentai.IpbMemberId;
        ExHentaiIpbPassHash = currentConfiguration.ExHentai.IpbPassHash;
        ExHentaiIgneous = currentConfiguration.ExHentai.Igneous;
        ExHentaiUserAgent = currentConfiguration.ExHentai.UserAgent;
    }

    public string Title =>  "Accounts";

    public int State => 2;

    [RelayCommand]
    private void GoNext()
    {
        PrepareAccounts();
        _messenger.Send(new NavigateTo(InstallerStep.Locations));
    }

    [RelayCommand]
    private void GoBack() => _messenger.Send(new NavigateTo(InstallerStep.Prerequisites));

    [RelayCommand]
    private void OpenHyperlink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private void PrepareAccounts()
    {
        if (IsDanbooruFilled)
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    DanbooruLogin = DanbooruLogin ?? "",
                    DanbooruApiKey = DanbooruApiKey ?? ""
                }
            });
        }
        else
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    DanbooruLogin = "",
                    DanbooruApiKey = ""
                }
            });
        }

        if (IsSankakuFilled)
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    SankakuLogin = SankakuLogin ?? "",
                    SankakuPassword = SankakuPassword ?? ""
                }
            });
        }
        else
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    SankakuLogin = "",
                    SankakuPassword = ""
                }
            });
        }

        if (IsYandereFilled)
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    YandereLogin = YandereLogin ?? "",
                    YandereApiKey = YandereApiKey ?? ""
                }
            });
        }
        else
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    YandereLogin = "",
                    YandereApiKey = ""
                }
            });
        }

        if (IsGelbooruFilled)
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    GelbooruApiKey = GelbooruApiKey ?? "",
                    GelbooruUserId = GelbooruUserId ?? ""
                }
            });
        }
        else
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    GelbooruApiKey = "",
                    GelbooruUserId = ""
                }
            });
        }

        if (IsExhentaiFilled)
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                ExHentai = new AppConfiguration.ExHentaiSettings(
                    IpbMemberId: ExHentaiIpbMemberId ?? "",
                    IpbPassHash: ExHentaiIpbPassHash ?? "",
                    Igneous: ExHentaiIgneous ?? "",
                    UserAgent: ExHentaiUserAgent ?? "")
            });
        }
        else
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                ExHentai = new AppConfiguration.ExHentaiSettings(
                    IpbMemberId: "",
                    IpbPassHash: "",
                    Igneous: "",
                    UserAgent: "")
            });
        }

        if (IsRule34Filled)
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    Rule34ApiKey = Rule34ApiKey ?? "",
                    Rule34UserId = Rule34UserId ?? ""
                }
            });
        }
        else
        {
            _configurationStorage.UpdateConfiguration(x => x with
            {
                Api = x.Api with
                {
                    Rule34ApiKey = "",
                    Rule34UserId = ""
                }
            });
        }
    }
}

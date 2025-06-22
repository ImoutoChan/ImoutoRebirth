using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common.WPF.ValidationAttributes;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;
using Npgsql;

namespace ImoutoRebirth.Tori.UI.Steps.Database;

public partial class DatabaseStepViewModel : ObservableValidator, IStep
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    private string? _host = "localhost";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    private string? _user = "postgres";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    private string? _pass = "postgres";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(1, 100_000)]
    private int _port = 5432;
    
    [ObservableProperty]
    private bool _isConnectionChecking;

    [ObservableProperty]
    private bool _isConnectionSuccessful;

    [ObservableProperty]
    private bool _isConnectionFailed;

    [ObservableProperty]
    private string? _connectionStatusMessage;

    public DatabaseStepViewModel(IMessenger messenger, ConfigurationToInstallStorage configurationToInstallStorage)
    {
        _messenger = messenger;
        var currentConfiguration = configurationToInstallStorage.CurrentConfiguration;

        if (currentConfiguration != null)
        {
            var builder = new NpgsqlConnectionStringBuilder(currentConfiguration.Connection.RoomConnectionString);

            Host = builder.Host;
            Port = builder.Port;
            User = builder.Username;
            Pass = builder.Password;
		}
	}

    public string Title =>  "Database";

    public int State => 4;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext()
        => _messenger.Send(new NavigateTo(InstallerStep.Installation));

    private bool CanGoNext() => !HasErrors;

    [RelayCommand]
    private void GoBack()
        => _messenger.Send(new NavigateTo(InstallerStep.Locations));

    [RelayCommand]
    private async Task CheckConnection()
    {
        try
        {
            IsConnectionChecking = true;
            IsConnectionSuccessful = false;
            IsConnectionFailed = false;
            ConnectionStatusMessage = null;

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                Username = User,
                Password = Pass,
                Database = "postgres",
                Timeout = 5
            };

            await Task.Run(async () =>
            {
                await Task.Delay(1500);
                await using var connection = new NpgsqlConnection(connectionStringBuilder.ToString());
                await connection.OpenAsync();

                // debug states
                // if (Random.Shared.Next(1, 3) == 2)
                //     throw new Exception("oops");
            });

            IsConnectionSuccessful = true;
        }
        catch (NpgsqlException ex)
        {
            IsConnectionFailed = true;
            ConnectionStatusMessage = $"Connection failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            IsConnectionFailed = true;
            ConnectionStatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsConnectionChecking = false;
        }
    }

    // todo disable this step if postgres is not installed
}

using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common.WPF.ValidationAttributes;
using ImoutoRebirth.Tori.Configuration;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;
using Npgsql;

namespace ImoutoRebirth.Tori.UI.Steps.Database;

public partial class DatabaseStepViewModel : ObservableValidator, IStep
{
    private readonly IMessenger _messenger;
    private readonly IConfigurationStorage _configurationStorage;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    private string _host = "localhost";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    private string _user = "postgres";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    private string _pass = "postgres";

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

    [ObservableProperty]
    private bool _isPostgresSettingsEnabled;

    public DatabaseStepViewModel(IMessenger messenger, IConfigurationStorage configurationStorage)
    {
        _messenger = messenger;
        _configurationStorage = configurationStorage;

        var currentConfiguration = configurationStorage.CurrentConfiguration;
        var builder = new NpgsqlConnectionStringBuilder(currentConfiguration.Connection.RoomConnectionString);

        Host = builder.Host!;
        Port = builder.Port;
        User = builder.Username!;
        Pass = builder.Password!;

        IsPostgresSettingsEnabled = !configurationStorage.ShouldInstallPostgreSql;
    }

    public string Title =>  "Database";

    public int State => 4;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext()
    {
        PrepareConnections();
        _messenger.Send(new NavigateTo(InstallerStep.Installation));
    }

    private bool CanGoNext() => !HasErrors;

    [RelayCommand]
    private void GoBack() => _messenger.Send(new NavigateTo(InstallerStep.Locations));

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

    private void PrepareConnections()
    {
        var currentConfiguration = _configurationStorage.CurrentConfiguration;
        var roomBuilder = new NpgsqlConnectionStringBuilder(currentConfiguration.Connection.RoomConnectionString);
        var lilinBuilder = new NpgsqlConnectionStringBuilder(currentConfiguration.Connection.LilinConnectionString);
        var meidoBuilder = new NpgsqlConnectionStringBuilder(currentConfiguration.Connection.MeidoConnectionString);
        var massTransitBuilder = new NpgsqlConnectionStringBuilder(currentConfiguration.Connection.MassTransitConnectionString);

        if (!string.IsNullOrWhiteSpace(Host))
        {
            roomBuilder.Host = Host;
            lilinBuilder.Host = Host;
            meidoBuilder.Host = Host;
            massTransitBuilder.Host = Host;
        }

        if (Port != 0)
        {
            roomBuilder.Port = Port;
            lilinBuilder.Port = Port;
            meidoBuilder.Port = Port;
            massTransitBuilder.Port = Port;
        }

        if (!string.IsNullOrWhiteSpace(User))
        {
            roomBuilder.Username = User;
            lilinBuilder.Username = User;
            meidoBuilder.Username = User;
            massTransitBuilder.Username = User;
        }

        if (!string.IsNullOrWhiteSpace(Pass))
        {
            roomBuilder.Password = Pass;
            lilinBuilder.Password = Pass;
            meidoBuilder.Password = Pass;
            massTransitBuilder.Password = Pass;
        }

        _configurationStorage.UpdateConfiguration(x
            => x with
            {
                Connection = new AppConfiguration.ConnectionSettings(
                    RoomConnectionString: roomBuilder.ConnectionString,
                    LilinConnectionString: lilinBuilder.ConnectionString,
                    MeidoConnectionString: meidoBuilder.ConnectionString,
                    MassTransitConnectionString: massTransitBuilder.ConnectionString)
            });
    }
}

using System.Reflection;
using Serilog;

namespace ImoutoRebirth.Navigator.Services;

public interface ISettingsUpgradeService
{
    void UpgradeSettingsIfRequired();
}

public class SettingsUpgradeService : ISettingsUpgradeService
{
    private static readonly ILogger Logger = Log.ForContext<SettingsUpgradeService>();
    
    public void UpgradeSettingsIfRequired()
    {
        try
        {
            var settings = Settings.Default;
            
            if (settings.IsUpgradeRequired)
            {
                Logger.Information("Upgrading application settings from previous version");
                
                settings.Upgrade();
                settings.IsUpgradeRequired = false;
                settings.Save();
                
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
                Logger.Information("Settings upgraded successfully for version {Version}", currentVersion);
            }
            else
            {
                Logger.Debug("Settings upgrade not required");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to upgrade application settings");
        }
    }
}

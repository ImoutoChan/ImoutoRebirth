using System.Diagnostics;

namespace ImoutoRebirth.Tori.Services;

public interface IShortcutService
{
    void CreateShortcutToDesktop(string exePath, string name);
}

internal class ShortcutService : IShortcutService
{
    public void CreateShortcutToDesktop(string exePath, string name)
    {
        var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @$"\{name}.lnk";

        var strCmdText = $"$ws = New-Object -ComObject WScript.Shell; " +
                         $"$s = $ws.CreateShortcut('{shortcutPath}'); " +
                         $"$s.TargetPath = '{exePath}'; " +
                         $"$s.Save()";
        
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"{strCmdText}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        Process.Start(psi);
    }
}

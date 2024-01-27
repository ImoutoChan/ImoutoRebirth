using System.Diagnostics;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF.Commands;
using Microsoft.Win32;

namespace ImoutoViewer.ViewModel;

internal class OpenWithVM
{
    #region Constructors

    public OpenWithVM()
    {
        CurrentList = EditProgram.List.Where(x => x.IsAvailable).ToList();
    }

    #endregion Constructors

    public List<EditProgram> CurrentList { get; set; }

}

internal class EditProgram
{
    #region Constructors

    private EditProgram()
    {
        ClickCommand = new RelayCommand(Click);
    }

    #endregion Constructors

    #region Properties

    public bool IsAvailable { get; set; }

    public required string Name { get; init; }

    public required string RegistryPath { get; init; }

    public string? ExePath { get; set; }

    public required string IconPath { get; set; }

    #endregion  Properties

    #region Commands

    public ICommand ClickCommand { get; set; }

    private void Click(object? arg)
    {
        if (!(arg is string) || ExePath == null) 
            return;

        try
        {
            Process.Start(ExePath, "\"" + arg + "\"");
        }
        catch
        {
            // ignore
        }
    }

    #endregion  Commands

    static EditProgram()
    {
        var list = new List<EditProgram>
        {
            new EditProgram()
            {
                Name = "Photoshop",
                RegistryPath = @"Photoshop.exe\shell\edit\command",
                IconPath = @"pack://application:,,,/Resources/img/edit/photoshop.ico"
            },
            new EditProgram()
            {
                Name = "Paint",
                RegistryPath = @"mspaint.exe\shell\edit\command",
                IconPath = @"pack://application:,,,/Resources/img/edit/ms_paint.ico"
            },
            new EditProgram()
            {
                Name = "IrfanView",
                RegistryPath = @"i_view32.exe\shell\open\command",
                IconPath = @"pack://application:,,,/Resources/img/edit/irfan_view.ico"
            },

        };

        foreach (var entry in list)
        {
            RegistryKey? prog = Registry.ClassesRoot.OpenSubKey(@"Applications\" + entry.RegistryPath);
            if (prog != null)
            {
                entry.ExePath =
                    prog.GetValue("")!
                        .ToString()!
                        .Split(new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries)
                        .First();
                entry.IsAvailable = true;
            }
        }
        List = list;
    }

    public static List<EditProgram> List;
}

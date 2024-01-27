#nullable enable
namespace ImoutoRebirth.Common.WindowsAssociationManager;

public class ShellCommand : IDisposable
{
    private readonly FileAssociationManager _associationManager;
    private readonly string _shellRegistryPath;
    private bool _disposed;

    internal ShellCommand(
        FileAssociationManager associationManager,
        string parentclassid,
        string shellname,
        string displayname)
    {
        if (string.IsNullOrEmpty(shellname))
            throw new Exception("Name cannot be null or empty!");
        _associationManager = associationManager;
        _shellRegistryPath = parentclassid + "\\shell\\";
        ShellName = shellname;
        DisplayName = displayname;
    }

    public string ShellName { get; }

    public string? DisplayName
    {
        get
        {
            using var registryKey = _associationManager.GetClassesRoot().OpenSubKey(_shellRegistryPath + ShellName);
            return registryKey == null ? "" : registryKey.GetValue(null) as string;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("DisplayName cannot be null or empty!");
            using var subKey = _associationManager.GetClassesRoot().CreateSubKey(_shellRegistryPath + ShellName);
            subKey.SetValue(null, value);
        }
    }

    public string? Command
    {
        get
        {
            using var registryKey = _associationManager.GetClassesRoot()
                .OpenSubKey(_shellRegistryPath + ShellName + "\\command");
            return registryKey == null ? "" : registryKey.GetValue(null) as string;
        }
        set
        {
            using var subKey = _associationManager.GetClassesRoot()
                .CreateSubKey(_shellRegistryPath + ShellName + "\\command");

            if (string.IsNullOrEmpty(value))
            {
                subKey.DeleteValue(null!, false);
            }
            else
            {
                var str = value;
                if (!str.Contains("%1"))
                    str += " \"%1\"";
                subKey.SetValue(null, str);
            }
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    ~ShellCommand() => Dispose(true);

    public void Dispose(bool disposing)
    {
        lock (this)
        {
            if (_disposed)
                return;
            try
            {
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}

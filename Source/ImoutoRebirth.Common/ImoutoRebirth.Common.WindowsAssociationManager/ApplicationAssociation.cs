#nullable enable
namespace ImoutoRebirth.Common.WindowsAssociationManager;

public class ApplicationAssociation : IDisposable
{
    private readonly FileAssociationManager _associationManager;
    private readonly string _classid;
    private bool _disposed;

    internal ApplicationAssociation(
        FileAssociationManager associationManager,
        string extension,
        string classid,
        string description)
    {
        _associationManager = associationManager;
        Extension = "." + extension.TrimStart('.');
        _classid = classid;
        if (string.IsNullOrEmpty(description))
            return;
        using var subKey = _associationManager.GetClassesRoot().CreateSubKey(_classid);
        subKey.SetValue(null, description);
    }

    public string Extension { get; }

    public string? Description
    {
        get
        {
            using var registryKey = _associationManager.GetClassesRoot().OpenSubKey(_classid);

            return registryKey?.GetValue(null) as string;
        }
        set
        {
            using var subKey = _associationManager.GetClassesRoot().CreateSubKey(_classid);

            if (string.IsNullOrEmpty(value))
                subKey.DeleteValue(null!, false);
            else
                subKey.SetValue(null, value);
        }
    }

    public string? InfoTip
    {
        get
        {
            using var registryKey = _associationManager.GetClassesRoot().OpenSubKey(_classid);
            return registryKey == null ? null : registryKey.GetValue(nameof(InfoTip)) as string;
        }
        set
        {
            using var subKey = _associationManager.GetClassesRoot().CreateSubKey(_classid);
            if (string.IsNullOrEmpty(value))
                subKey.DeleteValue(nameof(InfoTip), false);
            else
                subKey.SetValue(nameof(InfoTip), value);
        }
    }

    public FileTypeAttributeFlags EditFlags
    {
        get
        {
            using var registryKey = _associationManager.GetClassesRoot().OpenSubKey(_classid);
            return registryKey == null || !(registryKey.GetValue(nameof(EditFlags)) is int num)
                ? FileTypeAttributeFlags.FTA_None
                : (FileTypeAttributeFlags)num;
        }
        set
        {
            using var subKey = _associationManager.GetClassesRoot().CreateSubKey(_classid);
            if (value == FileTypeAttributeFlags.FTA_None)
                subKey.DeleteValue(nameof(EditFlags), false);
            else
                subKey.SetValue(nameof(EditFlags), (int)value);
        }
    }

    public ApplicationIcon? DefaultIcon
    {
        get
        {
            using (var registryKey =
                   _associationManager.GetClassesRoot().OpenSubKey(_classid + "\\shellex\\IconHandler"))
            {
                if (registryKey != null)
                {
                    var iExtractIconGuid = registryKey.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(iExtractIconGuid))
                        return new ApplicationIcon(null, iExtractIconGuid);
                }
            }

            using (var registryKey = _associationManager.GetClassesRoot().OpenSubKey(_classid + "\\DefaultIcon"))
            {
                if (registryKey != null)
                {
                    var iconlibrarypath = registryKey.GetValue(null) as string;
                    var length = iconlibrarypath?.LastIndexOf(',');
                    if (length > 0 && iconlibrarypath != null)
                    {
                        var s = iconlibrarypath.Substring(length.Value + 1).Trim();
                        try
                        {
                            var iconindex = int.Parse(s);
                            iconlibrarypath = iconlibrarypath.Substring(0, length.Value).Trim();
                            if (iconlibrarypath.StartsWith("\"") && iconlibrarypath.EndsWith("\""))
                                iconlibrarypath = iconlibrarypath.Substring(1, iconlibrarypath.Length - 2);
                            return new ApplicationIcon(iconlibrarypath, iconindex);
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    return new ApplicationIcon(iconlibrarypath);
                }
            }

            return null;
        }
        set
        {
            using var subKey = _associationManager.GetClassesRoot().CreateSubKey(_classid + "\\DefaultIcon");
            if (value == null)
            {
                subKey.DeleteValue(null!, false);
            }
            else
            {
                var str = value.IconLibraryPath;
                if (value.IconIndex.HasValue)
                    str = str + ", " + value.IconIndex;
                subKey.SetValue(null, str);
            }
        }
    }

    public string? ShellOpenCommand
    {
        get
        {
            using var registryKey =
                _associationManager.GetClassesRoot().OpenSubKey(_classid + "\\Shell\\Open\\Command");
            return registryKey == null ? null : registryKey.GetValue(null) as string;
        }
        set
        {
            using var subKey = _associationManager.GetClassesRoot().CreateSubKey(_classid + "\\Shell\\Open\\Command");
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

    public bool Associated
    {
        get
        {
            if (_associationManager.ApplicationRegistration != null)
            {
                var pfDefault = false;
                return (_associationManager.ApplicationRegistration.QueryAppIsDefault(Extension,
                    ASSOCIATIONTYPE.AT_FILEEXTENSION, ASSOCIATIONLEVEL.AL_EFFECTIVE, _associationManager.ProductName,
                    out pfDefault) == 0) & pfDefault;
            }

            using var registryKey = _associationManager.GetClassesRoot().OpenSubKey(Extension);
            return registryKey != null && _classid.Equals(registryKey.GetValue(null) as string,
                StringComparison.CurrentCultureIgnoreCase);
        }
        set
        {
            if (_associationManager.ApplicationRegistration != null)
            {
                using var subKey = _associationManager.GetClassesRoot().CreateSubKey(Extension);
                string? ppszAssociation = null;
                if (_associationManager.ApplicationRegistration.QueryCurrentDefault(Extension,
                        ASSOCIATIONTYPE.AT_FILEEXTENSION, ASSOCIATIONLEVEL.AL_EFFECTIVE, out ppszAssociation) != 0)
                    ppszAssociation = null;
                if (value)
                {
                    if (_classid.Equals(ppszAssociation, StringComparison.CurrentCultureIgnoreCase))
                        return;
                    if (!string.IsNullOrEmpty(ppszAssociation))
                        subKey.SetValue("PrevDefault", ppszAssociation);
                    if (_associationManager.ApplicationRegistration.SetAppAsDefault(_associationManager.ProductName,
                            Extension, ASSOCIATIONTYPE.AT_FILEEXTENSION) != 0)
                    {
                    }
                }
                else
                {
                    if (!_classid.Equals(ppszAssociation, StringComparison.CurrentCultureIgnoreCase))
                        return;
                    var keyvalue = subKey.GetValue("PrevDefault") as string;
                    if (!string.IsNullOrEmpty(keyvalue))
                    {
                        if (_associationManager.ApplicationRegistration.SetAppAsDefault(
                                GetAssociatedProgram(keyvalue, true), Extension,
                                ASSOCIATIONTYPE.AT_FILEEXTENSION) != 0)
                        {
                        }

                        subKey.DeleteValue("PrevDefault", false);
                    }
                    else if (_associationManager.ApplicationRegistration.SetAppAsDefault(
                                 GetAssociatedProgram(Extension, true), Extension,
                                 ASSOCIATIONTYPE.AT_FILEEXTENSION) != 0)
                    {
                    }
                }
            }
            else
            {
                using var subKey = _associationManager.GetClassesRoot().CreateSubKey(Extension);
                var str1 = subKey.GetValue(null) as string;
                if (value)
                {
                    if (_classid.Equals(str1, StringComparison.CurrentCultureIgnoreCase))
                        return;
                    if (!string.IsNullOrEmpty(str1))
                        subKey.SetValue("PrevDefault", str1);
                    subKey.SetValue(null, _classid);
                }
                else
                {
                    if (!_classid.Equals(str1, StringComparison.CurrentCultureIgnoreCase))
                        return;
                    var str2 = subKey.GetValue("PrevDefault") as string;
                    if (!string.IsNullOrEmpty(str2))
                    {
                        subKey.SetValue(null, str2);
                        subKey.DeleteValue("PrevDefault", false);
                    }
                    else
                        subKey.DeleteValue(null!, false);
                }
            }
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    ~ApplicationAssociation() => Dispose(true);

    public void Dispose(bool disposing)
    {
        lock (this)
        {
            if (_disposed)
                return;
            try
            {
                if (!disposing)
                {
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    private bool HasValue(Dictionary<string, string> dictionary, string keyvalue, bool isvalue)
    {
        foreach (var keyValuePair in dictionary)
        {
            if (isvalue)
            {
                if (keyValuePair.Value.Equals(keyvalue, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            else if (keyValuePair.Key.Equals(keyvalue, StringComparison.CurrentCultureIgnoreCase))
                return true;
        }

        return false;
    }

    private string? GetAssociatedProgram(string keyvalue, bool isclassid)
    {
        foreach (var registeredProgram in _associationManager.GetRegisteredPrograms())
        {
            if (HasValue(registeredProgram.Extensions, keyvalue, isclassid) ||
                HasValue(registeredProgram.Mimes, keyvalue, isclassid))
                return registeredProgram.Name;
        }

        return null;
    }

    public ShellCommand RegisterShellCommand(string shellname, string displayname)
    {
        return RegisterShellCommand(shellname, displayname, null);
    }

    public ShellCommand RegisterShellCommand(string shellname, string displayname, string? command)
    {
        var shellCommand = new ShellCommand(_associationManager, _classid, shellname, displayname);
        if (command != null)
            shellCommand.Command = command;
        return shellCommand;
    }
}

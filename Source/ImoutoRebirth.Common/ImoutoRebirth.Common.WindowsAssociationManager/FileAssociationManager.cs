using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ImoutoRebirth.Common.WindowsAssociationManager;

public class FileAssociationManager : IDisposable
{
    public enum AssociationLocation
    {
        LocalMachine,
        CurrentUser
    }

    private readonly string _applicationDescription;
    private readonly string _companyName;
    private readonly AssociationLocation _location = AssociationLocation.LocalMachine;
    private bool _disposed;
    private bool _modified;
    private IEnumerable<RegisteredProgram> _registeredPrograms;

    public FileAssociationManager(
        AssociationLocation location = AssociationLocation.LocalMachine)
        : this(location, null, null, null)
    {
    }

    public FileAssociationManager(
        string companyname,
        string productname,
        string applicationdescription)
        : this(AssociationLocation.LocalMachine, companyname, productname, applicationdescription)
    {
    }

    public FileAssociationManager(
        AssociationLocation location,
        string companyname,
        string productname,
        string applicationdescription)
    {
        _location = location;
        if (string.IsNullOrEmpty(companyname))
            throw new Exception("Company name cannot be null or empty!");
        if (string.IsNullOrEmpty(productname))
            throw new Exception("Product name cannot be null or empty!");

        if (string.IsNullOrEmpty(applicationdescription))
        {
            applicationdescription =
                ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(),
                    typeof(AssemblyDescriptionAttribute))).Description;
            if (string.IsNullOrEmpty(applicationdescription))
                applicationdescription = productname;
        }

        _companyName = companyname;
        ProductName = productname;
        _applicationDescription = applicationdescription;
        if (!IsDefaultProgramsAvailable())
            return;
        ApplicationRegistration = (IApplicationAssociationRegistration)new ApplicationAssociationRegistration();
    }

    internal IApplicationAssociationRegistration ApplicationRegistration { get; private set; }

    internal string ProductName { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern void SHChangeNotify(
        int wEventId,
        int uFlags,
        IntPtr dwItem1,
        IntPtr dwItem2);

    private bool IsDefaultProgramsAvailable() => Environment.OSVersion.Version.Major >= 6;

    private string GetDefaultCompanyKeyPath()
    {
        return string.Format("Software\\{0}\\", _companyName);
    }

    private string GetDefaultProducKeyPath()
    {
        return GetDefaultCompanyKeyPath() + ProductName + "\\";
    }

    private string GetDefaultCapabilitiesKeyPath()
    {
        return GetDefaultProducKeyPath() + "Capabilities\\";
    }

    private RegistryKey GetRegistry()
    {
        return _location == AssociationLocation.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;
    }

    internal RegistryKey GetClassesRoot() => Registry.ClassesRoot;

    private string GetCapabilitiesLocation()
    {
        var capabilitiesKeyPath = GetDefaultCapabilitiesKeyPath();
        var str = capabilitiesKeyPath;
        if (IsDefaultProgramsAvailable())
        {
            using var subKey = GetRegistry().CreateSubKey("Software\\RegisteredApplications");
            str = subKey.GetValue(ProductName) as string;
            if (string.IsNullOrEmpty(str))
            {
                str = capabilitiesKeyPath.TrimEnd('\\');
                subKey.SetValue(ProductName, str);
            }
        }

        var subkey = str.TrimEnd('\\', '/') + "\\";
        using (var subKey = GetRegistry().CreateSubKey(subkey))
        {
            if (subKey.GetValue("ApplicationDescription") == null)
                subKey.SetValue("ApplicationDescription", _applicationDescription);
        }

        return subkey;
    }

    private void RemoveAssociatedClassIds(
        RegistryKey capabilitieskey,
        string subkeyname,
        string classidprefix)
    {
        classidprefix = ProductName + "." + classidprefix + ".";
        using var registryKey = capabilitieskey.OpenSubKey(subkeyname);
        if (registryKey == null)
            return;
        foreach (var valueName in registryKey.GetValueNames())
        {
            var str = registryKey.GetValue(valueName) as string;
            if (!string.IsNullOrEmpty(str) &&
                str.StartsWith(classidprefix, StringComparison.CurrentCultureIgnoreCase))
            {
                using (var applicationAssociation = new ApplicationAssociation(this, valueName, str, null))
                    applicationAssociation.Associated = false;
                GetClassesRoot().DeleteSubKeyTree(str, false);
            }
        }
    }

    public void UnregisterApplicationAssociation() => UnregisterApplicationAssociation(true);

    public void UnregisterApplicationAssociation(bool removeregistrykeys)
    {
        _modified = true;
        string name = null;
        if (IsDefaultProgramsAvailable())
        {
            using var registryKey = GetRegistry().OpenSubKey("Software\\RegisteredApplications", true);
            if (registryKey != null)
            {
                name = registryKey.GetValue(ProductName) as string;
                registryKey.DeleteValue(ProductName, false);
            }
        }

        if (string.IsNullOrEmpty(name))
            name = GetDefaultCapabilitiesKeyPath();
        using (var capabilitieskey = GetRegistry().OpenSubKey(name, true))
        {
            if (capabilitieskey != null)
            {
                RemoveAssociatedClassIds(capabilitieskey, "FileAssociations", "AssocFile");
                RemoveAssociatedClassIds(capabilitieskey, "MimeAssociations", "MIME");
                capabilitieskey.DeleteSubKey("FileAssociations", false);
                capabilitieskey.DeleteSubKey("MimeAssociations", false);
            }
        }

        if (!removeregistrykeys)
            return;
        var defaultCompanyKeyPath = GetDefaultCompanyKeyPath();
        if (name.StartsWith(defaultCompanyKeyPath, StringComparison.CurrentCultureIgnoreCase))
        {
            var flag = true;
            using (var registryKey = GetRegistry().OpenSubKey(defaultCompanyKeyPath, true))
            {
                if (registryKey != null)
                {
                    registryKey.DeleteSubKeyTree(ProductName, false);
                    var subKeyNames = registryKey.GetSubKeyNames();
                    var valueNames = registryKey.GetValueNames();
                    if (subKeyNames.Length != 0 || valueNames.Length != 0)
                        flag = false;
                }
            }

            if (flag)
            {
                using var registryKey = GetRegistry().OpenSubKey("Software", true);
                registryKey?.DeleteSubKey(_companyName, false);
            }
        }
    }

    internal IEnumerable<RegisteredProgram> GetRegisteredPrograms()
    {
        if (_registeredPrograms == null)
            _registeredPrograms = RegisteredProgram.GetRegisteredPrograms();
        return _registeredPrograms;
    }

    public ApplicationAssociation RegisterFileAssociation(string extension)
    {
        return RegisterFileAssociation(extension, null);
    }

    public ApplicationAssociation RegisterFileAssociation(string extension, string description)
    {
        _modified = true;
        extension = extension.TrimStart('.');
        if (string.IsNullOrEmpty(extension))
            throw new Exception("Extension cannot be null or empty!");
        if (string.IsNullOrEmpty(description))
            description = string.Format("{0} {1} File", ProductName, extension.ToUpper());
        using var subKey = GetRegistry().CreateSubKey(GetCapabilitiesLocation() + "FileAssociations");
        var classid = string.Format("{0}.AssocFile.{1}", ProductName, extension);
        subKey.SetValue("." + extension, classid);
        return new ApplicationAssociation(this, extension, classid, description);
    }

    public ApplicationAssociation GetFileAssociation(string extension)
    {
        extension = extension.TrimStart('.');
        if (string.IsNullOrEmpty(extension))
            throw new Exception("Extension cannot be null or empty!");
        using var registryKey = GetClassesRoot().OpenSubKey("." + extension);
        var classid = registryKey?.GetValue(null)?.ToString();
        return !string.IsNullOrEmpty(classid)
            ? new ApplicationAssociation(this, extension, classid, null)
            : throw new Exception("Unhandled extension configuration. The extensions classid is missing.");
    }

    private ApplicationAssociation RegisterMimeAssociation(string contenttype, string subtype)
    {
        return RegisterMimeAssociation(contenttype, subtype, null);
    }

    private ApplicationAssociation RegisterMimeAssociation(
        string contenttype,
        string subtype,
        string description)
    {
        throw new Exception("Mime association has not been tested properly yet!");
    }

    public ShellCommand RegisterDirectoryShellCommand(string shellname, string displayname)
    {
        return RegisterDirectoryShellCommand(shellname, displayname, null);
    }

    public ShellCommand RegisterDirectoryShellCommand(
        string shellname,
        string displayname,
        string command)
    {
        var shellCommand = new ShellCommand(this, "Directory", shellname, displayname);
        if (command != null)
            shellCommand.Command = command;
        return shellCommand;
    }

    ~FileAssociationManager() => Dispose(true);

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

                ApplicationRegistration = null;
                if (_modified)
                    SHChangeNotify(134217728, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}

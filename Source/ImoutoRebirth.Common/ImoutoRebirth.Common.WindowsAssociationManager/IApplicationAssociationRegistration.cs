using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ImoutoRebirth.Common.WindowsAssociationManager;

[ComVisible(true)]
[Guid("4e530b0a-e611-4c77-a3ac-9031d022281b")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComImport]
internal interface IApplicationAssociationRegistration
{
    [MethodImpl(MethodImplOptions.PreserveSig)]
    int QueryCurrentDefault(
        [MarshalAs(UnmanagedType.LPWStr)] [In] string pszQuery,
        [MarshalAs(UnmanagedType.I4)] [In] ASSOCIATIONTYPE atQueryType,
        [MarshalAs(UnmanagedType.I4)] [In] ASSOCIATIONLEVEL alQueryLevel,
        [MarshalAs(UnmanagedType.LPWStr)] out string ppszAssociation);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int QueryAppIsDefault(
        [MarshalAs(UnmanagedType.LPWStr)] [In] string pszQuery,
        [In] ASSOCIATIONTYPE atQueryType,
        [In] ASSOCIATIONLEVEL alQueryLevel,
        [MarshalAs(UnmanagedType.LPWStr)] [In] string pszAppRegistryName,
        out bool pfDefault);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int QueryAppIsDefaultAll(
        [In] ASSOCIATIONLEVEL alQueryLevel,
        [MarshalAs(UnmanagedType.LPWStr)] [In] string pszAppRegistryName,
        out bool pfDefault);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int SetAppAsDefault(
        [MarshalAs(UnmanagedType.LPWStr)] [In] string pszAppRegistryName,
        [MarshalAs(UnmanagedType.LPWStr)] [In] string pszSet, [In] ASSOCIATIONTYPE atSetType);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int SetAppAsDefaultAll([MarshalAs(UnmanagedType.LPWStr)] [In] string pszAppRegistryName);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int ClearUserAssociations();
}

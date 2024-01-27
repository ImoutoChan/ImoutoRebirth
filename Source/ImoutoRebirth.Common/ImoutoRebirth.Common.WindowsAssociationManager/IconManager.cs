using System.Drawing;
using System.Runtime.InteropServices;

namespace ImoutoRebirth.Common.WindowsAssociationManager;

public class IconManager
{
    public enum IconSize
    {
        Small,

        Large
    }


    private readonly ApplicationIcon _applicationIcon;


    private readonly string _libaryPath;


    public IconManager(ApplicationIcon icon)
    {
        _applicationIcon = icon;
        var flag = _applicationIcon == null;
        if (!flag)
        {
            _libaryPath = icon.IconLibraryPath;
            var flag2 = !string.IsNullOrEmpty(_libaryPath);
            if (flag2)
            {
                Count = (int)Shell32.ExtractIconEx(_libaryPath, -1, null, null, 0U);
            }
        }
    }


    public IconManager(string libaryPath)
    {
        _libaryPath = libaryPath;
        Count = (int)Shell32.ExtractIconEx(_libaryPath, -1, null, null, 0U);
    }


    public int Count { get; private set; }


    public Icon GetIcon(IconSize iconSize, string filename)
    {
        var applicationIcon = _applicationIcon;
        var flag = !string.IsNullOrEmpty(applicationIcon != null ? applicationIcon.IExtractIconGuid : null) &&
                   !string.IsNullOrEmpty(filename);
        if (flag)
        {
            var shfileinfo = default(Shell32.SHFILEINFO);
            try
            {
                var num = 272U;
                if (iconSize != IconSize.Small)
                {
                    if (iconSize == IconSize.Large)
                    {
                        num |= 0U;
                    }
                }
                else
                {
                    num |= 1U;
                }

                Shell32.SHGetFileInfo(filename, 0U, ref shfileinfo, (uint)Marshal.SizeOf(shfileinfo), num);
                return (Icon)Icon.FromHandle(shfileinfo.hIcon).Clone();
            }
            catch (Exception innerException)
            {
                throw new Exception("Failed to extract icon", innerException);
            }
            finally
            {
                try
                {
                    var flag2 = true;
                    if (flag2)
                    {
                        Shell32.DestroyIcon(shfileinfo.hIcon);
                    }
                }
                catch
                {
                }
            }
        }

        var applicationIcon2 = _applicationIcon;
        return GetIcon(iconSize, applicationIcon2 != null ? applicationIcon2.IconIndex : null);
    }


    public Icon GetIcon(IconSize iconSize, int? iconIndex)
    {
        var flag = iconIndex == null;
        if (flag)
        {
            iconIndex = 0;
        }

        var num = 0U;
        IntPtr[] array =
        {
            IntPtr.Zero
        };
        IntPtr[] array2 =
        {
            IntPtr.Zero
        };
        try
        {
            if (iconSize != IconSize.Small)
            {
                if (iconSize == IconSize.Large)
                {
                    num = Shell32.ExtractIconEx(_libaryPath, iconIndex.Value, array2, array, 1U);
                }
            }
            else
            {
                num = Shell32.ExtractIconEx(_libaryPath, iconIndex.Value, array, array2, 1U);
            }

            var flag2 = num > 0U && array2[0] != IntPtr.Zero;
            if (flag2)
            {
                return (Icon)Icon.FromHandle(array2[0]).Clone();
            }
        }
        catch (Exception innerException)
        {
            throw new Exception("Failed to extract icon", innerException);
        }
        finally
        {
            foreach (var intPtr in array2)
            {
                var flag3 = intPtr == IntPtr.Zero;
                if (!flag3)
                {
                    try
                    {
                        Shell32.DestroyIcon(intPtr);
                    }
                    catch
                    {
                    }
                }
            }

            foreach (var intPtr2 in array)
            {
                var flag4 = intPtr2 == IntPtr.Zero;
                if (!flag4)
                {
                    try
                    {
                        Shell32.DestroyIcon(intPtr2);
                    }
                    catch
                    {
                    }
                }
            }
        }

        return null;
    }


    private static class Shell32
    {
        public const uint SHGFI_ICON = 256U;
        public const uint SHGFI_DISPLAYNAME = 512U;
        public const uint SHGFI_TYPENAME = 1024U;
        public const uint SHGFI_ATTRIBUTES = 2048U;
        public const uint SHGFI_ICONLOCATION = 4096U;
        public const uint SHGFI_EXETYPE = 8192U;
        public const uint SHGFI_SYSICONINDEX = 16384U;
        public const uint SHGFI_LINKOVERLAY = 32768U;
        public const uint SHGFI_SELECTED = 65536U;
        public const uint SHGFI_ATTR_SPECIFIED = 131072U;
        public const uint SHGFI_LARGEICON = 0U;
        public const uint SHGFI_SMALLICON = 1U;
        public const uint SHGFI_OPENICON = 2U;
        public const uint SHGFI_SHELLICONSIZE = 4U;
        public const uint SHGFI_PIDL = 8U;
        public const uint SHGFI_USEFILEATTRIBUTES = 16U;
        public const uint SHGFI_ADDOVERLAYS = 32U;
        public const uint SHGFI_OVERLAYINDEX = 64U;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 16U;
        public const uint FILE_ATTRIBUTE_NORMAL = 128U;

        private const int MAX_PATH = 260;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern uint ExtractIconEx(
            string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("Shell32.dll")]
        public static extern IntPtr SHGetFileInfo(
            string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public struct SHFILEINFO
        {
            public const int NAMESIZE = 80;
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }
}

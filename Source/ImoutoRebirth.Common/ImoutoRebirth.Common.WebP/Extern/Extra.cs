using System.Runtime.InteropServices;

namespace ImoutoRebirth.Common.WebP.Extern;

public partial class NativeMethods
{
    public static void WebPSafeFree(IntPtr toDeallocate)
    {
        WebPFree(toDeallocate);
    }


    [DllImport("libwebp", EntryPoint = "WebPFree", CallingConvention = CallingConvention.Cdecl)]
    public static extern void WebPFree(IntPtr toDeallocate);
}

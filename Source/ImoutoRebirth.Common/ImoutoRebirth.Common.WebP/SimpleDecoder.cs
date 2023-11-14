using System.Drawing;
using System.Drawing.Imaging;
using NativeMethods = ImoutoRebirth.Common.WebP.Extern.NativeMethods;

namespace ImoutoRebirth.Common.WebP;

public class SimpleDecoder
{
    /// <summary>
    ///     Creates a new instance of SimpleDecoder
    /// </summary>
    public SimpleDecoder()
    {
    }


    public static string GetDecoderVersion()
    {
        var v = (uint)NativeMethods.WebPGetDecoderVersion();
        var revision = v % 256;
        var minor = (v >> 8) % 256;
        var major = (v >> 16) % 256;
        return major + "." + minor + "." + revision;
    }


    public unsafe Bitmap DecodeFromBytes(byte[] data, long length)
    {
        fixed (byte* dataptr = data)
        {
            return DecodeFromPointer((IntPtr)dataptr, length);
        }
    }

    public Bitmap DecodeFromPointer(IntPtr data, long length)
    {
        int w = 0, h = 0;
        //Validate header and determine size
        if (NativeMethods.WebPGetInfo(data, (UIntPtr)length, ref w, ref h) == 0)
            throw new Exception("Invalid WebP header detected");

        var success = false;
        Bitmap? b = null;
        BitmapData? bd = null;
        try
        {
            //Allocate canvas
            b = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            //Lock surface for writing
            bd = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            //Decode to surface
            var result = NativeMethods.WebPDecodeBGRAInto(data, (UIntPtr)length, bd.Scan0,
                (UIntPtr)(bd.Stride * bd.Height), bd.Stride);
            if (bd.Scan0 != result) throw new Exception("Failed to decode WebP image with error " + (long)result);
            success = true;
        }
        finally
        {
            //Unlock surface
            if (bd != null && b != null) b.UnlockBits(bd);
            //Dispose of bitmap if anything went wrong
            if (!success && b != null) b.Dispose();
        }

        return b;
    }
}

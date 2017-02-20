using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Noterium.Core.Helpers
{
    public class ClipboardHelper
    {
        public static Image GetImageFromClipboard()
        {
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null)
                return null;

            if (dataObject.GetDataPresent(DataFormats.Dib))
            {
                var dib = ((MemoryStream) Clipboard.GetData(DataFormats.Dib)).ToArray();
                var width = BitConverter.ToInt32(dib, 4);
                var height = BitConverter.ToInt32(dib, 8);
                var bpp = BitConverter.ToInt16(dib, 14);
                if (bpp == 32)
                {
                    var gch = GCHandle.Alloc(dib, GCHandleType.Pinned);
                    Bitmap bmp = null;
                    try
                    {
                        var ptr = new IntPtr((long) gch.AddrOfPinnedObject() + 40);
                        bmp = new Bitmap(width, height, width*4, PixelFormat.Format32bppArgb, ptr);

                        var newBmp = new Bitmap(bmp);
                        newBmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                        return newBmp;
                    }
                    finally
                    {
                        gch.Free();
                        bmp?.Dispose();
                    }
                }
            }

            return Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
        }
    }
}
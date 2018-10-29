using System.Linq;
using Microsoft.Win32;

namespace Noterium.Core.Helpers
{
    public class MimeType
    {
        private static readonly byte[] BMP = {66, 77};
        private static readonly byte[] GIF = {71, 73, 70, 56};
        private static readonly byte[] ICO = {0, 0, 1, 0};
        private static readonly byte[] JPG = {255, 216, 255};
        private static readonly byte[] PNG = {137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82};
        private static readonly byte[] TIFF = {73, 73, 42, 0};

        public static string GetMimeType(byte[] file, string defaultMime = "application/octet-stream")
        {
            var mime = defaultMime; //DEFAULT UNKNOWN MIME TYPE

            if (file.Take(2).SequenceEqual(BMP))
                mime = "image/bmp";
            else if (file.Take(4).SequenceEqual(GIF))
                mime = "image/gif";
            else if (file.Take(4).SequenceEqual(ICO))
                mime = "image/x-icon";
            else if (file.Take(3).SequenceEqual(JPG))
                mime = "image/jpeg";
            else if (file.Take(16).SequenceEqual(PNG))
                mime = "image/png";
            else if (file.Take(4).SequenceEqual(TIFF)) mime = "image/tiff";

            return mime;
        }

        public static string GetDefaultExtension(string mimeType)
        {
            var key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            var value = key?.GetValue("Extension", null);
            var result = value?.ToString() ?? string.Empty;

            return result;
        }

        public static bool IsImageExtension(string ext)
        {
            switch (ext)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".bmp":
                    return true;
            }

            return false;
        }
    }
}
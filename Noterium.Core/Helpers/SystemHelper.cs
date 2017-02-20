using System.Drawing;
using System.Runtime.InteropServices;

namespace Noterium.Core.Helpers
{
    public class SystemHelper
    {
        private const int SM_CXDRAG = 68;
        private const int SM_CYDRAG = 69;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        public static Point GetDragThreshold()
        {
            return new Point(GetSystemMetrics(SM_CXDRAG), GetSystemMetrics(SM_CYDRAG));
        }
    }
}
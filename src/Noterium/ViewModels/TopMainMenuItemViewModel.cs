using System.Windows.Controls;
using System.Windows.Media;

namespace Noterium.ViewModels
{
    public class TopMainMenuItemViewModel
    {
        public TopMainMenuItemViewModel()
        {
            IconColor = Brushes.Black;
        }

        public string Name { get; set; }
        public string ToolTip { get; set; }
        public Canvas Icon { get; set; }
        public SolidColorBrush IconColor { get; set; }
    }
}
using System.Windows;

namespace Noterium.Code.Converters
{
    public sealed class BindingNullToVisibilityConverter : BindingNullConverter<Visibility>
    {
        public BindingNullToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        {
        }
    }
}
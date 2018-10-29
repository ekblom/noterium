using MahApps.Metro.Controls.Dialogs;

namespace Noterium.Code.Helpers
{
    public static class DialogHelpers
    {
        public static MetroDialogSettings GetDefaultDialogSettings()
        {
            var settings = new MetroDialogSettings();
            settings.AnimateShow = false;
            settings.AnimateHide = false;
            settings.DefaultButtonFocus = MessageDialogResult.Negative;
            settings.ColorScheme = MetroDialogColorScheme.Theme;
            return settings;
        }
    }
}
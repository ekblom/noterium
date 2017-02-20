using MahApps.Metro.Controls.Dialogs;

namespace Noterium.Code.Helpers
{
    public static class DialogHelpers
    {
        public static MetroDialogSettings GetDefaultDialogSettings()
        {
            MetroDialogSettings settings = new MetroDialogSettings();
            settings.AnimateShow = false;
            settings.AnimateHide = false;
            settings.DefaultButtonFocus = MessageDialogResult.Negative;
            return settings;
        }


    }
}
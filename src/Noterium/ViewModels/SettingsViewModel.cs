using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro;
using Noterium.Code.Commands;
using Noterium.Code.Extensions;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Core.Security;
using Noterium.Windows;

namespace Noterium.ViewModels
{
	public class SettingsViewModel : NoteriumViewModelBase
	{
		private bool _secureNotesEnabled;
		private string _enteredPassword;
		private string _lastBackupDate;
	    private List<AppThemeMenuData> _appThemes;
	    private List<AccentColorMenuData> _accentColors;
	    private AccentColorMenuData _selectedAccent;
	    private AccentColorMenuData _selectedTheme;
	    private List<Tag> _tags;
	    public Settings Settings { get; }

		public ICommand SaveSettingsCommand { get; set; }
		public ICommand ChangeThemeCommand { get; set; }
		public ICommand BackupNowCommand { get; set; }
		public ICommand TurnOnEncryptionCommand { get; set; }
		public ICommand TurnOffEncryptionCommand { get; set; }
		public ICommand ApplyThemeCommand { get; set; }
		public ICommand ShowAboutWindowCommand { get; set; }
		public ICommand OpenBackupManagerCommand { get; set; }

		private IEnumerable<string> _noteViewModes;
        public IEnumerable<string> NoteViewModes
        {
            get { return _noteViewModes; }
            set
            {
                _noteViewModes = value; RaisePropertyChanged();
            }
        }

        private IEnumerable<string> _themes;
        public IEnumerable<string> Themes
        {
            get { return _themes; }
            set
            {
				_themes = value; RaisePropertyChanged();
            }
        }


        public List<AppThemeMenuData> AppThemes
        {
            get { return _appThemes; }
            set { _appThemes = value; RaisePropertyChanged(); }
        }

	    public List<AccentColorMenuData> AccentColors
	    {
	        get { return _accentColors; }
	        set { _accentColors = value; RaisePropertyChanged(); }
	    }

	    public bool SecureNotesEnabled
		{
			get { return _secureNotesEnabled; }
			set { _secureNotesEnabled = value; RaisePropertyChanged(); }
		}

	    public string EnteredPassword
		{
			get { return _enteredPassword; }
			set { _enteredPassword = value; RaisePropertyChanged(); }
		}

	    public string LastBackupDate
		{
			get { return _lastBackupDate; }
			set { _lastBackupDate = value; RaisePropertyChanged(); }
		}

	    public AccentColorMenuData SelectedTheme
	    {
	        get { return _selectedTheme; }
	        set { _selectedTheme = value; RaisePropertyChanged(); TestTheme(); }
	    }

	    public AccentColorMenuData SelectedAccent
	    {
	        get { return _selectedAccent; }
	        set { _selectedAccent = value; RaisePropertyChanged();
                TestTheme();
	        }
	    }

	    public List<Tag> Tags
	    {
	        get { return _tags; }
	        set { _tags = value; RaisePropertyChanged(); }
	    }

	    public SettingsViewModel()
		{
			Settings = Hub.Instance.Settings;

	        List<string> temp = new List<string> {"View", "Split", "Edit"};
	        NoteViewModes = temp;

			ShowAboutWindowCommand = new SimpleCommand(ShowAboutWindow);
			ChangeThemeCommand = new RelayCommand(ChangeTheme);
			SaveSettingsCommand = new RelayCommand(SaveSettings);
			OpenBackupManagerCommand = new RelayCommand(OpenBackupManager);
			BackupNowCommand = new RelayCommand(Backup);
			TurnOnEncryptionCommand = new SimpleCommand(TurnOnEncryption);
			TurnOffEncryptionCommand = new SimpleCommand(TurnOffEncryption);
            SecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;
			InitBackupLabel();

		    //Themes = new List<string> {"Dark", "Light"};

			//AccentColors = ThemeManager.Accents
			//					.Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
			//					.ToList();

			AccentColors = new List<AccentColorMenuData>
			{
				new AccentColorMenuData {AccentName = "VSDark", Name = "Dark"},
				new AccentColorMenuData {AccentName = "VSLight", Name = "Light"}
			};

			// create metro theme color menu items for the demo
			//AppThemes = ThemeManager.AppThemes
			//								.Where(a => a.Name == "VSDark")
			//							   .Select(a => new AppThemeMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
			//							   .ToList();

			//   AppThemes = ThemeManager.AppThemes.Select(a => new AppThemeMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
			//                                  .ToList();

			//   AccentColors = ThemeManager.Accents.Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
			//                                   .ToList();

			SelectedAccent = AccentColors.FirstOrDefault(a => a.Name == Settings.Accent);
			//SelectedTheme = AppThemes.FirstOrDefault(a => a.Name == Settings.Theme);
		}

		private void ShowAboutWindow(object arg)
		{
			AboutWindow win = new AboutWindow { DataContext = new AboutWindowViewModel() };
			win.ShowDialog();
		}

		private void OpenBackupManager()
		{
			BackupManager manager = new BackupManager();
			manager.Loaded += (object win, RoutedEventArgs ee) => {
				var ctx = manager.DataContext as BackupManagerViewModel;
				ctx.Window = manager;
			};

			manager.ShowDialog();
		}

		private void ChangeTheme()
		{
			InvokeOnCurrentDispatcher(() =>
			{
				Settings.Accent = SelectedAccent.AccentName;
				var accent = ThemeManager.GetAccent(Settings.Accent);
				var theme = ThemeManager.GetAppTheme("BaseDark");
				ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
				foreach (Window window in Application.Current.Windows)
					ThemeManager.ChangeAppStyle(window, accent, theme);
			});
		}

		private void TestTheme()
	    {
	        if (SelectedTheme == null || SelectedAccent == null)
	            return;

            var appTheme = ThemeManager.GetAppTheme(SelectedTheme.Name);
            var accent = ThemeManager.GetAccent(SelectedAccent.Name);

            Settings.Theme = SelectedTheme.Name;
            Settings.Accent = SelectedAccent.Name;
            Settings.Save();

            InvokeOnCurrentDispatcher(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    ThemeManager.ChangeAppStyle(window, accent, appTheme);
                }
            });
	    }

	    private void InitBackupLabel()
		{
			DateTime lastBackup = Hub.Instance.Storage.GetLastBackupDate();
			if (lastBackup > DateTime.MinValue)
				LastBackupDate = lastBackup.ToString(CultureInfo.CurrentCulture);
		}

		private void TurnOffEncryption(object o)
		{
            object[] objects = (object[])o;
            if (objects.Length != 2)
                return;

            PasswordBox pass1 = (PasswordBox)objects[0];
            PasswordBox pass2 = (PasswordBox)objects[1];

            if (!VerifyPassword(pass1, pass2))
                return;

            if (!Hub.Instance.EncryptionManager.ValidatePassword(pass1.SecurePassword))
            {
                MessageBox.Show("Incorrect password.", "Incorrect password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("This will decrypt all your secure notes.\n\nDo you want to continue?", "Turn of secure notes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
                Hub.Instance.EncryptionManager.DisableSecureNotes(pass1.SecurePassword);
                SecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;
			    pass1.Password = string.Empty;
			    pass2.Password = string.Empty;
			}
		}

	    private static bool VerifyPassword(PasswordBox pass1, PasswordBox pass2)
	    {
	        if (pass1.SecurePassword.Length < AESGCM.MinPasswordLength)
	        {
	            MessageBox.Show($"The password is to short, it needs to be at least {AESGCM.MinPasswordLength} characters long.", "Short password", MessageBoxButton.OK, MessageBoxImage.Information);
	            return false;
	        }

	        if (!pass1.SecurePassword.IsEqualTo(pass2.SecurePassword))
	        {
	            MessageBox.Show("The passwords you entered does not match, retype the passwords and try again.", "Password missmatch", MessageBoxButton.OK, MessageBoxImage.Information);
	            return false;
	        }
	        return true;
	    }

	    private void TurnOnEncryption(object o)
		{
		    object[] objects = (object[]) o;
		    if (objects.Length != 2)
		        return;

            PasswordBox pass1 = (PasswordBox)objects[0];
            PasswordBox pass2 = (PasswordBox)objects[1];

            if (!VerifyPassword(pass1, pass2))
                return;

            Hub.Instance.EncryptionManager.EnableSecureNotes(pass1.SecurePassword);
			SecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;
		}

		private void Backup()
		{
			Hub.Instance.Storage.Backup();
			InitBackupLabel();
		}

		private void SaveSettings()
		{
			Settings.Save();
		}

        public class AccentColorMenuData
        {
            public string Name { get; set; }
            public Brush BorderColorBrush { get; set; }
            public Brush ColorBrush { get; set; }
			public string AccentName { get; set; }

	        public override string ToString()
	        {
		        return Name;
	        }
        }
        public class AppThemeMenuData : AccentColorMenuData
        {

        }
    }
}
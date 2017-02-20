using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using log4net;
using MahApps.Metro;
using Noterium.Code.Commands;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.ViewModels;
using Noterium.Views.Dialogs;

namespace Noterium
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private MainWindow _mainWindow;
		private bool _mainWindowLoaded;
		private static ILog _log;
		private DispatcherTimer _activityTimer;
		private Point _inactiveMousePosition = new Point(0, 0);

		public App()
		{
			var currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_log.Error(e.ExceptionObject);
		}

		public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			_log.Error(e.Exception.Message, e.Exception);
		}


		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			//TODO: Flytta från registry
			//TODO: Lägga trail-license i en annan fil i appdata katalogen
			ThemeManager.AddAccent("VSDark", new Uri("pack://application:,,,/Resources/VSDark.xaml"));
			ThemeManager.AddAccent("VSLight", new Uri("pack://application:,,,/Resources/VSLight.xaml"));

			try
			{
				Hub.Instance.AppSettings.Init();
			}
			catch (InvalidConfigurationFileException)
			{
				var result = MessageBox.Show("The configuration file seems to be corrupt. Do you want me to recreate it with default settings for you?", "Corrupt configuration file", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					Hub.Instance.AppSettings.Save();
				}
				else
				{
					Current.Shutdown();
					return;
				}
			}
			catch (Exception ex)
			{

				MessageBox.Show(ex.ToString(), ex.Message);
				Current.Shutdown();
				return;
			}

			if (!Hub.Instance.AppSettings.Librarys.Any())
			{
				var oldType = Hub.Instance.Storage.GetStorageType();
				if (oldType != StorageType.Undefined)
				{
					string path = Hub.Instance.Storage.GetStoragePath();
					string name = Path.GetFileName(path);
					Hub.Instance.AppSettings.Librarys.Add(new Library
					{
						Name = name,
						Path = path,
						StorageType = oldType
					});

					Hub.Instance.AppSettings.Save();
				}
			}

			if (!Hub.Instance.AppSettings.Librarys.Any())
			{
				StorageSelector selector = new StorageSelector();
				bool? result = selector.ShowDialog();
				if (result == null || !result.Value)
				{
					Current.Shutdown(1);
					return;
				}
			}

			Hub.Instance.Init();
			InitLog4Net();
			SetAppTheme();

			LoadingWindow loading = new LoadingWindow();

			SetTheme(loading);

			loading.Loaded += LoadingLoaded;
			loading.SetMessage("Initializing core");
			loading.Show();

			//bool autenticated = false;
			//if (Hub.Instance.EncryptionManager.SecureNotesEnabled)
			//{
			//    _authenticationWindow = new AuthenticationWindow();
			//    _authenticationWindow.AuthForm.OnlyVerifyPassword = false;
			//    _authenticationWindow.AuthForm.OnAuthenticated += AuthForm_OnAuthenticated;
			//    _authenticationWindow.AuthForm.OnAuthentionCanceled += AuthForm_OnAuthentionCanceled;
			//    _authenticationWindow.ShowDialog();
			//}
			//else
			//{
			//    autenticated = true;
			//}

			//if (autenticated)
			//{
			//}
		}

		private void LoadingLoaded(object sender, RoutedEventArgs e)
		{
			Thread t = new Thread(Load);
			t.Start(sender);
		}
		private void Load(object sender)
		{
			LoadingWindow loading = (LoadingWindow)sender;

			_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			_log.Info("Application started");

			loading.SetMessage("Loading license");

			//Hub.Instance.LicenseManager.LoadLicense();
			//if (!Hub.Instance.LicenseManager.ValidLicense)
			//{
			//    Hub.Instance.LicenseManager.InitTrailLicense();
			//}

			//if (!Hub.Instance.LicenseManager.ValidLicense)
			//{
			//    MessageBox.Show("Your trail period has ended, please purchase a valid license if you like Noterium.", "Trail", MessageBoxButton.OK, MessageBoxImage.Information);
			//    Current.Shutdown(-1);
			//    return;
			//}

			Current.DispatcherUnhandledException += CurrentDispatcherUnhandledException;
			Current.Deactivated += OnDeactivated;

			Hub.Instance.Storage.DataStore.InitCache(s =>
			{
				Current.Dispatcher.Invoke(() => { loading.SetMessage(s); });
			});

			loading.SetMessage("Loading main window");

			Current.Dispatcher.Invoke(ShowMainWindow);

			Current.Dispatcher.Invoke(() =>
			{
				loading.Close();
			});
		}

		private static void CurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			_log.Fatal(e.Exception.Message, e.Exception);
		}

		private void ShowMainWindow()
		{
			_mainWindow = new MainWindow();
			_mainWindow.DataContext = ViewModelLocator.Instance.Main;
			_mainWindow.Model = ViewModelLocator.Instance.Main;
			_mainWindow.Width = Hub.Instance.AppSettings.WindowSize.Width;
			_mainWindow.Height = Hub.Instance.AppSettings.WindowSize.Height;
			_mainWindow.WindowState = Hub.Instance.AppSettings.WindowState;

			_mainWindow.Model.LockCommand = new BasicCommand(Lock);
			//Re-enable normal shutdown mode.
			Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
			Current.MainWindow = _mainWindow;

			SetTheme(_mainWindow);

			_mainWindow.Show();
			if (Hub.Instance.EncryptionManager.SecureNotesEnabled)
			{
				_mainWindow.Lock(true);
			}
			_mainWindowLoaded = true;

			Hub.Instance.Settings.PropertyChanged += SettingsPropertyChanged;

			InputManager.Current.PreProcessInput += OnActivity;
			_activityTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMinutes(Hub.Instance.Settings.AutoLockMainWindowAfter),
				IsEnabled = true
			};
			_activityTimer.Tick += OnInactivity;
		}

		private void SettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "AutoLockMainWindowAfter")
			{
				_activityTimer.Stop();
				_activityTimer.Interval = TimeSpan.FromMinutes(Hub.Instance.Settings.AutoLockMainWindowAfter);
				_activityTimer.Start();
			}
		}

		void OnInactivity(object sender, EventArgs e)
		{
			if (!Hub.Instance.EncryptionManager.SecureNotesEnabled)
				return;

			// remember mouse position
			_inactiveMousePosition = Mouse.GetPosition(_mainWindow.MainGrid);

			Lock();
		}

		void OnActivity(object sender, PreProcessInputEventArgs e)
		{
			InputEventArgs inputEventArgs = e.StagingItem.Input;

			if (inputEventArgs is MouseEventArgs || inputEventArgs is KeyboardEventArgs)
			{
				MouseEventArgs mouseEventArgs = e.StagingItem.Input as MouseEventArgs;

				// no button is pressed and the position is still the same as the application became inactive
				if (mouseEventArgs?.LeftButton == MouseButtonState.Released &&
					mouseEventArgs.RightButton == MouseButtonState.Released &&
					mouseEventArgs.MiddleButton == MouseButtonState.Released &&
					mouseEventArgs.XButton1 == MouseButtonState.Released &&
					mouseEventArgs.XButton2 == MouseButtonState.Released &&
					_inactiveMousePosition == mouseEventArgs.GetPosition(_mainWindow.MainGrid))
					return;

				_activityTimer.Stop();
				_activityTimer.Start();
			}
		}

		private void SetAppTheme()
		{
			var theme = ThemeManager.GetAppTheme(Hub.Instance.Settings.Theme);
			var accent = ThemeManager.GetAccent(Hub.Instance.Settings.Accent);

			ThemeManager.ChangeAppStyle(Current, accent, theme);
		}

		public static void SetTheme(Window w)
		{
			var theme = ThemeManager.GetAppTheme(Hub.Instance.Settings.Theme);
			var accent = ThemeManager.GetAccent(Hub.Instance.Settings.Accent);

			if (w != null)
				ThemeManager.ChangeAppStyle(w, accent, theme);
		}

		private static void InitLog4Net()
		{
			string logXml = Noterium.Properties.Resources.LogXml;
			string logPath = Hub.Instance.Storage.DataStore.RootFolder + "\\log";
			if (!Directory.Exists(logPath))
				Directory.CreateDirectory(logPath);

			logXml = logXml.Replace("{LOGPATH}", logPath);

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(logXml);
			log4net.Config.XmlConfigurator.Configure(doc.DocumentElement);
		}

		private void OnDeactivated(object sender, EventArgs eventArgs)
		{
			if (!_mainWindowLoaded)
				return;

			if (Hub.Instance.EncryptionManager.SecureNotesEnabled)
			{
				if (Hub.Instance.Settings.AutoLockMainWindowAfter == 0)
					Lock();
			}
		}

		private bool Lock(object param)
		{
			Lock();
			return true;
		}

		private void Lock()
		{
			_mainWindow.Lock();
		}
	}
}

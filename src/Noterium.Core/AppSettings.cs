using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Noterium.Core.DataCarriers;
using Noterium.Core.Helpers;

namespace Noterium.Core
{
	[DataContract]
	public class AppSettings
	{
		private string _settingsFilePath;
		private FileSystemWatcher _watcher;
		public ObservableCollection<Library> Librarys { get; set; } = new ObservableCollection<Library>();
		
		[DataMember]
		public string DefaultLibrary { get; set; } = string.Empty;

		[DataMember]
		public List<string> LibraryFiles { get; set; } = new List<string>(); 

		public string SettingsFolder
		{
			get
			{
				var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				var appFolder = Path.Combine(appdataFolder, "Viktor Ekblom", "Noterium");
				return appFolder;
			}
		}

		public void Init()
		{
			if (!Directory.Exists(SettingsFolder))
				Directory.CreateDirectory(SettingsFolder);

			_settingsFilePath = GetSettingsFilePath();

			if (File.Exists(_settingsFilePath))
			{
				try
				{
					InitInstance();

					_watcher = new FileSystemWatcher
					{
						Path = Path.GetDirectoryName(SettingsFolder),
						Filter = Path.GetFileName(_settingsFilePath),
						NotifyFilter = NotifyFilters.LastWrite
					};
					_watcher.Changed += OnChanged;
					_watcher.EnableRaisingEvents = true;
				}
				catch (Exception e)
				{
					throw new InvalidConfigurationFileException(e);
				}
			}
		}

		private void InitInstance()
		{
			var json = File.ReadAllText(_settingsFilePath);
			JsonConvert.PopulateObject(json, this);
			LoadLibrarys();
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			InitInstance();
		}

		private void LoadLibrarys()
		{
			Librarys.Clear();
			foreach (string libraryFile in LibraryFiles)
			{
				if (File.Exists(libraryFile))
				{
					var lib = FileHelpers.LoadObjectFromFile<Library>(new FileInfo(libraryFile));
					if (lib != null)
					{
						lib.Default = lib.Name.Equals(DefaultLibrary);
						Librarys.Add(lib);
					}
				}
			}
		}

		private string GetSettingsFilePath()
		{
#if DEBUG
			const string configurationFileName = "configuration.debug.json";
#else
                const string configurationFileName = "configuration.json";
#endif

			return Path.Combine(SettingsFolder, configurationFileName);
		}

		public void Save()
		{
			lock (_settingsFilePath)
			{
				var json = this.ToJson();

				_watcher.EnableRaisingEvents = false;
				File.WriteAllText(_settingsFilePath, json);
				_watcher.EnableRaisingEvents = true;
			}
		}

		public void LogFatal(string message)
		{
			var di = Directory.CreateDirectory(Path.Combine(SettingsFolder, "log"));
			string logFile = Path.Combine(di.FullName, $"noterium_fatal_error_{DateTime.Now.Ticks}.log");
			File.WriteAllText(logFile, message);
		}
	}

	public class InvalidConfigurationFileException : Exception
	{
		public InvalidConfigurationFileException(Exception innerException = null) : base(string.Empty, innerException)
		{

		}
	}
}
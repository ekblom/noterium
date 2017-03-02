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
					var json = File.ReadAllText(_settingsFilePath);
					JsonConvert.PopulateObject(json, this);

					LoadLibrarys();
				}
				catch (Exception e)
				{
					throw new InvalidConfigurationFileException(e);
				}
			}
		}

		private void LoadLibrarys()
		{
			foreach (string libraryFile in LibraryFiles)
			{
				if (File.Exists(libraryFile))
				{
					var lib = FileHelpers.LoadObjectFromFile<Library>(new FileInfo(libraryFile));
					if (lib != null)
						Librarys.Add(lib);
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
				File.WriteAllText(_settingsFilePath, json);
			}
		}

		public void LogFatal(string message)
		{
			string logFile = Path.Combine(GetSettingsFilePath(), "log", $"noterium_fatal_error_{DateTime.Now.Ticks}.log");
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
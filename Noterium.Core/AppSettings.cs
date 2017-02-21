using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Noterium.Core.DataCarriers;
using Noterium.Core.Helpers;

namespace Noterium.Core
{
	public class AppSettings
	{
		private string _settingsFilePath;
		public ObservableCollection<Library> Librarys { get; set; } = new ObservableCollection<Library>();
		public double NoteColumnWidth { get; set; } = 250;
		public double NotebookColumnWidth { get; set; } = 205;
		public Size WindowSize { get; set; } = new Size(1024, 768);
		public WindowState WindowState { get; set; } = WindowState.Normal;
		public string SelectedLibrary { get; set; } = string.Empty;

		public void Init()
		{
			_settingsFilePath = GetSettingsFilePath();

			if (File.Exists(_settingsFilePath))
			{
				try
				{
					var json = File.ReadAllText(_settingsFilePath);
					JsonConvert.PopulateObject(json, this);
				}
				catch (Exception e)
				{
					throw new InvalidConfigurationFileException(e);
				}
			}
		}

		private string GetSettingsFilePath()
		{
			var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var appFolder = Path.Combine(appdataFolder, "Viktor Ekblom", "Noterium");
			if (!Directory.Exists(appFolder))
				Directory.CreateDirectory(appFolder);

#if DEBUG
			const string configurationFileName = "configuration.debug.json";
#else
                const string configurationFileName = "configuration.json";
#endif

			return Path.Combine(appFolder, configurationFileName);
		}

		public void Save()
		{
			lock (_settingsFilePath)
			{
				var json = this.ToJson();
				File.WriteAllText(_settingsFilePath, json);
			}
		}
	}

	public class InvalidConfigurationFileException : Exception
	{
		public InvalidConfigurationFileException(Exception innerException = null) : base(string.Empty, innerException)
		{

		}
	}
}
using System;
using System.Collections.Generic;
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
		public ObservableCollection<Library> Librarys { get; set; }
		public double NoteColumnWidth { get; set; } = 250;
		public double NotebookColumnWidth { get; set; } = 205;
		public Size WindowSize { get; set; } = new Size(1024, 768);
		public WindowState WindowState { get; set; } = WindowState.Normal;
		public string SelectedLibrary { get; set; }

		public void Init()
		{
			Librarys = new ObservableCollection<Library>();
			_settingsFilePath = GetSettingsFilePath();

			var fi = new FileInfo(_settingsFilePath);
			if (!fi.Exists)
			{
				var temp = new AppSettings();
				var json = temp.ToJson();
				File.WriteAllText(_settingsFilePath, json);
			}
			else
			{
				var json = File.ReadAllText(_settingsFilePath);
				var temp = JsonConvert.DeserializeObject<AppSettings>(json);
				if (temp == null)
					throw new InvalidConfigurationFileException();

				CopySettings(temp);
			}
		}

		private string GetSettingsFilePath()
		{
			var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var baseFolder = Path.Combine(appdataFolder, "Viktor Ekblom");
			VerifyFolder(baseFolder);

			var appFolder = Path.Combine(baseFolder, "Noterium");
			VerifyFolder(appFolder);

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

		private void CopySettings(AppSettings temp)
		{
			Librarys = temp.Librarys ?? new ObservableCollection<Library>();
			NoteColumnWidth = temp.NoteColumnWidth;
			NotebookColumnWidth = temp.NotebookColumnWidth;
			WindowState = temp.WindowState;
			WindowSize = temp.WindowSize;
			SelectedLibrary = temp.SelectedLibrary ?? string.Empty;
		}

		private void VerifyFolder(string baseFolder)
		{
			var di = new DirectoryInfo(baseFolder);
			if (!di.Exists)
				di.Create();
		}
	}

	public class InvalidConfigurationFileException : Exception
	{
		public InvalidConfigurationFileException()
		{

		}
	}
}
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Noterium.Core.Localization
{
	public class LocalizationManager
	{
		private List<CultureInfo> _supportedCultures;

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static LocalizationManager()
		{
		}

		private LocalizationManager()
		{
			InitLanguages();
		}

		private void InitLanguages()
		{
			CultureInfo culture = new CultureInfo("");
			foreach (string dir in Directory.GetDirectories(System.Windows.Forms.Application.StartupPath))
			{
				try
				{
					DirectoryInfo di = new DirectoryInfo(dir);
					culture = CultureInfo.GetCultureInfo(di.Name);

					if (di.GetFiles(Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath) + ".resources.dll").Length > 0)
					{
						_supportedCultures.Add(culture);
						Debug.WriteLine(string.Format("Found Culture: {0} [{1}]", culture.DisplayName, culture.Name));
					}
				}
				catch { }
			}
		}

		public void ChangeCulture(CultureInfo culture)
		{
			if (_supportedCultures.Contains(culture))
			{
				Properties.Resources.Culture = culture;
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = culture;
				FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
			}
			else
				Debug.WriteLine("Culture [{0}] not available", culture);
		}

		public static LocalizationManager Instance { get; } = new LocalizationManager();
	}
}

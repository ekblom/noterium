using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using Noterium.Core.Helpers;

namespace Noterium.Core.DataCarriers
{
	[DataContract]
	public class Library : IComparable, IComparable<Note>, IEquatable<Note>, INotifyPropertyChanged
	{
		private string _name;

		[DataMember]
		public string Name
		{
			get { return _name; }
			set
			{
				// RenameFile(_name, value);
				_name = value;
				RaiseOnPropetyChanged();
			}
		}

		private void RenameFile(string from, string to)
		{
			string fileName = GetFileName(from);
			string toFileName = GetFileName(to);
			if (File.Exists(fileName) && !File.Exists(toFileName))
			{
				FileInfo fi = new FileInfo(fileName);
				fi.MoveTo(toFileName);
				return;
			}

			if(File.Exists(fileName))
				throw new LibraryExistsException();
		}

		public string FilePath => GetFileName(Name);

		private string GetFileName(string name)
		{
			string validFileName = FileHelpers.GetValidFileName(name.ToLowerInvariant());
			return System.IO.Path.Combine(Hub.Instance.AppSettings.SettingsFolder, $"{validFileName}.libcfg");
		}

		[DataMember]
		public string Path { get; set; }
		[DataMember]
		public StorageType StorageType { get; set; }
		[DataMember]
		public double NoteColumnWidth { get; set; } = 250;
		[DataMember]
		public double NotebookColumnWidth { get; set; } = 205;
		[DataMember]
		public Size WindowSize { get; set; } = new Size(1024, 768);
		[DataMember]
		public WindowState WindowState { get; set; } = WindowState.Normal;

		public int CompareTo(object obj)
		{
			return string.Compare(Name, obj.ToString(), StringComparison.Ordinal);
		}

		public int CompareTo(Note other)
		{
			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}

		public bool Equals(Note other)
		{
			return Name.Equals(other?.Name);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Save()
		{
			FileHelpers.Save(this, FilePath);
		}

		public void Delete()
		{
			if(File.Exists(FilePath))
				File.Delete(FilePath);
		}
	}

	internal class LibraryExistsException : Exception
	{
	}
}
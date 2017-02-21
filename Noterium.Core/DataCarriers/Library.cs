using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Noterium.Core.DataCarriers
{
	public class Library : IComparable, IComparable<Note>, IEquatable<Note>, INotifyPropertyChanged
	{
		private string _name;

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				RaiseOnPropetyChanged();
			}
		}

		public string Path { get; set; }
		public StorageType StorageType { get; set; }

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
	}
}
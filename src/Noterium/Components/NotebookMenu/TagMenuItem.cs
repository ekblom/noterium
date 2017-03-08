using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noterium.Core.DataCarriers;
using Noterium.Properties;

namespace Noterium.Components.NotebookMenu
{
	public class TagMenuItem : INotifyPropertyChanged, IMainMenuItem
    {
		public Tag Tag { get; set; }
		private bool _isSelected;

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; OnPropertyChanged(); }
		}

		public TagMenuItem(Tag tag)
		{
			Tag = tag;
		    Name = Tag.Name;
        }

	    public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
		    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	    public string Name { get; set; }
    }
}
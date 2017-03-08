using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noterium.Core.DataCarriers;
using Noterium.Properties;

namespace Noterium.ViewModels
{
	public class MainMenuItemViewModel : INotifyPropertyChanged
    {
		private bool _visible;
		public Notebook Notebook { get; }

		public List<MainMenuItemViewModel> Notebooks { get; set; }

		public string Name => Notebook.Name;

	    public int SortIndex => Notebook.SortIndex;

		public bool Visible
		{
			get { return _visible; }
			set { _visible = value; OnPropertyChanged(); }
		}

		public MainMenuItemViewModel(Notebook notebook)
		{
            Notebook = notebook;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
		    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
    }
}
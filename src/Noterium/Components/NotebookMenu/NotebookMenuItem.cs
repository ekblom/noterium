using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noterium.Core.DataCarriers;
using Noterium.Properties;

namespace Noterium.Components.NotebookMenu
{
	public class NotebookMenuItem : INotifyPropertyChanged, IMainMenuItem
	{
		private bool _isSelected;
	    private string _name;
	    public Notebook Notebook { get; }

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; OnPropertyChanged(); }
		}

		public NotebookMenuItem(Notebook notebook)
		{
			Notebook = notebook;
            _name = Notebook.Name;
		    Notebook.PropertyChanged += (sender, args) =>
		    {
		        if (args.PropertyName == "Name")
		            Name = Notebook.Name;
		    };
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
		    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	    public string Name
	    {
	        get { return _name; }
	        set { _name = value; OnPropertyChanged(); }
	    }
	}
}
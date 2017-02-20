using System.Windows;

namespace Noterium.Components.NoteMenu
{
	/// <summary>
	/// Interaction logic for NoteMenu.xaml
	/// </summary>
	public partial class NoteMenu
	{
		public NoteMenuViewModel Model => DataContext as NoteMenuViewModel;

	    public NoteMenu()
		{
			InitializeComponent();
		}

		private void NoteMenu_OnLoaded(object sender, RoutedEventArgs e)
		{
			
		}
	}
}

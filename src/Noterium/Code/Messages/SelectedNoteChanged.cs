using Noterium.ViewModels;

namespace Noterium.Code.Messages
{
	internal struct SelectedNoteChanged
	{
		public NoteViewModel SelectedNote { get; private set; }

		public SelectedNoteChanged(NoteViewModel note)
		{
			SelectedNote = note;
		}
	}
}

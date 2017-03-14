using Noterium.ViewModels;

namespace Noterium.Code.Messages
{
	internal struct DeleteNote
	{
		public NoteViewModel Note { get; set; }
		public DeleteNote(NoteViewModel note)
		{
			Note = note;
		}
	}
}

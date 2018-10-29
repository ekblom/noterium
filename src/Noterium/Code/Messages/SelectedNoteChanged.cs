using Noterium.ViewModels;

namespace Noterium.Code.Messages
{
    internal struct SelectedNoteChanged
    {
        public NoteViewModel SelectedNote { get; }

        public SelectedNoteChanged(NoteViewModel note)
        {
            SelectedNote = note;
        }
    }
}
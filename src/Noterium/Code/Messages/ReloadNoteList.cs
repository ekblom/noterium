using Noterium.Core.DataCarriers;

namespace Noterium.Code.Messages
{
	internal struct ReloadNoteList
	{
		public Tag Tag { get; private set; }

		public Notebook Notebook { get; private set; }

		public LibraryType LibraryType { get; private set; }

		public Note SelectedNote { get; private set; }

		public ReloadNoteList(Tag tag)
		{
			LibraryType = LibraryType.Undefined;
			Notebook = null;
			SelectedNote = null;
			Tag = tag;
		}

		public ReloadNoteList(Notebook notebook, Note selectedNote = null)
		{
			LibraryType = LibraryType.Undefined;
			Notebook = notebook;
			SelectedNote = selectedNote;
			Tag = null;
		}

		public ReloadNoteList(LibraryType type)
		{
			LibraryType = type;
			Notebook = null;
			SelectedNote = null;
			Tag = null;
		}

		public bool IsEmpty()
		{
			return Tag == null && Notebook == null && SelectedNote == null && LibraryType == LibraryType.Undefined;
		}
	}
}

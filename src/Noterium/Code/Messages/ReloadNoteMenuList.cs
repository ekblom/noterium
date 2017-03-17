using Noterium.Core.DataCarriers;

namespace Noterium.Code.Messages
{
	internal struct ReloadNoteMenuList
	{
		public Tag Tag { get; private set; }

		public Notebook Notebook { get; private set; }

		public MenuItemType LibraryType { get; private set; }

		public Note SelectedNote { get; private set; }

		public ReloadNoteMenuList(Tag tag)
		{
			LibraryType = MenuItemType.Undefined;
			Notebook = null;
			SelectedNote = null;
			Tag = tag;
		}

		public ReloadNoteMenuList(Notebook notebook, Note selectedNote = null)
		{
			LibraryType = MenuItemType.Undefined;
			Notebook = notebook;
			SelectedNote = selectedNote;
			Tag = null;
		}

		public ReloadNoteMenuList(MenuItemType type)
		{
			LibraryType = type;
			Notebook = null;
			SelectedNote = null;
			Tag = null;
		}

		public bool IsEmpty()
		{
			return Tag == null && Notebook == null && SelectedNote == null && LibraryType == MenuItemType.Undefined;
		}
	}
}

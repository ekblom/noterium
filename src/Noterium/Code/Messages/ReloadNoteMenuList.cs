using Noterium.Core.DataCarriers;

namespace Noterium.Code.Messages
{
    internal struct ReloadNoteMenuList
    {
        public Tag Tag { get; }

        public Notebook Notebook { get; }

        public MenuItemType LibraryType { get; }

        public Note SelectedNote { get; }

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
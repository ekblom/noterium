namespace Noterium.Code.Messages
{
    internal struct ApplicationPartLoaded
    {
        public enum ApplicationParts
        {
            NoteView,
            NoteMenu,
            NotebookMenu
        }

        public ApplicationParts Part { get; }

        public ApplicationPartLoaded(ApplicationParts part)
        {
            Part = part;
        }
    }
}
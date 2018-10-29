using Noterium.Core.DataCarriers;

namespace Noterium.Code.Messages
{
    internal struct SelectNote
    {
        public Note Note { get; }

        public SelectNote(Note note)
        {
            Note = note;
        }
    }
}
using Noterium.Core.DataCarriers;

namespace Noterium.Code.Messages
{
    internal struct ChangeLibrary
    {
        public Library Library { get; }

        public ChangeLibrary(Library l)
        {
            Library = l;
        }
    }
}
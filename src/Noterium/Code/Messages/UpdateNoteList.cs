using Noterium.Code.Interfaces;

namespace Noterium.Code.Messages
{
	internal struct UpdateNoteList
	{
		public IMainMenuItem MenuItem { get; private set; }
		public UpdateNoteList(IMainMenuItem menuItem)
		{
			MenuItem = menuItem;
		}
	}
}

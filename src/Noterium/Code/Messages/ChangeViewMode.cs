using Noterium.Core.Constants;

namespace Noterium.Code.Messages
{
	internal struct ChangeViewMode
	{
		public NoteViewModes Mode  { get; private set; }
		public ChangeViewMode(NoteViewModes mode)
		{
			Mode = mode;
		}
	}
}

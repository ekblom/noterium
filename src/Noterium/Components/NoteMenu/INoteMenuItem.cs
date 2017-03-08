using Noterium.Core.DataCarriers;

namespace Noterium.Components.NoteMenu
{
	public interface INoteMenuItem
	{
		Note Note { get; }
		string ShortDescription { get; }
		bool Secure { get; }
	}
}
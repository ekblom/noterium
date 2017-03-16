using Noterium.Core.DataCarriers;

namespace Noterium.Code.Interfaces
{
	public interface INoteMenuItem
	{
		Note Note { get; }
		string ShortDescription { get; }
		bool Secure { get; }
	}
}
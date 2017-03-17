using Noterium.Core.DataCarriers;

namespace Noterium.Code.Messages
{
	internal struct ChangeLibrary
	{
		public Library Library { get; private set; }

		public ChangeLibrary(Library l)
		{
			Library = l;
		}
	}
}

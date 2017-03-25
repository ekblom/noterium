using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public ApplicationParts Part{ get; private set; }

		public ApplicationPartLoaded(ApplicationParts part)
		{
			Part = part;
		}
	}
}

using Noterium.Core.DataCarriers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noterium.Code.Messages
{
	internal struct SelectNote
	{
		public Note Note{ get; private set; }

		public SelectNote(Note note)
		{
			Note = note;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noterium.Code.Messages
{
	internal struct ConfigureControlsForParnetType
	{
		public ParentType Type { get; private set; }

		public ConfigureControlsForParnetType(ParentType type)
		{
			Type = type;
		}

		public enum ParentType
		{
			Notebook,
			Tag,
			Library
		}
	}
}

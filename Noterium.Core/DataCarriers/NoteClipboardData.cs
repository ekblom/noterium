using System;
using System.Collections.Generic;

namespace Noterium.Core.DataCarriers
{
	[Serializable]
	public class NoteClipboardData
	{
		public string Text { get; set; } 
		public List<string> Tags { get; set; }
		public List<ClipboardFile> Files { get; set; }
		public string Name { get; set; }
	}

	[Serializable]
	public class ClipboardFile
	{
		public string Name { get; set; }
		public byte[] Data { get; set; }
		public string MimeType { get; set; }
		public string FileName { get; set; }
	}
}
using System.IO;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Shell;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
	public class NoteFileViewModel
	{
		public NoteFile NoteFile { get; set; }

		public ImageSource Thumbnail
		{
			get
			{
				string filePath = NoteFile.FullName;
				if (File.Exists(filePath))
				{
					ShellFile shellFile = ShellFile.FromFilePath(filePath);
				    return shellFile.Thumbnail?.MediumBitmapSource;
				}
				return null;
			}
		}

		public NoteFileViewModel(NoteFile noteFile)
		{
			NoteFile = noteFile;

		}
	}
}
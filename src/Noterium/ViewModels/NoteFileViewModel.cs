using System.IO;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Shell;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
    public class NoteFileViewModel
    {
        public NoteFileViewModel(NoteFile noteFile)
        {
            NoteFile = noteFile;
        }

        public NoteFile NoteFile { get; set; }

        public ImageSource Thumbnail
        {
            get
            {
                var filePath = NoteFile.FullName;
                if (File.Exists(filePath))
                {
                    var shellFile = ShellFile.FromFilePath(filePath);
                    return shellFile.Thumbnail?.MediumBitmapSource;
                }

                return null;
            }
        }
    }
}
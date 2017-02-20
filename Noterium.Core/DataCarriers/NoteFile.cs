using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using Noterium.Core.Annotations;
using Noterium.Core.Helpers;

namespace Noterium.Core.DataCarriers
{
    [DataContract]
    public class NoteFile: INotifyPropertyChanged
    {
        private string _name;

        public NoteFile()
        {
        }

        public NoteFile(string fileName, string name, Guid owner)
        {
            FileName = fileName;
            Name = name;
            Owner = owner;
        }

        [DataMember(Order = 0)]
        public string FileName { get; set; }

        [DataMember(Order = 1)]
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        [DataMember(Order = 2)]
        public Guid Owner { get; set; }

        public bool IsImage => MimeType.IsImageExtension(Path.GetExtension(FileName));

        public string FullName => Hub.Instance.Storage.GetNoteFilePath(this);

        public BitmapSource Thumbnail
        {
            get
            {
                var shellFile = ShellFile.FromFilePath(FullName);
                if(shellFile != null)
                    return shellFile.Thumbnail.BitmapSource;

                return null;
            }
        }

        public FileInfo GetFile()
        {
            var path = Hub.Instance.Storage.GetNoteFilePath(this);
            return new FileInfo(path);
        }

        public static NoteFile Create(string name, string mime, byte[] data, Note owner, string extension = null)
        {
            var folder = Hub.Instance.Storage.GetNoteFolderPath(owner.ID);
            var ext = extension ?? MimeType.GetDefaultExtension(mime);
            if (string.IsNullOrWhiteSpace(ext) && name != null && name.IndexOf(".", StringComparison.Ordinal) != -1)
            {
                ext = name.Split('.')[1];
            }

            if (string.IsNullOrWhiteSpace(ext))
                throw new Exception("Unable to get file extension.");

            var finalFileName = owner.ID + "_" + GetNewFilenumber(owner.Files.ToList()) + ext;
            var finalFilePath = folder + "\\" + finalFileName;
            try
            {
                File.WriteAllBytes(finalFilePath, data);

                var nf = new NoteFile(finalFileName, name ?? finalFileName, owner.ID);
                owner.Files.Add(nf);
                owner.Save();
                return nf;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string ProposeNoteFileName(string name, Note note)
        {
            var existingNoteFile = note.Files.FirstOrDefault(enf => enf.Name.Equals(name));

            int count = 1;
            while (existingNoteFile != null)
            {
                int indexOfDot = name.IndexOf(".", StringComparison.Ordinal);
                name = name.Insert(indexOfDot, count.ToString());

                existingNoteFile = note.Files.FirstOrDefault(enf => enf.Name.Equals(name));
            }

            return name;
        }

        private static int GetNewFilenumber(List<NoteFile> files)
        {
            if (!files.Any())
                return 0;

            var numbers = files.ConvertAll(file =>
            {
                var chunks = file.FileName.Split('_');
                if (chunks.Length == 2)
                {
                    chunks = chunks[1].Split('.');
                    int number;
                    if (int.TryParse(chunks[0], out number))
                        return number;
                }

                return 0;
            });

            return numbers.Max() + 1;
        }

        public void Delete()
        {
            var fi = GetFile();
            if (fi.Exists)
                fi.Delete();
        }

        public string GetAsImageMarkdown()
        {
            return $"![{Name}]({FileName})";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetAsFileLink()
        {
            return $"[{Name}]({FileName})";
        }
    }
}
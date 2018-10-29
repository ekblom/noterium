using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using Noterium.Core.Helpers;

namespace Noterium.Core.DataCarriers
{
    [DataContract]
    public class Library : IComparable, IComparable<Note>, IEquatable<Note>, INotifyPropertyChanged
    {
        private bool _default;
        private string _name;

        public Library()
        {
            _default = Hub.Instance.AppSettings.DefaultLibrary.Equals(Name);
        }

        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                // RenameFile(_name, value);
                _name = value;
                RaiseOnPropetyChanged();
            }
        }

        public string FilePath => GetFileName(Name);

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public StorageType StorageType { get; set; }

        [DataMember]
        public double NoteColumnWidth { get; set; } = 250;

        [DataMember]
        public double NotebookColumnWidth { get; set; } = 205;

        [DataMember]
        public Size WindowSize { get; set; } = new Size(1024, 768);

        [DataMember]
        public WindowState WindowState { get; set; } = WindowState.Normal;

        public bool Default
        {
            get => _default;
            set
            {
                _default = value;
                RaiseOnPropetyChanged();
            }
        }

        public int CompareTo(object obj)
        {
            return string.Compare(Name, obj.ToString(), StringComparison.Ordinal);
        }

        public int CompareTo(Note other)
        {
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public bool Equals(Note other)
        {
            return Name.Equals(other?.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RenameFile(string from, string to)
        {
            var fileName = GetFileName(from);
            var toFileName = GetFileName(to);
            if (File.Exists(fileName) && !File.Exists(toFileName))
            {
                var fi = new FileInfo(fileName);
                fi.MoveTo(toFileName);
                return;
            }

            if (File.Exists(fileName))
                throw new LibraryExistsException();
        }

        private string GetFileName(string name)
        {
            var validFileName = FileHelpers.GetValidFileName(name.ToLowerInvariant());
            return System.IO.Path.Combine(Hub.Instance.AppSettings.SettingsFolder, $"{validFileName}.libcfg");
        }

        private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save()
        {
            FileHelpers.Save(this, FilePath);
        }

        public void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }

    internal class LibraryExistsException : Exception
    {
    }
}
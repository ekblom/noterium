using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Noterium.Code.Commands;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Windows;

namespace Noterium.ViewModels
{
    public class BackupManagerViewModel : NoteriumViewModelBase
    {
        private BackupSet _selectedBackupSet;
        private FileTreeNode _selectedFileNode;
        private bool _showNoteFields;
        public ObservableCollection<BackupSet> BackupSets { get; set; }

        public BackupSet SelectedBackupSet
        {
            get { return _selectedBackupSet; }
            set { _selectedBackupSet = value; RaiseOnPropetyChanged(); }
        }

        public ObservableCollection<ITreeNode> BackupSetNodes { get; }

        public FileTreeNode SelectedFileNode
        {
            get { return _selectedFileNode; }
            set
            {
                _selectedFileNode = value;
                ShowNoteFields = _selectedFileNode != null;
                RaiseOnPropetyChanged();
            }
        }

        public bool ShowNoteFields
        {
            get { return _showNoteFields; }
            set { _showNoteFields = value; RaiseOnPropetyChanged(); }
        }

        public ICommand RestoreNoteCommand { get; set; }
        public BackupManager Window { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BackupManagerViewModel()
        {
            LoadBackupSets();
            BackupSetNodes = new ObservableCollection<ITreeNode>();
            RestoreNoteCommand = new SimpleCommand(RestoreSelectedNote);
            PropertyChanged += OnPropertyChanged;
        }

        private void RestoreSelectedNote(object obj)
        {
            if (SelectedFileNode == null)
                return;

            var task = Window.ShowMessageAsync("Restore?", "Do you want to restore this note?", MessageDialogStyle.AffirmativeAndNegative);
            task.ContinueWith(task1 =>
            {
                if (task1.Result == MessageDialogResult.Affirmative)
                {
                    var existingNote = Hub.Instance.Storage.GetNote(SelectedFileNode.Note.ID);
                    if (existingNote != null)
                    {
                        //TODO: Add more functionality, make sure protection and so on and so forth
                        existingNote.Text = SelectedFileNode.Note.Text;
                        existingNote.Tags = SelectedFileNode.Note.Tags;
                    }
                    else
                    {
                        Notebook nb = Hub.Instance.Storage.GetNotebook(SelectedFileNode.Note.Notebook);
                        if (nb != null)
                        {
                           ;
                            var dialog = (BaseMetroDialog)Window.Resources["SelectTargetNotebookDialog"];

                            var settings = new MetroDialogSettings()
                            {
                                AffirmativeButtonText = "OK",
                                AnimateShow = true,
                                NegativeButtonText = "Go away!",
                                FirstAuxiliaryButtonText = "Cancel",
                            };


                            Window.ShowMetroDialogAsync(dialog, settings);
                        }

                    }
                }
            });
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "SelectedBackupSet")
                LoadBackupSet(SelectedBackupSet.File.FullName);
        }

        private void LoadBackupSets()
        {
            BackupSets = new ObservableCollection<BackupSet>();
            var files = Hub.Instance.Storage.GetBackupFiles();
            foreach (var file in files)
            {
                BackupSet set = new BackupSet(file);
                BackupSets.Add(set);
            }
        }

        private void LoadBackupSet(string fileName)
        {
            List<FolderTreeNode> folders = new List<FolderTreeNode>();
            List<FolderTreeNode> allTreeNodes = new List<FolderTreeNode>();

            using (ZipArchive zip = ZipFile.Open(fileName, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    string path = entry.FullName.Contains("/") ? entry.FullName.Substring(0, entry.FullName.LastIndexOf('/')) : "/";
                    FolderTreeNode parentFolder = null;
                    if (path != "/")
                    {
                        Queue<string> pathChunks = new Queue<string>(path.Split('/'));
                        while (pathChunks.Any())
                        {
                            string folderName = pathChunks.Dequeue();
                            FolderTreeNode node = allTreeNodes.FirstOrDefault(f => f.FolderId == folderName);

                            if (node == null)
                            {
                                node = new FolderTreeNode(folderName, parentFolder);
                                if (parentFolder != null)
                                    parentFolder.Children.Add(node);
                                else
                                    folders.Add(node);

                                parentFolder = node;
                                allTreeNodes.Add(node);
                            }
                            else
                            {
                                parentFolder = node;
                            }
                        }

                    }

                    if (entry.Name.EndsWith(".note"))
                    {
                        Note note = ConvertFileInfo<Note>(entry.Open());
                        parentFolder?.Children.Add(new FileTreeNode(entry, note, parentFolder));
                    }
                    else if (entry.Name.EndsWith(".book"))
                    {
                        Notebook nb = ConvertFileInfo<Notebook>(entry.Open());
                        var treeNode = allTreeNodes.FirstOrDefault(n => n.FolderId.ToLowerInvariant() == nb.ID.ToString().ToLowerInvariant());
                        if (treeNode != null)
                        {
                            treeNode.Name = nb.Name;
                            treeNode.Notebook = nb;
                        }
                    }
                }
            }

            SelectedFileNode = null;
            BackupSetNodes.Clear();
            folders.ForEach(BackupSetNodes.Add);
        }

        private T ConvertFileInfo<T>(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            var fileContent = reader.ReadToEnd();
            reader.Close();

            var result = JsonConvert.DeserializeObject<T>(fileContent);

            return result;
        }

    }

    public interface ITreeNode
    {
        string Name { get; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
    }

    public class FolderTreeNode : ITreeNode, INotifyPropertyChanged
    {
        private readonly ITreeNode _parent;
        public string Name { get; set; }
        public string Path { get; }
        private bool _isSelected;
        private bool _isExpanded;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    RaiseOnPropetyChanged("IsSelected");
                }
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    RaiseOnPropetyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        public List<ITreeNode> Children { get; private set; }
        public Notebook Notebook { get; set; }

        private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FolderTreeNode(string folderId, ITreeNode parent = null)
        {
            _parent = parent;
            FolderId = folderId;
            Children = new List<ITreeNode>();
        }

        public string FolderId { get; set; }
    }

    public class FileTreeNode : ITreeNode, INotifyPropertyChanged
    {
        public Note Note { get; set; }
        private readonly ZipArchiveEntry _entry;
        private readonly ITreeNode _parent;
        private bool _isSelected;
        private bool _isExpanded;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    RaiseOnPropetyChanged("IsSelected");
                }
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    RaiseOnPropetyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        public string Name => Note.Name;

        public FileTreeNode(ZipArchiveEntry entry, Note note, ITreeNode parent)
        {
            Note = note;
            _entry = entry;
            _parent = parent;
        }

        private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BackupSet
    {
        public FileInfo File { get; }
        public string FileName => File.Name;

        public BackupSet(FileInfo file)
        {
            File = file;
        }
    }

    public class FileCarrier
    {
        public string FilePath { get; }
        public string FileName { get; }

        public FileCarrier(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }
    }
}
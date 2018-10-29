using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public BackupManagerViewModel()
        {
            LoadBackupSets();
            BackupSetNodes = new ObservableCollection<ITreeNode>();
            RestoreNoteCommand = new SimpleCommand(RestoreSelectedNote);
            PropertyChanged += OnPropertyChanged;
        }

        public ObservableCollection<BackupSet> BackupSets { get; set; }

        public BackupSet SelectedBackupSet
        {
            get => _selectedBackupSet;
            set
            {
                _selectedBackupSet = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<ITreeNode> BackupSetNodes { get; }

        public FileTreeNode SelectedFileNode
        {
            get => _selectedFileNode;
            set
            {
                _selectedFileNode = value;
                ShowNoteFields = _selectedFileNode != null;
                RaisePropertyChanged();
            }
        }

        public bool ShowNoteFields
        {
            get => _showNoteFields;
            set
            {
                _showNoteFields = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RestoreNoteCommand { get; set; }
        public BackupManager Window { get; set; }
        public bool Loaded { get; internal set; }

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
                        var nb = Hub.Instance.Storage.GetNotebook(SelectedFileNode.Note.Notebook);
                        if (nb != null)
                        {
                            ;
                            var dialog = (BaseMetroDialog) Window.Resources["SelectTargetNotebookDialog"];

                            var settings = new MetroDialogSettings
                            {
                                AffirmativeButtonText = "OK",
                                AnimateShow = true,
                                NegativeButtonText = "Go away!",
                                FirstAuxiliaryButtonText = "Cancel"
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
                var set = new BackupSet(file);
                BackupSets.Add(set);
            }
        }

        private void LoadBackupSet(string fileName)
        {
            var folders = new List<FolderTreeNode>();
            var allTreeNodes = new List<FolderTreeNode>();

            using (var zip = ZipFile.Open(fileName, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    var path = entry.FullName.Contains("/") ? entry.FullName.Substring(0, entry.FullName.LastIndexOf('/')) : "/";
                    FolderTreeNode parentFolder = null;
                    if (path != "/")
                    {
                        var pathChunks = new Queue<string>(path.Split('/'));
                        while (pathChunks.Any())
                        {
                            var folderName = pathChunks.Dequeue();
                            var node = allTreeNodes.FirstOrDefault(f => f.FolderId == folderName);

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
                        var note = ConvertFileInfo<Note>(entry.Open());
                        parentFolder?.Children.Add(new FileTreeNode(entry, note, parentFolder));
                    }
                    else if (entry.Name.EndsWith(".book"))
                    {
                        var nb = ConvertFileInfo<Notebook>(entry.Open());
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
            var reader = new StreamReader(stream);
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
        private bool _isExpanded;
        private bool _isSelected;

        public FolderTreeNode(string folderId, ITreeNode parent = null)
        {
            _parent = parent;
            FolderId = folderId;
            Children = new List<ITreeNode>();
        }

        public string Path { get; }

        public List<ITreeNode> Children { get; }
        public Notebook Notebook { get; set; }

        public string FolderId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
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
            get => _isExpanded;
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

        private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FileTreeNode : ITreeNode, INotifyPropertyChanged
    {
        private readonly ZipArchiveEntry _entry;
        private readonly ITreeNode _parent;
        private bool _isExpanded;
        private bool _isSelected;

        public FileTreeNode(ZipArchiveEntry entry, Note note, ITreeNode parent)
        {
            Note = note;
            _entry = entry;
            _parent = parent;
        }

        public Note Note { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get => _isSelected;
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
            get => _isExpanded;
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

        private void RaiseOnPropetyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BackupSet
    {
        public BackupSet(FileInfo file)
        {
            File = file;
        }

        public FileInfo File { get; }
        public string FileName => File.Name;
    }

    public class FileCarrier
    {
        public FileCarrier(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }

        public string FilePath { get; }
        public string FileName { get; }
    }
}
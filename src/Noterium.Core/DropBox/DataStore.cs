using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using log4net;
using Noterium.Core.DataCarriers;
using Noterium.Core.Helpers;
using File = Noterium.Core.Constants.File;

namespace Noterium.Core.DropBox
{
    public class DataStore
    {
        private const string BackupFileDateFormat = "yyyyMMddHHmmss";
        private readonly DirectoryInfo _backupFolder;
        private readonly DirectoryInfo _dataFolder;


        private readonly ILog _log = LogManager.GetLogger(typeof(DataStore));
        private readonly DirectoryInfo _rootFolder;

        private readonly string _rootPath;

        private Dictionary<Notebook, List<Note>> _cache;
        private FileSystemWatcher _watcher;

        public DataStore(string path)
        {
            _rootPath = string.IsNullOrWhiteSpace(path) ? GetDropBoxPath() : path;

            if (!string.IsNullOrWhiteSpace(_rootPath))
            {
                EnsureFolder(_rootPath);
                _rootFolder = new DirectoryInfo(_rootPath);

                var dataPath = _rootPath + "\\data";
                EnsureFolder(dataPath);
                _dataFolder = new DirectoryInfo(dataPath);

                var backupPath = _rootPath + "\\backup";
                EnsureFolder(backupPath);
                _backupFolder = new DirectoryInfo(backupPath);
            }
        }

        public string RootFolder => _rootFolder.FullName;

        public string DataFolder => _dataFolder.FullName;

        #region -- Security --

        internal string MasterPasswordFile => _rootFolder.FullName + "\\noterium.key";

        #endregion

        private void StartWatch(string path)
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = path;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.IncludeSubdirectories = true;
            _watcher.Filter = "*.*";
            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        private void DisableWatcher()
        {
            if (_watcher != null) _watcher.EnableRaisingEvents = false;
        }

        private void EnableWatcher()
        {
            if (_watcher != null) _watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            _log.Debug($"Change detected on {e.FullPath}");
            if (e.Name.EndsWith(File.NoteFileExtension))
                HandleNoteChange(e);
            else if (e.Name.EndsWith(File.NotebookFileExtension)) HandleNotebookChange(e);
            _log.Debug($"Handled {e.Name}, reason {e.ChangeType}.");
        }

        private void HandleNoteChange(FileSystemEventArgs e)
        {
            var fileName = Path.GetFileName(e.FullPath);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var idString = fileName.Replace("." + File.NoteFileExtension, string.Empty);
            Guid noteId;
            if (Guid.TryParse(idString, out noteId))
            {
                var n = GetNote(noteId);
                if (n != null)
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        var tempNote = FileHelpers.LoadObjectFromFile<Note>(new FileInfo(e.FullPath));
                        if (tempNote.Changed > n.Changed)
                            try
                            {
                                n.IsUpdatingFromDisc = true;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    tempNote?.CopyProperties(n);
                                    tempNote?.RaiseRefreshedFromDisk();
                                });
                            }
                            catch (Exception ex)
                            {
                                _log.Error("Unable to update note from disk", ex);
                            }
                            finally
                            {
                                n.IsUpdatingFromDisc = false;
                            }
                    }
                    else if (e.ChangeType == WatcherChangeTypes.Deleted)
                    {
                        DeleteNote(n);
                    }
                }
                else
                {
                    n = FileHelpers.LoadObjectFromFile<Note>(new FileInfo(e.FullPath));
                    if (n != null)
                    {
                        var nb = GetNoteBook(n.Notebook);
                        if (nb != null && _cache.ContainsKey(nb))
                        {
                            var noteList = _cache[nb];
                            if (!noteList.Contains(n))
                                noteList.Add(n);
                        }
                    }
                }
            }

            _log.Debug($"Handled changed note {e.Name}");
        }

        private void HandleNotebookChange(FileSystemEventArgs e)
        {
            var fileName = Path.GetFileName(e.FullPath);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var idString = fileName.Replace("." + File.NotebookFileExtension, string.Empty);
            Guid notebookId;
            if (Guid.TryParse(idString, out notebookId))
            {
                var nb = GetNoteBook(notebookId);
                if (nb != null)
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        var tempNotebook = FileHelpers.LoadObjectFromFile<Notebook>(new FileInfo(e.FullPath));
                        Application.Current.Dispatcher.Invoke(() => { tempNotebook?.CopyProperties(nb); });
                    }
                    else if (e.ChangeType == WatcherChangeTypes.Deleted)
                    {
                        DeleteNoteBook(nb);
                    }
                }
            }

            _log.Debug($"Handled changed notebook {e.Name}");
        }

        public void SaveSettings(Settings settings)
        {
            var filePath = _rootFolder.FullName + "\\settings.json";
            Save(settings, filePath);
        }

        private void Save<T>(T o, string filePath)
        {
            DisableWatcher();
            FileHelpers.Save(o, filePath);
            EnableWatcher();
        }

        public Settings GetSettings()
        {
            var filePath = _rootFolder.FullName + "\\settings.json";
            var fi = new FileInfo(filePath);
            if (fi.Exists)
                return FileHelpers.LoadObjectFromFile<Settings>(fi);

            var settings = new Settings();
            SaveSettings(settings);
            return settings;
        }

        public void SaveNote(Note note, bool skipAddToGroup = false)
        {
            var filePath = GetNoteFilePath(note);
            _log.Debug($"Saving note {note.Name}, ID: {note.ID}");
            Save(note, filePath);

            if (!skipAddToGroup)
                AddNoteToGroup(note);
        }

        public void MoveNote(Note note, Notebook notebook)
        {
            var oldNotebook = note.Notebook;

            var filePath = GetNoteFilePath(note);
            var fi = new FileInfo(filePath);

            DisableWatcher();

            //NOTE: Move files before updating notebook id since nf.FullName uses that.
            var noteFolder = GetNotebookFolderPath(notebook.ID);
            foreach (var nf in note.Files)
            {
                var noteFile = new FileInfo(nf.FullName);
                if (noteFile.Exists)
                    noteFile.MoveTo(string.Join("\\", noteFolder, nf.FileName));
            }

            note.Notebook = notebook.ID;
            filePath = GetNoteFilePath(note);

            fi.MoveTo(filePath);

            EnableWatcher();

            SaveNote(note, true);

            foreach (var cachItem in _cache)
                if (cachItem.Key.ID == oldNotebook)
                {
                    cachItem.Value.Remove(note);
                    break;
                }

            _cache[notebook].Add(note);
        }

        private string GetNoteFilePath(Note note)
        {
            string filePath;
            if (note.Notebook != Guid.Empty)
            {
                var folderPath = GetNotebookFolderPath(note.Notebook);
                EnsureFolder(folderPath);
                filePath = string.Format("{0}\\{1}.{2}", folderPath, note.ID, File.NoteFileExtension);
            }
            else
            {
                filePath = GetNoteFilePath(note.ID, File.NoteFileExtension);
            }

            return filePath;
        }

        private void AddNoteToGroup(Note note)
        {
            var g = GetGroup(note.Notebook);
            if (!_cache[g].Contains(note))
                _cache[g].Add(note);
        }

        private Notebook GetGroup(Guid group)
        {
            foreach (var keyValuePair in _cache)
                if (keyValuePair.Key.ID == group)
                    return keyValuePair.Key;
            return null;
        }

        public void SaveNoteBook(Notebook noteBook)
        {
            var folderPath = GetNotebookFolderPath(noteBook.ID);
            EnsureFolder(folderPath);

            var filePath = string.Format("{0}\\{1}.{2}", folderPath, noteBook.ID, File.NotebookFileExtension);

            Save(noteBook, filePath);

            if (!_cache.ContainsKey(noteBook))
                _cache.Add(noteBook, new List<Note>());
        }

        public List<Note> GetNotes(Notebook notebook)
        {
            return _cache[notebook];
        }

        public List<Notebook> GetNoteBooks()
        {
            return _cache.Keys.ToList();
        }

        public int GetNoteCount(Notebook notebook)
        {
            return _cache[notebook].Count;
        }

        public int GetTotalNoteCount()
        {
            return _cache.Values.Sum(k => k.Count);
        }

        public List<Note> GetAllNotes()
        {
            return _cache.SelectMany(kvp => kvp.Value).ToList();
        }

        public Note GetNote(Guid id)
        {
            foreach (var kvp in _cache)
            foreach (var m in kvp.Value)
                if (m.ID == id)
                    return m;

            return null;
        }

        public void DeleteNote(Note note)
        {
            var filePath = GetNoteFilePath(note);
            var fi = new FileInfo(filePath);
            if (fi.Exists)
            {
                DisableWatcher();
                fi.Delete();
                EnableWatcher();
            }

            ReloadNotebook(note.Notebook);
        }

        public void DeleteNoteBook(Notebook noteBook)
        {
            var di = GetGroupDirectory(noteBook);
            if (di.Exists)
            {
                DisableWatcher();
                di.Delete(true);
                EnableWatcher();
            }

            _cache.Remove(noteBook);
        }

        public void EnsureOneNotebook()
        {
            if (_cache.Count == 0)
            {
                var nb = new Notebook {ID = Guid.NewGuid(), Created = DateTime.Now, Name = "My notes"};
                SaveNoteBook(nb);
                var n = new Note
                {
                    Notebook = nb.ID,
                    Name = "Welcome!",
                    Text = @"Welcome!

In noterium you write your notes in plain text with MarkDown suport.

Examples:

You can make headlines
==

Sub headlines
--

### Small healines

[Example of named link](examplelink)  
[Second example of named link](google)  
This is a simple link: <http://www.example.com>  
[Example of a regular link](http://www.example.com)

**Make stuf bold**  
*Italic*  
~~Strike~~  

* Make lists
* For
* Something

- [ ] To do list
- [ ] For
- [ ] Your
- [ ] Lists

And nestled lists:  

- List item
   1. Sub list item
   2. Sub list item
- List item

Insert code:  

	function()
	{
		alert(""Bananas"");
	}

Add inline code ``` alert(""Bananas""); ```.

> Make quotes...
 > With multiple rows! ;)

And you can make tables:
| Tables | Are | Cool |
|:--------------|---------:|:-----:|
| col 3 is      | r - l    | $1600 |
| col 2 is      | centered |   $12 |
| zebra stripes | are neat |    $1 |


[examplelink]: http://www.example.se ""A link to example.com""
 [google]: http://www.google.se ""Google""",
                    Created = DateTime.Now,
                    Tags = new ObservableCollection<string>(new List<string> {"welcome", "noterium"})
                };
                n.SetIsInitialized();
                n.Save();

                //InitCache();
            }
        }

        public string GetStoragePath()
        {
            return _dataFolder.FullName;
        }

        public bool EnsureDropBox()
        {
            return !string.IsNullOrWhiteSpace(_rootPath);
        }


        public void BackupData()
        {
            try
            {
                var backupFile = _backupFolder.FullName + "\\" + DateTime.Now.ToString(BackupFileDateFormat) + ".zip";
                ZipFile.CreateFromDirectory(_dataFolder.FullName, backupFile, CompressionLevel.Fastest, false);
            }
            catch (Exception e)
            {
                _log.Error("Error when creating backup file.", e);
            }
        }

        public DateTime GetLastBackupDate()
        {
            var files = GetBackupFiles();
            if (!files.Any())
                return DateTime.MinValue;

            var backupDate = DateTime.MinValue;
            foreach (var fi in files)
            {
                var dt = GetFileDate(fi);
                if (dt > backupDate)
                    backupDate = dt;
            }

            return backupDate;
        }

        public List<FileInfo> GetBackupFiles()
        {
            return _backupFolder.GetFiles("*.zip").ToList();
        }

        public void CleanBackupData(int backupsToKeep)
        {
            if (backupsToKeep <= 0)
                return;

            try
            {
                var temp = _backupFolder.GetFiles("*.zip");
                if (temp.Length == 0)
                    return;

                var files = new List<FileInfo>();
                files.AddRange(temp);

                files.Sort((x, y) => DateTime.Compare(GetFileDate(y), GetFileDate(x)));

                var filesToDelete = files.Skip(backupsToKeep).ToList();

                filesToDelete.ForEach(fi => fi.Delete());
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
        }

        private DateTime GetFileDate(FileInfo fi)
        {
            try
            {
                var fileName = fi.Name.Replace(fi.Extension, string.Empty);
                return DateTime.ParseExact(fileName, BackupFileDateFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }

            return DateTime.MinValue;
        }

        public Notebook GetNoteBook(Guid guid)
        {
            return _cache.Keys.FirstOrDefault(key => key.ID == guid);
        }

        #region -- File system --

        private void EnsureFolder(string path)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                if (_watcher != null)
                    _watcher.EnableRaisingEvents = false;

                di.Create();

                if (_watcher != null)
                    _watcher.EnableRaisingEvents = true;
            }
        }

        private DirectoryInfo GetGroupDirectory(Notebook notebook)
        {
            DirectoryInfo di = null;
            if (notebook != null) di = new DirectoryInfo(_dataFolder.FullName + "\\" + notebook.ID);
            return di;
        }

        internal string GetNoteFilePath(Guid guid, string extension)
        {
            return string.Format("{0}\\{1}.{2}", DataFolder, guid, extension);
        }

        internal string GetNotebookFolderPath(Guid guid)
        {
            return string.Format("{0}\\{1}", DataFolder, guid);
        }

        public static string GetDropBoxPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbPath = Path.Combine(appDataPath, "Dropbox\\host.db");

            if (!System.IO.File.Exists(dbPath))
            {
                appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                dbPath = Path.Combine(appDataPath, "Dropbox\\host.db");

                if (!System.IO.File.Exists(dbPath))
                    return null;
            }

            var lines = System.IO.File.ReadAllLines(dbPath);
            var dbBase64Text = Convert.FromBase64String(lines[1]);
            var folderPath = Encoding.UTF8.GetString(dbBase64Text);

            return folderPath + "\\noterium";
        }

        #endregion

        #region -- Cache --

        public void InitCache(Action<string> callback)
        {
            // TODO: i18n
            _cache = new Dictionary<Notebook, List<Note>>();
            callback("Loading notebooks");
            var notebooks = GetNotebookFromDisc();

            for (var index = 0; index < notebooks.Count; index++)
            {
                callback("Loading notes from notebook " + (index + 1) + " of " + notebooks.Count);

                var group = notebooks[index];
                var notes = GetNotesFromDisc(group);
                notes.ForEach(n => n.SetIsInitialized());
                _cache.Add(group, notes);
            }

            StartWatch(DataFolder);
        }

        private void ReloadNotebook(Guid notebookId)
        {
            var notebook = _cache.Keys.FirstOrDefault(memorygroup => memorygroup.ID == notebookId);
            if (notebook == null)
                return;

            var notes = GetNotesFromDisc(notebook);
            var cachedNotes = _cache[notebook];

            var notesToRemove = cachedNotes.Where(cachedMemory => !notes.Contains(cachedMemory)).ToList();
            notesToRemove.ForEach(n => cachedNotes.Remove(n));

            foreach (var note in notes)
            {
                var existing = cachedNotes.FirstOrDefault(mm => mm.ID == note.ID);
                if (existing == null)
                {
                    note.SetIsInitialized();
                    _cache[notebook].Add(note);
                }
            }
        }

        public List<Note> GetNotesFromDisc(Notebook noteBook)
        {
            var di = GetGroupDirectory(noteBook);

            var files = di.GetFiles("*." + File.NoteFileExtension);

            return FileHelpers.ConvertFileInfos<Note>(files);
        }

        public List<Notebook> GetNotebookFromDisc()
        {
            var files = _dataFolder.GetFiles("*." + File.NotebookFileExtension, SearchOption.AllDirectories);

            return FileHelpers.ConvertFileInfos<Notebook>(files);
        }

        #endregion
    }
}
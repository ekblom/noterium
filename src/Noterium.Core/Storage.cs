using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Noterium.Core.DataCarriers;
using Noterium.Core.DropBox;

namespace Noterium.Core
{
    public class Storage
    {
        public delegate void TagListUpdated();

        private HashSet<string> _tags;

        internal Storage()
        {
        }

        public DataStore DataStore { get; private set; }

        public List<string> Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = new HashSet<string>();
                    foreach (var n in GetAllNotes())
                    {
                        if (n.Tags != null && n.Tags.Count > 0)
                        {
                            foreach (var t in n.Tags)
                            {
                                if (!_tags.Contains(t))
                                    _tags.Add(t);
                            }
                        }
                    }
                }

                return _tags.ToList();
            }
        }

        public event TagListUpdated OnTagListUpdated;

        public void Init(Library library)
        {
            if (library == null)
                throw new ApplicationException("Unable to detect storage type.");

	        _tags = null;

	        string path = null;
	        if (library.StorageType == StorageType.DropBox)
		        path = DataStore.GetDropBoxPath();
			else if(!string.IsNullOrWhiteSpace(library.Path))
			path = library.Path;

	        if (path == null)
		        throw new Exception("Library path is null");

			DataStore = new DataStore(path);
        }

        internal void SaveNote(Note note)
        {
            DataStore.SaveNote(note);

            var updated = false;
            foreach (var t in note.Tags)
            {
                if (!Tags.Contains(t))
                {
                    Tags.Add(t);
                    updated = true;
                }
            }

            if (updated)
                OnTagListUpdated?.Invoke();
        }

        internal void SaveNotebook(Notebook notebook)
        {
            DataStore.SaveNoteBook(notebook);
        }

        public List<Note> GetNotes(Notebook notebook)
        {
            return DataStore.GetNotes(notebook);
        }

        public int GetNoteCount(Notebook notebook)
        {
            return DataStore.GetNoteCount(notebook);
        }

        public int GetTotalNoteCount()
        {
            return DataStore.GetTotalNoteCount();
        }

        public List<Notebook> GetNotebooks()
        {
            return DataStore.GetNoteBooks();
        }

        public List<Note> GetAllNotes()
        {
            return DataStore.GetAllNotes();
        }

        internal void SaveSettings(DataCarriers.Settings settings)
        {
            DataStore.SaveSettings(settings);
        }

        internal DataCarriers.Settings GetSettings()
        {
            return DataStore.GetSettings();
        }

        public Note GetNote(Guid id)
        {
            return DataStore.GetNote(id);
        }

        internal void DeleteNote(Note note)
        {
            DataStore.DeleteNote(note);
        }

        internal void DeleteGroup(Notebook noteBook)
        {
            DataStore.DeleteNoteBook(noteBook);
        }

        public bool EnsureDropBox()
        {
            return DataStore.EnsureDropBox();
        }

        public void EnsureOneNotebook()
        {
            DataStore.EnsureOneNotebook();
        }

        internal void MoveNote(Note note, Notebook noteBook)
        {
            DataStore.MoveNote(note, noteBook);
        }

        public void Backup()
        {
            DataStore.BackupData();
        }

        public void CleanBackupData()
        {
            DataStore.CleanBackupData(Hub.Instance.Settings.NumberOfBackupsToKeep);
        }

        public DateTime GetLastBackupDate()
        {
            return DataStore.GetLastBackupDate();
        }

        public List<FileInfo> GetBackupFiles()
        {
            return DataStore.GetBackupFiles();
        }

        public Notebook GetNotebook(Guid guid)
        {
            return DataStore.GetNoteBook(guid);
        }

        public string GetNoteFilePath(NoteFile nf)
        {
            var note = DataStore.GetNote(nf.Owner);
            var groupPath = DataStore.GetNotebookFolderPath(note.Notebook);
            return groupPath + "\\" + nf.FileName;
        }

        public string GetNoteFolderPath(Guid noteId)
        {
            var note = DataStore.GetNote(noteId);
            return DataStore.GetNotebookFolderPath(note.Notebook);
        }
    }
}
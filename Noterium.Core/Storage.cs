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

        public DropBoxDataStore DataStore { get; private set; }

        internal string RegKey
        {
            get
            {
                const string userRoot = "HKEY_CURRENT_USER\\Software\\";
                const string company = "Viktor Ekblom";

#if DEBUG
                const string applicationName = "NoteriumDebug";
#else
                const string applicationName = "Noterium";
#endif

                const string keyName = userRoot + "\\" + company + "\\" + applicationName;
                return keyName;
            }
        }

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

            var path = library.Path;

            DataStore = new DropBoxDataStore(path);
        }

        public StorageType GetStorageType()
        {
            try
            {
                var value = (int) Registry.GetValue(RegKey, "StorageType", -1);
                if (value == -1)
                    return StorageType.Undefined;

                var type = (StorageType) value;
                return type;
            }
            catch (Exception)
            {
            }

            return StorageType.Undefined;
        }

        public bool HasStorageTypeSet()
        {
            var type = GetStorageType();
            return type != StorageType.Undefined;
        }

        public bool SetStorageType(StorageType type, string path = null)
        {
            try
            {
                if (type == StorageType.Disc && string.IsNullOrWhiteSpace(path))
                    return false;

                Registry.SetValue(RegKey, "StorageType", (int) type, RegistryValueKind.DWord);

                if (!string.IsNullOrEmpty(path))
                    Registry.SetValue(RegKey, "StoragePath", path, RegistryValueKind.String);
                else
                {
                    var existingPath = (string) Registry.GetValue(RegKey, "StoragePath", null);
                    if (!string.IsNullOrWhiteSpace(existingPath))
                    {
                        var regKey = Registry.CurrentUser.OpenSubKey(RegKey);
                        if (regKey != null)
                            regKey.DeleteValue("StoragePath");
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal void SaveReminder(SimpleReminder reminder)
        {
            DataStore.SaveReminder(reminder);
        }

        internal void DeleteReminder(SimpleReminder reminder)
        {
            DataStore.DeleteReminder(reminder);
        }

        internal List<SimpleReminder> GetReminders()
        {
            return DataStore.GetReminders();
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

        internal void SaveNotebook(Notebook noteBook)
        {
            DataStore.SaveNoteBook(noteBook);
        }

        public List<Note> GetNotes(Notebook mg)
        {
            return DataStore.GetNotes(mg);
        }

        public int GetNoteCount(Notebook mg)
        {
            return DataStore.GetNoteCount(mg);
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

        public int GetAllNoteCount()
        {
            return 100;
        }

        public string GetStoragePath()
        {
            try
            {
                var value = (string) Registry.GetValue(RegKey, "StoragePath", null);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            catch (Exception)
            {
            }

            return null;
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
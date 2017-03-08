using System;
using System.Collections.Generic;
using Noterium.Core.DataCarriers;

namespace Noterium.Core
{
    public interface IDataStore
    {
        void SaveNote(Note note);
        void SaveNoteBook(Notebook noteBook);
        List<Note> GetNotes(Notebook mg);
        int GetNoteCount(Notebook mg);
        int GetTotalNoteCount();
        List<Notebook> GetNoteBooks();
        List<Note> GetAllNotes();
        void SaveSettings(DataCarriers.Settings settings);
        DataCarriers.Settings GetSettings();
        Note GetNote(Guid id);
        void DeleteNote(Note note);
        void DeleteNoteBook(Notebook noteBook);
        void EnsureOneNotebook();
        string GetStoragePath();

        bool EnsureDropBox();

        void MoveNote(Note note, Notebook noteBook);

        void BackupData();
        DateTime GetLastBackupDate();

        Notebook GetNoteBook(Guid guid);

        void CleanBackupData(int backupsToKeep);

        List<SimpleReminder> GetReminders();
        void SaveReminder(SimpleReminder reminder);
        void DeleteReminder(SimpleReminder reminder);
    }
}
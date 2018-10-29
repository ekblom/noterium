using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
    internal class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            RegisterTypes();
        }

        public static ViewModelLocator Instance => (ViewModelLocator) Application.Current.Resources["Locator"];

        /// <summary>
        ///     Gets the Main property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        /// <summary>
        ///     Gets the Main property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public NoteViewerViewModel NoteView => ServiceLocator.Current.GetInstance<NoteViewerViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public LibrarysViewModel Librarys => ServiceLocator.Current.GetInstance<LibrarysViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public BackupManagerViewModel BackupManager => ServiceLocator.Current.GetInstance<BackupManagerViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public NoteMenuViewModel NoteMenu => ServiceLocator.Current.GetInstance<NoteMenuViewModel>();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public NotebookMenuViewModel NotebookMenu => ServiceLocator.Current.GetInstance<NotebookMenuViewModel>();

        private static void RegisterTypes()
        {
            // TODO: Design mode hub
            // TODO: NoteDataService
            if (ViewModelBase.IsInDesignModeStatic)
            {
                //SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<LibrarysViewModel>();
            SimpleIoc.Default.Register<BackupManagerViewModel>();
            SimpleIoc.Default.Register<NoteMenuViewModel>();
            SimpleIoc.Default.Register<NotebookMenuViewModel>();
            SimpleIoc.Default.Register<NoteViewModel>();
            SimpleIoc.Default.Register<NoteViewerViewModel>();
            SimpleIoc.Default.Register<NotebookViewModel>();
        }

        public NoteViewModel GetNoteViewModel(Note note)
        {
            var isRegistered = SimpleIoc.Default.IsRegistered<NoteViewModel>(note.ID.ToString());

            var model = SimpleIoc.Default.GetInstance<NoteViewModel>(note.ID.ToString());
            if (!isRegistered)
                model.Init(note);

            return model;
        }

        public List<NoteViewModel> GetNoteViewModels(List<Note> notes)
        {
            var result = new List<NoteViewModel>();
            for (var i = 0; i < notes.Count; i++)
            {
                var n = notes[i];
                var model = GetNoteViewModel(n);
                result.Add(model);
            }

            return result;
        }

        public NotebookViewModel GetNotebookViewModel(Notebook notebook)
        {
            var isRegistered = SimpleIoc.Default.IsRegistered<NotebookViewModel>(notebook.ID.ToString());

            var model = SimpleIoc.Default.GetInstance<NotebookViewModel>(notebook.ID.ToString());
            if (!isRegistered)
                model.Init(notebook);

            return model;
        }

        public List<NotebookViewModel> GetNotebookViewModels(List<Notebook> notebooks)
        {
            var result = new List<NotebookViewModel>();
            for (var i = 0; i < notebooks.Count; i++)
            {
                var n = notebooks[i];
                var model = GetNotebookViewModel(n);
                result.Add(model);
            }

            return result;
        }

        public void Unregister(string key)
        {
            SimpleIoc.Default.Unregister(key);
        }

        /// <summary>
        ///     Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            // TODO: Reset all view models
            SimpleIoc.Default.Reset();
            RegisterTypes();
        }
    }
}
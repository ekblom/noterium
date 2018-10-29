using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Noterium.Components.NotebookMenu;
using Noterium.Components.NoteMenu;
using Noterium.Core.DataCarriers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;

namespace Noterium.ViewModels
{
	class ViewModelLocator
	{
		public static ViewModelLocator Instance => (ViewModelLocator)App.Current.Resources["Locator"];

		static ViewModelLocator()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
			RegisterTypes();
		}

		private static void RegisterTypes()
		{
			// TODO: Design mode hub
			// TODO: NoteDataService
			if (ViewModelBase.IsInDesignModeStatic)
			{
				//SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
			}
			else
			{
				//SimpleIoc.Default.Register<IDataService, DataService>();
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

		/// <summary>
		/// Gets the Main property.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public MainViewModel Main
		{
			get
			{
				return ServiceLocator.Current.GetInstance<MainViewModel>();
			}
		}

		/// <summary>
		/// Gets the Main property.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public NoteViewerViewModel NoteView
		{
			get
			{
				return ServiceLocator.Current.GetInstance<NoteViewerViewModel>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public LibrarysViewModel Librarys
		{
			get
			{
				return ServiceLocator.Current.GetInstance<LibrarysViewModel>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public SettingsViewModel Settings
		{
			get
			{
				return ServiceLocator.Current.GetInstance<SettingsViewModel>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public BackupManagerViewModel BackupManager
		{
			get
			{
				return ServiceLocator.Current.GetInstance<BackupManagerViewModel>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public NoteMenuViewModel NoteMenu
		{
			get
			{
				return ServiceLocator.Current.GetInstance<NoteMenuViewModel>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public NotebookMenuViewModel NotebookMenu
		{
			get
			{
				return ServiceLocator.Current.GetInstance<NotebookMenuViewModel>();
			}
		}

		public NoteViewModel GetNoteViewModel(Note note)
		{
			bool isRegistered = SimpleIoc.Default.IsRegistered<NoteViewModel>(note.ID.ToString());

			var model = SimpleIoc.Default.GetInstance<NoteViewModel>(note.ID.ToString());
			if (!isRegistered)
				model.Init(note);

			return model;
		}

		public List<NoteViewModel> GetNoteViewModels(List<Note> notes)
		{
			List<NoteViewModel> result = new List<NoteViewModel>();
			for (int i = 0; i < notes.Count; i++)
			{
				Note n = notes[i];
				var model = GetNoteViewModel(n);
				result.Add(model);
			}

			return result;
		}

		public NotebookViewModel GetNotebookViewModel(Notebook notebook)
		{
			bool isRegistered = SimpleIoc.Default.IsRegistered<NotebookViewModel>(notebook.ID.ToString());

			var model = SimpleIoc.Default.GetInstance<NotebookViewModel>(notebook.ID.ToString());
			if (!isRegistered)
				model.Init(notebook);

			return model;
		}

		public List<NotebookViewModel> GetNotebookViewModels(List<Notebook> notebooks)
		{
			List<NotebookViewModel> result = new List<NotebookViewModel>();
			for (int i = 0; i < notebooks.Count; i++)
			{
				Notebook n = notebooks[i];
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
		/// Cleans up all the resources.
		/// </summary>
		public static void Cleanup()
		{
			// TODO: Reset all view models
			SimpleIoc.Default.Reset();
			RegisterTypes();
		}
	}
}

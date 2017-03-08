using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using MimeTypes;
using Noterium.Code.Commands;
using Noterium.Code.Helpers;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Core.Utilities;
using Noterium.Components.NotebookMenu;
using Noterium.Components.NoteMenu;
using Noterium.Windows;

namespace Noterium.ViewModels
{
	public class MainViewModel : NoteriumViewModelBase, IDisposable
	{
		public ICommand CreateNewNoteCommand { get; set; }
		public ICommand CreateNewSecureNoteCommand { get; set; }
		public ICommand DeleteNoteCommand { get; set; }
		public ICommand EditNoteNameCommand { get; set; }
		public ICommand DeleteNoteBookCommand { get; set; }
		public ICommand EditNoteCommand { get; set; }
		public ICommand SaveNoteCommand { get; set; }
		public ICommand ToggleNoteMenu { get; set; }
		public ICommand ToggleHelpViewCommand { get; set; }
		public ICommand ToggleNotebookMenu { get; set; }
		public ICommand SelectedNoteContainerChangedCommand { get; set; }
		public ICommand SelectedNoteChangedCommand { get; set; }
		public ICommand MainWindowLoadedCommand { get; set; }
		public ICommand LockCommand { get; set; }
		public ICommand ShowAboutWindowCommand { get; set; }
		public ICommand ToggleSearchFlyoutCommand { get; set; }
		public ICommand SearchTextChangedCommand { get; set; }
		public ICommand SearchResultSelectedCommand { get; set; }
		public ICommand CloseSearchCommand { get; set; }
		public ICommand PerformLockActionsCommand { get; set; }
		public ICommand ToggleSettingsFlyoutCommand { get; set; }
		public ICommand ChangeLibraryCommand { get; set; }
		public ICommand SetDefaultLibraryCommand { get; set; }
		public ICommand DeleteLibraryCommand { get; set; }
		public ICommand AddLibraryCommand { get; set; }

		private readonly object _currentNotesLockObject = new object();
		private readonly object _currentNotesbooksLockObject = new object();
		private readonly object _mainMenuListLockObject = new object();
		private readonly object _searchResultLockObject = new object();

		public Dictionary<Guid, NoteViewModel> NoteViewModels { get; set; }
		public ObservableCollection<NotebookMenuItem> NotebookMenuItems { get; set; }
		public ObservableCollection<TopMainMenuItemViewModel> TopMainMenuItems { get; set; }

		private Timer _intervallTimer;
		private NotebookMenuViewModel _menuContext;
		private IMainMenuItem _selectedMenuItem;
		private NoteMenuViewModel _noteMenuContext;
		private bool _addNoteButtonVisible;
		private readonly Timer _quickMessageTimer;
		private readonly Timer _searchTimer;

		#region -- Static menu items --
		private readonly LibraryMenuItem _favoritesLibraryMenuItem = new LibraryMenuItem("Favorites", "Favorites");
		private readonly LibraryMenuItem _trashLibraryMenuItem = new LibraryMenuItem("Trash", "Trash");
		private readonly LibraryMenuItem _allLibraryMenuItem = new LibraryMenuItem("All notes", "All");
		private readonly LibraryMenuItem _recentLibraryMenuItem = new LibraryMenuItem("Recent", "Recent");
		private bool _secureNotesEnabled;
		private SettingsViewModel _settingsViewModel;
		private bool _quickInformationIsOpen;
		private string _quickInformationMessage;
		private bool _notebookColumnVisibility = true;
		private bool _showNoteColumn = true;
		private bool _searchFlyoutIsVisible;
		private string _searchTerm;
		private bool _isSearching;
		private bool _isHelpVisible;
		private bool _settingsFlyoutIsVisible;
		private bool _librarysFlyoutIsVisible;
		private Library _currentLibrary;
		private string _helpDocumentText;

		#endregion

		#region -- Propertys --

		public bool AddNoteButtonVisible
		{
			get { return _addNoteButtonVisible; }
			set
			{
				bool oldValue = _addNoteButtonVisible;
				_addNoteButtonVisible = value;
				if (oldValue != value)
					RaisePropertyChanged();
			}
		}

		public bool SearchFlyoutIsVisible
		{
			get { return _searchFlyoutIsVisible; }
			set { _searchFlyoutIsVisible = value; RaisePropertyChanged(); }
		}

		public IMainMenuItem SelectedMenuItem
		{
			get { return _selectedMenuItem; }
			set { _selectedMenuItem = value; RaisePropertyChanged(); }
		}

		public ObservableCollection<Tag> Tags => Hub.Instance.Settings.Tags;

		public NotebookMenuViewModel MenuContext
		{
			get { return _menuContext; }
			set { _menuContext = value; RaisePropertyChanged(); }
		}

		public NoteMenuViewModel NoteMenuContext
		{
			get { return _noteMenuContext; }
			set { _noteMenuContext = value; RaisePropertyChanged(); }
		}

		public SettingsViewModel SettingsViewModel
		{
			get { return _settingsViewModel; }
			set { _settingsViewModel = value; RaisePropertyChanged(); }
		}

		public bool IsSecureNotesEnabled
		{
			get { return _secureNotesEnabled; }
			set { _secureNotesEnabled = value; RaisePropertyChanged(); }
		}

		public bool QuickInformationIsOpen
		{
			get { return _quickInformationIsOpen; }
			set { _quickInformationIsOpen = value; RaisePropertyChanged(); }
		}

		public string QuickInformationMessage
		{
			get { return _quickInformationMessage; }
			set { _quickInformationMessage = value; RaisePropertyChanged(); }
		}

		public bool NotebookColumnVisibility
		{
			get { return _notebookColumnVisibility; }
			set { _notebookColumnVisibility = value; RaisePropertyChanged(); }
		}

		public bool ShowNoteColumn
		{
			get { return _showNoteColumn; }
			set { _showNoteColumn = value; RaisePropertyChanged(); }
		}

		public string SearchTerm
		{
			get { return _searchTerm; }
			set
			{
				_searchTerm = value;
				RaisePropertyChanged();
			}
		}

		public ObservableCollection<NoteViewModel> SearchResult { get; }
		public ObservableCollection<LibraryViewModel> Librarys { get; }

		public bool IsSearching
		{
			get { return _isSearching; }
			set { _isSearching = value; RaisePropertyChanged(); }
		}

		public bool IsHelpVisible
		{
			get { return _isHelpVisible; }
			set { _isHelpVisible = value; RaisePropertyChanged(); }
		}

		public bool SettingsFlyoutIsVisible
		{
			get { return _settingsFlyoutIsVisible; }
			set { _settingsFlyoutIsVisible = value; RaisePropertyChanged(); }
		}

		public bool LibrarysFlyoutIsVisible
		{
			get { return _librarysFlyoutIsVisible; }
			set { _librarysFlyoutIsVisible = value; RaisePropertyChanged(); }
		}

		public Library CurrentLibrary
		{
			get { return _currentLibrary; }
			set { _currentLibrary = value; RaisePropertyChanged(); }
		}

		public string HelpDocumentText
		{
			get { return _helpDocumentText; }
			set { _helpDocumentText = value; RaisePropertyChanged(); }
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel(Library library)
		{
			CurrentLibrary = library;
			
			CreateNewNoteCommand = new RelayCommand(CreateNewNote);
			CreateNewSecureNoteCommand = new RelayCommand(CreateNewSecureNote);
			DeleteNoteCommand = new RelayCommand(DeleteCurrentNote);
			DeleteNoteBookCommand = new RelayCommand(DeleteCurrentNoteBook);
			EditNoteCommand = new RelayCommand(EditNote);
			SaveNoteCommand = new RelayCommand(SaveNote);
			EditNoteNameCommand = new RelayCommand(EditNoteName);
			MainWindowLoadedCommand = new RelayCommand(MainWindowLoaded);
			ShowAboutWindowCommand = new SimpleCommand(ShowAboutWindow);
			ToggleNoteMenu = new SimpleCommand(ToggleNoteMenuVisibility);
			ToggleNotebookMenu = new SimpleCommand(ToggleNotebookMenuVisibility);
			SelectedNoteContainerChangedCommand = new SimpleCommand(SelectedNoteContainerChanged);
			ToggleSearchFlyoutCommand = new SimpleCommand(ToggleSearchFlyout);
			//SearchTextChangedCommand = new SimpleCommand(Search);
			SearchResultSelectedCommand = new SimpleCommand(SearchResultSelected);
			CloseSearchCommand = new RelayCommand(CloseSearch);
			ToggleHelpViewCommand = new RelayCommand(ToggleHelpView);
			PerformLockActionsCommand = new RelayCommand(PerformLockActions);
			ToggleSettingsFlyoutCommand = new RelayCommand(ToggleSettingsFlyout);
			AddLibraryCommand = new RelayCommand(AddLibrary);
			DeleteLibraryCommand = new SimpleCommand(DeleteLibrary);
			SetDefaultLibraryCommand = new SimpleCommand(SetDefaultLibrary);

			NotebookMenuItems = new ObservableCollection<NotebookMenuItem>();
			NoteViewModels = new Dictionary<Guid, NoteViewModel>();
			TopMainMenuItems = new ObservableCollection<TopMainMenuItemViewModel>();
			SearchResult = new ObservableCollection<NoteViewModel>();

			Librarys = new ObservableCollection<LibraryViewModel>();
			ReloadLibrary();

			BindingOperations.EnableCollectionSynchronization(NotebookMenuItems, _currentNotesbooksLockObject);
			BindingOperations.EnableCollectionSynchronization(SearchResult, _searchResultLockObject);

			PropertyChanged += MainViewModelPropertyChanged;

			Hub.Instance.Settings.PropertyChanged += SettingsPropertyChanged;

			Hub.Instance.EncryptionManager.PropertyChanged += SettingsPropertyChanged;
			IsSecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;

			SettingsViewModel = new SettingsViewModel(Hub.Instance.Settings);

			_quickMessageTimer = new Timer { AutoReset = false };
			_quickMessageTimer.Elapsed += QuickMessageTimerElapsed;
			_quickMessageTimer.Interval = 2000;

			_searchTimer = new Timer { AutoReset = false };
			_searchTimer.Elapsed += SearchTimerElapsed;
			_searchTimer.Interval = 250;

			Hub.Instance.AppSettings.Librarys.CollectionChanged += LibrarysOnCollectionChanged;

			HelpDocumentText = Properties.Resources.Help_Document;
		}

		private void SetDefaultLibrary(object obj)
		{
			LibraryViewModel model = obj as LibraryViewModel;
			if (model != null)
			{
				Hub.Instance.AppSettings.Librarys.ToList().ForEach(l =>
				{
					l.Default = model.Library == l;
					l.Save();
				});
				Hub.Instance.AppSettings.DefaultLibrary = model.Library.Name;
				Hub.Instance.AppSettings.Save();
			}
		}

		private void LibrarysOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			ReloadLibrary();
		}

		private void ReloadLibrary()
		{
			Librarys.Clear();
			Hub.Instance.AppSettings.Librarys.ToList().ConvertAll(li => new LibraryViewModel(li)).ForEach(Librarys.Add);
		}

		private void DeleteLibrary(object o)
		{
			LibraryViewModel library = (LibraryViewModel)o;

			var settings = DialogHelpers.GetDefaultDialogSettings();

			string message = $"Do you want to delete the library '{library.Library.Name}'?\nAll files will be left untouched, it's just the connection that is removed.";
			MainWindowInstance.ShowMessageAsync("Delete library", message, MessageDialogStyle.AffirmativeAndNegative, settings).
				ContinueWith(delegate (Task<MessageDialogResult> task)
				{
					if (task.Result == MessageDialogResult.Affirmative)
					{
						InvokeOnCurrentDispatcher(() =>
						{
							Hub.Instance.AppSettings.Librarys.Remove(library.Library);
							library.Library.Delete();
						});
					}
				});
		}

		private void AddLibrary()
		{
			var dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true,
				AllowNonFileSystemItems = false,
				EnsurePathExists = true,
				Multiselect = false
			};

			CommonFileDialogResult result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok)
			{
				DirectoryInfo di = new DirectoryInfo(dialog.FileName);
				if (di.Exists)
				{
					string path = dialog.FileName;
					string name = Path.GetFileName(path);
					var library = new Library
					{
						Name = name,
						Path = path,
						StorageType = StorageType.Disc
					};
					library.Save();

					Hub.Instance.AppSettings.Librarys.Add(library);
					Hub.Instance.AppSettings.LibraryFiles.Add(library.FilePath);
					Hub.Instance.AppSettings.Save();

					ChangeLibraryCommand?.Execute(new LibraryViewModel(library));
				}
			}
		}

		private void ToggleSettingsFlyout()
		{
			SettingsFlyoutIsVisible = !SettingsFlyoutIsVisible;
		}

		private void PerformLockActions()
		{
			if (IsHelpVisible)
				IsHelpVisible = false;
			if (SettingsFlyoutIsVisible)
				SettingsFlyoutIsVisible = false;
			if (SearchFlyoutIsVisible)
				SearchFlyoutIsVisible = false;
		}

		private void ToggleHelpView()
		{
			IsHelpVisible = !IsHelpVisible;
		}

		private void CloseSearch()
		{
			SearchFlyoutIsVisible = false;
			SearchResult.Clear();
			SearchTerm = string.Empty;
		}

		private void SearchResultSelected(object obj)
		{
			NoteViewModel model = obj as NoteViewModel;
			if (model != null)
			{
				var nb = NotebookMenuItems.FirstOrDefault(n => n.Notebook == model.Notebook);
				if (nb != null)
				{
					SelectedNoteContainerChanged(nb, model);
					CloseSearch();
				}
			}
		}

		private void Search()
		{
			lock (_searchResultLockObject)
			{
				_searchTimer.Stop();
				_searchTimer.Start();
			}
		}

		private void SearchTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(SearchTerm))
				return;

			InvokeOnCurrentDispatcher(() =>
			{
				IsSearching = true;
			});

			var searchResult = Hub.Instance.SearchManager.Search(SearchTerm);

			InvokeOnCurrentDispatcher(() =>
			{
				var models = GetNoteViewModels(searchResult);

				SearchResult.Clear();
				models.ForEach(SearchResult.Add);

				IsSearching = false;

			});
		}

		private void ToggleSearchFlyout(object obj)
		{
			SearchFlyoutIsVisible = !SearchFlyoutIsVisible;
		}

		private void ToggleNotebookMenuVisibility(object obj)
		{
			NotebookColumnVisibility = !NotebookColumnVisibility;
		}

		private void ToggleNoteMenuVisibility(object obj)
		{
			ShowNoteColumn = !ShowNoteColumn;
		}

		private void QuickMessageTimerElapsed(object sender, ElapsedEventArgs e)
		{
			InvokeOnCurrentDispatcher(() => { QuickInformationIsOpen = false; });
		}

		private void ShowQuickMessage(string message)
		{
			QuickInformationMessage = message;
			QuickInformationIsOpen = true;
			_quickMessageTimer.Start();
		}

		private void ShowAboutWindow(object arg)
		{
			AboutWindow win = new AboutWindow { DataContext = new AboutWindowViewModel() };
			win.ShowDialog();
		}

		private void SettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SecureNotesEnabled")
			{
				IsSecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;
			}
		}

		public void MainWindowLoaded()
		{
			Init();
			InitMainMenu();
			//LoadTheme();
		}

		private void Init()
		{
			Hub.Instance.EncryptionManager.OnPasswordNeeded += SystemNeedsEncryptionPassword;

			Hub.Instance.Storage.EnsureOneNotebook();

			if (Hub.Instance.Settings.EnableTextClipper)
			{
				//Hub.Instance.TextClipper.Init(this.Handle);
				//Hub.Instance.TextClipper.OnTextClipped += TextClipper_OnTextClipped;
			}

			//Hub.Instance.ReminderManager.OnQueueItemHandle += ReminderManagerOnOnQueueItemHandle;
			//Hub.Instance.ReminderManager.OnQueueItemHandled += ReminderManagerOnOnQueueItemHandled;

			Hub.Instance.Reminders.OnReminderDue += Reminders_OnReminderDue;

			_intervallTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
			_intervallTimer.Elapsed += IntervallTimerOnElapsed;
			_intervallTimer.Enabled = true;
		}

		private SecureString SystemNeedsEncryptionPassword()
		{
			//string password = "abc123";

			Task<SecureString> pass = MainWindowInstance.ShowInputAsync("Password required", "Password:").ContinueWith(delegate (Task<string> task)
			{
				string name = task.Result;
				if (!string.IsNullOrWhiteSpace(name))
				{
					return FileSecurity.ConvertToSecureString(name);
				}
				throw new Exception("Password required.");
			});

			if (pass?.Result == null)
				throw new Exception("Password required.");

			return pass.Result;
		}

		private void IntervallTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (Hub.Instance.Settings.AutoBackup)
			{
				DateTime date = Hub.Instance.Storage.GetLastBackupDate();
				TimeSpan ts = DateTime.Now - date;
				if (ts.Days > 0)
				{
					Hub.Instance.Storage.Backup();
					Hub.Instance.Storage.CleanBackupData();
				}
			}
		}

		//private void ReminderManagerOnOnQueueItemHandled(GoogleCalendarReminderManager.QueueItem item)
		//{
		//    //ToolStripLabelSyncStatus.Text = string.Empty;
		//}

		//private void ReminderManagerOnOnQueueItemHandle(GoogleCalendarReminderManager.QueueItem item)
		//{
		//    //ToolStripLabelSyncStatus.Text = item.Note.Name;
		//}

		private void Reminders_OnReminderDue(SimpleReminder simpleReminder)
		{
			//ShowReminderDialog(simpleReminder);
		}

		private void EditNoteName()
		{

		}

		private void SaveNote()
		{
			NoteMenuContext.SelectedNote.SaveNoteCommand?.Execute(null);
		}
		private void EditNote()
		{
			NoteMenuContext.SelectedNote.EditNoteCommand?.Execute(null);
		}

		void MainViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SearchTerm")
				Search();
		}

		private void ReloadNoteList(NoteViewModel selected = null)
		{
			List<Note> notes = new List<Note>();

			if (MenuContext.SelectedTag != null)
			{
				var tempNotes = Hub.Instance.Storage.GetAllNotes();
				notes = tempNotes.Where(n => n.Tags.Contains(MenuContext.SelectedTag.Name)).Where(n => !n.InTrashCan).ToList();
			}
			else if (MenuContext.SelectedNotebook != null)
			{
				notes = Hub.Instance.Storage.GetNotes(MenuContext.SelectedNotebook.Notebook).Where(n => !n.InTrashCan).ToList();
			}
			else if (SelectedMenuItem is LibraryMenuItem)
			{
				if (SelectedMenuItem == _trashLibraryMenuItem)
				{
					var tempNotes = Hub.Instance.Storage.GetAllNotes();
					notes = tempNotes.Where(n => n.InTrashCan).ToList();
				}
				else if (SelectedMenuItem == _favoritesLibraryMenuItem)
				{
					var tempNotes = Hub.Instance.Storage.GetAllNotes();
					notes = tempNotes.Where(n => n.Favourite).Where(n => !n.InTrashCan).ToList();
				}
				else if (SelectedMenuItem == _allLibraryMenuItem)
				{
					notes = Hub.Instance.Storage.GetAllNotes().Where(n => !n.InTrashCan).OrderBy(n => n.Name).ToList();
				}
				else if (SelectedMenuItem == _recentLibraryMenuItem)
				{
					notes = Hub.Instance.Storage.GetAllNotes().Where(n => !n.InTrashCan).OrderBy(n => n.Changed).Take(15).ToList();
				}
			}

			if (NoteMenuContext == null)
			{
				NoteMenuContext = new NoteMenuViewModel
				{
					SelectedItemChangedCommand = SelectedNoteChangedCommand,
					DeleteItemCommand = DeleteNoteCommand,
					RenameItemCommand = new SimpleCommand(RenameNoteItem),
					EditItemCommand = EditNoteCommand,
					CopyNoteCommand = new SimpleCommand(CopyNote)
				};
				BindingOperations.EnableCollectionSynchronization(NoteMenuContext.DataSource, _currentNotesLockObject);
			}

			var models = GetNoteViewModels(notes);

			NoteMenuContext.UpdateDataSource(models);

			if (selected != null && models.Contains(selected))
				NoteMenuContext.SelectedNote = selected;
			else
				NoteMenuContext.SelectedNote = models.Any() ? models[0] : null;
		}

		private void CopyNote(object obj)
		{
			Note note = obj as Note;
			if (note != null)
			{
				if (note.Encrypted)
				{
					var settings = DialogHelpers.GetDefaultDialogSettings();
					MainWindowInstance.ShowMessageAsync(Properties.Resources.Note_Copy_SecureTitle, Properties.Resources.Note_Copy_SecureText, MessageDialogStyle.AffirmativeAndNegative, settings).
					ContinueWith(delegate (Task<MessageDialogResult> task)
					{
						if (task.Result == MessageDialogResult.Affirmative)
							DoCopyNote(note);
					});
				}
				else
					DoCopyNote(note);
			}
		}

		private static void DoCopyNote(Note note)
		{
			NoteClipboardData data = new NoteClipboardData
			{
				Name = note.Name,
				Text = note.DecryptedText,
				Tags = new List<string>(note.Tags)
			};

			if (note.Files.Any())
			{
				data.Files = new List<ClipboardFile>();
				foreach (NoteFile noteFile in note.Files)
				{
					ClipboardFile file = new ClipboardFile
					{
						Name = noteFile.Name,
						Data = File.ReadAllBytes(noteFile.FullName),
						FileName = noteFile.FileName,
						MimeType = MimeTypeMap.GetMimeType(Path.GetExtension(noteFile.FileName))
					};
					data.Files.Add(file);
				}
			}

			DataFormat format = DataFormats.GetDataFormat(typeof(NoteClipboardData).FullName);

			IDataObject dataObj = new DataObject();
			dataObj.SetData(format.Name, data, false);
			Clipboard.SetDataObject(dataObj, false);
		}

		private void SelectedNoteContainerChanged(object p)
		{
			SelectedNoteContainerChanged(p, null);
		}
		private bool SelectedNoteContainerChanged(object p, NoteViewModel selected)
		{
			if (Hub.Instance.EncryptionManager.SecureNotesEnabled && !Hub.Instance.EncryptionManager.IsAuthenticated)
				return false;

			object addedItem = null;
			SelectionChangedEventArgs selectionChangedEventArgs = p as SelectionChangedEventArgs;
			if (selectionChangedEventArgs != null)
			{
				SelectionChangedEventArgs args = selectionChangedEventArgs;
				if (args.AddedItems.Count > 0)
					addedItem = args.AddedItems[0];
			}
			else if (p is NotebookMenuItem)
				addedItem = p;

			var tag = addedItem as Tag;
			if (tag != null)
			{
				MenuContext.SelectedTag = tag;
				MenuContext.SelectedNotebook = null;
				AddNoteButtonVisible = false;
				SelectedMenuItem = null;
			}
			else if (addedItem is NotebookMenuItem)
			{
				AddNoteButtonVisible = true;
				MenuContext.SelectedTag = null;
				MenuContext.SelectedNotebook = (NotebookMenuItem)addedItem;
				SelectedMenuItem = MenuContext.SelectedNotebook;
			}
			else if (addedItem is ListViewItem)
			{
				ListViewItem item = (ListViewItem)addedItem;
				switch (item.Name)
				{
					case "Favorites":
						SelectedMenuItem = _favoritesLibraryMenuItem;
						break;
					case "AllNotes":
						SelectedMenuItem = _allLibraryMenuItem;
						break;
					case "Trash":
						SelectedMenuItem = _trashLibraryMenuItem;
						break;
					case "Recent":
						SelectedMenuItem = _recentLibraryMenuItem;
						break;
				}
				MenuContext.SelectedTag = null;
				MenuContext.SelectedNotebook = null;
				AddNoteButtonVisible = false;
			}

			if (MenuContext.SelectedNotebook != null || MenuContext.SelectedTag != null || SelectedMenuItem != null)
				ReloadNoteList(selected);

			return true;
		}

		private List<NoteViewModel> GetNoteViewModels(List<Note> notes)
		{
			List<NoteViewModel> models = new List<NoteViewModel>();

			foreach (Note note in notes)
			{
				var ntvm = GetNoteViewModel(note);
				ntvm.Visible = true;
				models.Add(ntvm);
			}

			return models;
		}

		private NoteViewModel GetNoteViewModel(Note note)
		{
			NoteViewModel ntvm = null;
			if (NoteViewModels.ContainsKey(note.ID))
				ntvm = NoteViewModels[note.ID];
			if (ntvm == null)
			{
				ntvm = new NoteViewModel(note);
				NoteViewModels.Add(note.ID, ntvm);
			}
			return ntvm;
		}

		private void InitMainMenu()
		{
			List<NotebookMenuItem> items = new List<NotebookMenuItem>();

			foreach (Notebook notebook in Hub.Instance.Storage.GetNotebooks())
			{
				var item = new NotebookMenuItem(notebook);
				NotebookMenuItems.Add(item);
				items.Add(item);
			}

			items.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

			MenuContext = new NotebookMenuViewModel
			{
				Notebooks = new ObservableCollection<NotebookMenuItem>(items),
				SelectedItemChangedCommand = new SimpleCommand(SelectedNoteContainerChanged),
				RenameItemCommand = new SimpleCommand(RenameItem),
				DeleteItemCommand = new SimpleCommand(DeleteItem),
				AddNotebookCommand = new SimpleCommand(CreateNotebook),
				EmptyTrashCommand = new SimpleCommand(EmptyTrash),
				PasteNoteCommand = new SimpleCommand(PasteNote)
			};

			if (!Hub.Instance.EncryptionManager.SecureNotesEnabled && items.Any())
			{
				MenuContext.SelectedNotebook = items[0];
				MenuContext.SelectedNotebook.IsSelected = true;
			}

			BindingOperations.EnableCollectionSynchronization(MenuContext.Notebooks, _mainMenuListLockObject);
		}

		private void PasteNote(object obj)
		{
			Notebook notebook = obj as Notebook;
			if (notebook != null)
			{
				DataFormat format = DataFormats.GetDataFormat(typeof(NoteClipboardData).FullName);
				if (!Clipboard.ContainsData(format.Name))
					return;

				NoteClipboardData data = Clipboard.GetData(format.Name) as NoteClipboardData;
				if (data != null)
				{
					int index = Hub.Instance.Storage.GetNoteCount(notebook);
					Note note = CreateNewNote(data.Name, notebook.ID, false, index, false);
					note.Text = data.Text;
					if (data.Files != null && data.Files.Any())
					{
						foreach (ClipboardFile file in data.Files)
						{
							NoteFile nf = NoteFile.Create(file.Name, file.MimeType, file.Data, note);
							note.Text = note.Text.Replace(file.FileName, nf.FileName);
						}
					}

					note.Save();

					FocusNote(note);
				}
			}

		}

		private void EmptyTrash(object arg)
		{
			var settings = DialogHelpers.GetDefaultDialogSettings();

			MainWindowInstance.ShowMessageAsync("Empty trash", "Do you want to delete all notes in the trashcan?\nThis operation can not be undone.", MessageDialogStyle.AffirmativeAndNegative, settings).
				ContinueWith(delegate (Task<MessageDialogResult> task)
				{
					if (task.Result == MessageDialogResult.Affirmative)
					{
						foreach (NoteViewModel n in NoteMenuContext.DataSource)
						{
							Guid noteId = n.Note.ID;
							if (NoteViewModels.ContainsKey(noteId))
							{
								n.Note.Delete();
								NoteViewModels.Remove(noteId);
							}
						}
						NoteMenuContext.SelectedNote = null;
						NoteMenuContext.DataSource.Clear();
					}
				});
		}

		private void CreateNotebook(object arg)
		{
			MainWindowInstance.ShowInputAsync("Add notebook", "Name:").ContinueWith(delegate (Task<string> task)
			{
				string newName = task.Result;
				if (string.IsNullOrEmpty(newName))
					return;

				Notebook nb = new Notebook
				{
					Name = newName,
					Created = DateTime.Now,
					SortIndex = 0
				};
				nb.Save();
				var item = new NotebookMenuItem(nb);

				List<NotebookMenuItem> temp = new List<NotebookMenuItem>();
				temp.AddRange(MenuContext.Notebooks.ToArray());
				temp.Add(item);
				temp.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

				MenuContext.Notebooks.Clear();
				temp.ForEach(MenuContext.Notebooks.Add);

				MenuContext.SelectedNotebook = item;
				MenuContext.SelectedNotebook.IsSelected = true;
			});
		}

		private void DeleteItem(object arg)
		{
			if (SelectedMenuItem is TagMenuItem)
				return;

			if (SelectedMenuItem is NotebookMenuItem)
				DeleteCurrentNoteBook();
		}

		private void RenameNoteItem(object arg)
		{
			if (NoteMenuContext.SelectedNote != null)
			{
				string name = NoteMenuContext.SelectedNote.Note.Name;
				var settings = DialogHelpers.GetDefaultDialogSettings();
				settings.DefaultText = name;
				MainWindowInstance.ShowInputAsync("Rename", "Enter new name:", settings).ContinueWith(delegate (Task<string> task)
				{
					string newName = task.Result;
					if (!string.IsNullOrWhiteSpace(newName))
					{
						NoteMenuContext.SelectedNote.Note.Name = newName;
						//NoteMenuContext.SelectedNote.Note.Save();
					}
				});
			}
		}

		private void RenameItem(object arg)
		{
			if (SelectedMenuItem is TagMenuItem)
				return;

			string name = SelectedMenuItem.Name;
			var settings = DialogHelpers.GetDefaultDialogSettings();
			settings.DefaultText = name;
			MainWindowInstance.ShowInputAsync("Rename", "Enter new name:", settings).ContinueWith(delegate (Task<string> task)
			   {
				   string newName = task.Result;
				   if (!string.IsNullOrWhiteSpace(newName))
				   {
					   if (SelectedMenuItem is NotebookMenuItem)
					   {
						   MenuContext.SelectedNotebook.Notebook.Name = newName;
						   MenuContext.SelectedNotebook.Notebook.Save();
					   }
				   }
			   });
		}

		private void CreateNewSecureNote()
		{
			if (!Hub.Instance.EncryptionManager.SecureNotesEnabled)
			{
				MainWindowInstance.ShowMessageAsync("Secure notes", "To create secure notes you need to activate this function in settings.");
				return;
			}

			CreateNewNote(true);
		}

		private void CreateNewNote()
		{
			CreateNewNote(false);
		}

		private void CreateNewNote(bool secure)
		{
			var settings = DialogHelpers.GetDefaultDialogSettings();

			MainWindowInstance.ShowInputAsync("New note", "Name of the new note:", settings).ContinueWith(delegate (Task<string> task)
			{
				string name = task.Result;
				if (!string.IsNullOrWhiteSpace(name))
				{
					CreateNewNote(name, secure);
				}
			});
		}

		#region -- Delete stuff --

		private void DeleteCurrentNoteBook()
		{
			if (MenuContext.SelectedNotebook != null)
			{
				var notes = Hub.Instance.Storage.GetNotes(MenuContext.SelectedNotebook.Notebook);
				if (!notes.Any())
				{
					var settings = DialogHelpers.GetDefaultDialogSettings();

					if (MenuContext.Notebooks.Count == 1)
					{
						MainWindowInstance.ShowMessageAsync("Delete", "You can't delete the last notebook, rename it if the name is wrong.", MessageDialogStyle.Affirmative, settings);
						return;
					}

					MainWindowInstance.ShowMessageAsync("Delete", $"Do you want to delete the notebook {MenuContext.SelectedNotebook.Name}?", MessageDialogStyle.AffirmativeAndNegative, settings).ContinueWith(delegate (Task<MessageDialogResult> task)
					{
						if (task.Result == MessageDialogResult.Affirmative)
						{
							InvokeOnCurrentDispatcher(() =>
							{
								int index = MenuContext.Notebooks.IndexOf(MenuContext.SelectedNotebook);
								if (index >= MenuContext.Notebooks.Count - 1)
									index--;

								MenuContext.SelectedNotebook.Notebook.Delete();

								if (MenuContext.Notebooks.Contains(MenuContext.SelectedNotebook))
									MenuContext.Notebooks.Remove(MenuContext.SelectedNotebook);

								if (NotebookMenuItems.Contains(MenuContext.SelectedNotebook))
									NotebookMenuItems.Remove(MenuContext.SelectedNotebook);

								MenuContext.SelectedNotebook = MenuContext.Notebooks[index];
							});
						}
					});
				}
				else
				{
					MainWindowInstance.ShowMessageAsync("Delete", "You can't delete a notebook that contains notes.");
				}
			}
		}

		private void DeleteCurrentNote()
		{
			if (SelectedMenuItem == _trashLibraryMenuItem)
				return;

			if (NoteMenuContext.SelectedNote.Note.Protected)
			{
				ShowQuickMessage("You cant delete this note, it's protected from that.");
				return;
			}

			var settings = DialogHelpers.GetDefaultDialogSettings();

			MainWindowInstance.ShowMessageAsync("Delete", $"Do you want to delete {NoteMenuContext.SelectedNote.Note.Name}?", MessageDialogStyle.AffirmativeAndNegative, settings).ContinueWith(delegate (Task<MessageDialogResult> task)
			{
				if (task.Result == MessageDialogResult.Affirmative)
				{
					InvokeOnCurrentDispatcher(() =>
					{
						Guid noteId = NoteMenuContext.SelectedNote.Note.ID;
						if (NoteViewModels.ContainsKey(noteId))
						{
							//NoteMenuContext.SelectedNote.Note.Delete();
							NoteMenuContext.SelectedNote.Note.InTrashCan = true;
							//NoteMenuContext.SelectedNote.Note.Save();

							var nvm = NoteViewModels[noteId];
							NoteViewModels.Remove(noteId);
							NoteMenuContext.DataSource.Remove(nvm);
							NoteMenuContext.SelectedNote = NoteMenuContext.DataSource.FirstOrDefault();
							Hub.Instance.Settings.RefreshTags();
						}
					});
				}
			});
		}

		#endregion

		private void CreateNewNote(string name, bool secure = false)
		{
			CreateNewNote(name, MenuContext.SelectedNotebook.Notebook.ID, secure, NoteMenuContext.DataSource.Count);
		}

		private Note CreateNewNote(string name, Guid notebookId, bool secure, int sortIndex, bool focusNote = true)
		{
			Note note = new Note
			{
				ID = Guid.NewGuid(),
				Name = name,
				Notebook = notebookId,
				Created = DateTime.Now,
				Encrypted = secure,
				SortIndex = sortIndex
			};

			note.Save();

			if (focusNote)
				FocusNote(note);

			return note;
		}

		private void FocusNote(Note note)
		{
			var nvm = GetNoteViewModel(note);
			InvokeOnCurrentDispatcher(() =>
			{
				NoteMenuContext.DataSource.Add(nvm);
				NoteMenuContext.SelectedNote = nvm;
			});
		}

		public void Dispose()
		{
			_quickMessageTimer?.Dispose();
			_intervallTimer?.Dispose();
			_searchTimer?.Dispose();

		}
	}
}
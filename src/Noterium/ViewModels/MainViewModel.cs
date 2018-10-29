using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using Noterium.Code.Commands;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Core.Constants;
using Noterium.Core.DataCarriers;
using Noterium.Core.Utilities;
using Noterium.Properties;

namespace Noterium.ViewModels
{
    public class MainViewModel : NoteriumViewModelBase, IDisposable
    {
        private readonly Timer _quickMessageTimer;


        private readonly object _searchResultLockObject = new object();
        private readonly Timer _searchTimer;
        private bool _addNoteButtonVisible;

        private Timer _intervallTimer;

        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            MainWindowLoadedCommand = new RelayCommand(MainWindowLoaded);
            ToggleNoteMenu = new SimpleCommand(ToggleNoteMenuVisibility);
            ToggleNotebookMenu = new SimpleCommand(ToggleNotebookMenuVisibility);
            ToggleSearchFlyoutCommand = new SimpleCommand(ToggleSearchFlyout);
            //SearchTextChangedCommand = new SimpleCommand(Search);
            SearchResultSelectedCommand = new SimpleCommand(SearchResultSelected);
            CloseSearchCommand = new RelayCommand(CloseSearch);
            ToggleHelpViewCommand = new RelayCommand(ToggleHelpView);
            PerformLockActionsCommand = new RelayCommand(PerformLockActions);
            ToggleSettingsFlyoutCommand = new RelayCommand(ToggleSettingsFlyout);
            NoteViewModels = new Dictionary<Guid, NoteViewModel>();
            TopMainMenuItems = new ObservableCollection<TopMainMenuItemViewModel>();
            SearchResult = new ObservableCollection<NoteViewModel>();
            EditNoteCommand = new RelayCommand(() => { MessengerInstance.Send(new ChangeViewMode(NoteViewModes.Edit)); });

            BindingOperations.EnableCollectionSynchronization(SearchResult, _searchResultLockObject);

            PropertyChanged += MainViewModelPropertyChanged;

            Hub.Instance.Settings.PropertyChanged += SettingsPropertyChanged;

            Hub.Instance.EncryptionManager.PropertyChanged += SettingsPropertyChanged;
            IsSecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;

            _quickMessageTimer = new Timer {AutoReset = false};
            _quickMessageTimer.Elapsed += QuickMessageTimerElapsed;
            _quickMessageTimer.Interval = 2000;

            _searchTimer = new Timer {AutoReset = false};
            _searchTimer.Elapsed += SearchTimerElapsed;
            _searchTimer.Interval = 250;

            HelpDocumentText = Resources.Help_Document;

            MessengerInstance.Register<ConfigureControlsForParnetType>(this, ConfigureControlsForParentType);
            MessengerInstance.Register<QuickMessage>(this, ShowQuickMessage);
        }

        public ICommand ToggleNoteMenu { get; set; }
        public ICommand ToggleHelpViewCommand { get; set; }
        public ICommand ToggleNotebookMenu { get; set; }
        public ICommand MainWindowLoadedCommand { get; set; }
        public ICommand LockCommand { get; set; }
        public ICommand ToggleSearchFlyoutCommand { get; set; }
        public ICommand SearchTextChangedCommand { get; set; }
        public ICommand SearchResultSelectedCommand { get; set; }
        public ICommand CloseSearchCommand { get; set; }
        public ICommand PerformLockActionsCommand { get; set; }
        public ICommand ToggleSettingsFlyoutCommand { get; set; }
        public ICommand EditNoteCommand { get; set; }

        public Dictionary<Guid, NoteViewModel> NoteViewModels { get; set; }
        public ObservableCollection<TopMainMenuItemViewModel> TopMainMenuItems { get; set; }

        public void Dispose()
        {
            _quickMessageTimer?.Dispose();
            _intervallTimer?.Dispose();
            _searchTimer?.Dispose();
        }

        private void ConfigureControlsForParentType(ConfigureControlsForParnetType obj)
        {
            if (obj.Type == ConfigureControlsForParnetType.ParentType.Tag || obj.Type == ConfigureControlsForParnetType.ParentType.Library)
            {
                AddNoteContextMenuButtonVisible = false;
                AddNoteButtonVisible = false;
            }
            else if (obj.Type == ConfigureControlsForParnetType.ParentType.Notebook)
            {
                if (Hub.Instance.EncryptionManager.SecureNotesEnabled)
                {
                    AddNoteButtonVisible = false;
                    AddNoteContextMenuButtonVisible = true;
                }
                else
                {
                    AddNoteContextMenuButtonVisible = false;
                    AddNoteButtonVisible = true;
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
            var model = obj as NoteViewModel;
            if (model != null)
            {
                MessengerInstance.Send(new ReloadNoteMenuList(model.Notebook, model.Note));
                CloseSearch();
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

            InvokeOnCurrentDispatcher(() => { IsSearching = true; });

            var searchResult = Hub.Instance.SearchManager.Search(SearchTerm);

            InvokeOnCurrentDispatcher(() =>
            {
                var models = ViewModelLocator.Instance.GetNoteViewModels(searchResult);

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

        private void ShowQuickMessage(QuickMessage message)
        {
            QuickInformationMessage = message.Message;
            QuickInformationIsOpen = true;
            _quickMessageTimer.Start();
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SecureNotesEnabled") IsSecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;
        }

        public void MainWindowLoaded()
        {
            Init();
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

            _intervallTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _intervallTimer.Elapsed += IntervallTimerOnElapsed;
            _intervallTimer.Enabled = true;
        }

        private SecureString SystemNeedsEncryptionPassword()
        {
            //string password = "abc123";

            var pass = MainWindowInstance.ShowInputAsync("Password required", "Password:").ContinueWith(delegate(Task<string> task)
            {
                var name = task.Result;
                if (!string.IsNullOrWhiteSpace(name)) return FileSecurity.ConvertToSecureString(name);
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
                var date = Hub.Instance.Storage.GetLastBackupDate();
                var ts = DateTime.Now - date;
                if (ts.Days > 0)
                {
                    Hub.Instance.Storage.Backup();
                    Hub.Instance.Storage.CleanBackupData();
                }
            }
        }

        private void MainViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SearchTerm")
                Search();
        }


        private void FocusNote(Note note)
        {
            var nvm = ViewModelLocator.Instance.GetNoteViewModel(note);
            InvokeOnCurrentDispatcher(() =>
            {
                NoteMenuContext.DataSource.Add(nvm);
                NoteMenuContext.SelectedNote = nvm;
            });
        }

        #region -- Static menu items --

        private bool _secureNotesEnabled;
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
        private string _helpDocumentText;

        #endregion

        #region -- Propertys --

        public bool AddNoteContextMenuButtonVisible
        {
            get => _addNoteButtonVisible;
            set
            {
                var oldValue = _addNoteButtonVisible;
                _addNoteButtonVisible = value;
                if (oldValue != value)
                    RaisePropertyChanged();
            }
        }

        public bool AddNoteButtonVisible
        {
            get => _addNoteButtonVisible;
            set
            {
                var oldValue = _addNoteButtonVisible;
                _addNoteButtonVisible = value;
                if (oldValue != value)
                    RaisePropertyChanged();
            }
        }

        public bool SearchFlyoutIsVisible
        {
            get => _searchFlyoutIsVisible;
            set
            {
                _searchFlyoutIsVisible = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Tag> Tags => Hub.Instance.Settings.Tags;

        public NotebookMenuViewModel MenuContext => ViewModelLocator.Instance.NotebookMenu;

        public NoteMenuViewModel NoteMenuContext => ViewModelLocator.Instance.NoteMenu;
        public NoteViewerViewModel NoteViewContext => ViewModelLocator.Instance.NoteView;

        public bool IsSecureNotesEnabled
        {
            get => _secureNotesEnabled;
            set
            {
                _secureNotesEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool QuickInformationIsOpen
        {
            get => _quickInformationIsOpen;
            set
            {
                _quickInformationIsOpen = value;
                RaisePropertyChanged();
            }
        }

        public string QuickInformationMessage
        {
            get => _quickInformationMessage;
            set
            {
                _quickInformationMessage = value;
                RaisePropertyChanged();
            }
        }

        public bool NotebookColumnVisibility
        {
            get => _notebookColumnVisibility;
            set
            {
                _notebookColumnVisibility = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowNoteColumn
        {
            get => _showNoteColumn;
            set
            {
                _showNoteColumn = value;
                RaisePropertyChanged();
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NoteViewModel> SearchResult { get; }


        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                _isSearching = value;
                RaisePropertyChanged();
            }
        }

        public bool IsHelpVisible
        {
            get => _isHelpVisible;
            set
            {
                _isHelpVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool SettingsFlyoutIsVisible
        {
            get => _settingsFlyoutIsVisible;
            set
            {
                _settingsFlyoutIsVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool LibrarysFlyoutIsVisible
        {
            get => _librarysFlyoutIsVisible;
            set
            {
                _librarysFlyoutIsVisible = value;
                RaisePropertyChanged();
            }
        }


        public string HelpDocumentText
        {
            get => _helpDocumentText;
            set
            {
                _helpDocumentText = value;
                RaisePropertyChanged();
            }
        }

        public bool Loaded { get; internal set; }

        #endregion
    }
}
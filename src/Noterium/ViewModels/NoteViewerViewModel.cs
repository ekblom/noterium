using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Noterium.Code.Commands;
using Noterium.Code.Markdown;
using Noterium.Code.Messages;
using Noterium.Core;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
    public class NoteViewerViewModel : NoteriumViewModelBase
    {
        private NoteViewModel _noteViewModel;

        private bool _secureNotesEnabled;

        public NoteViewerViewModel()
        {
            PropertyChanged += NoteViewModelPropertyChanged;

            Hub.Instance.EncryptionManager.PropertyChanged += NoteViewModelPropertyChanged;
            IsSecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;

            DocumentCheckBoxCheckedCommand = new SimpleCommand(DocumentCheckBoxChecked);
            MessengerInstance.Register<SelectedNoteChanged>(this, UpdateSelectedNote);

            // Force selection of first note sincen NoteMenuViewModel is instantiated before this.
            CurrentNote = ViewModelLocator.Instance.NoteMenu.SelectedNote;
        }

        public ICommand EditNoteCommand { get; set; }
        public ICommand SaveNoteCommand { get; set; }
        public ICommand DocumentCheckBoxCheckedCommand { get; set; }
        public ICommand CheckBoxCheckUpdatedTextCommand { get; set; }
        public ICommand CopyNoteCommand { get; set; }
        public ICommand RenameNoteCommand { get; set; }

        public NoteViewModel CurrentNote
        {
            get => _noteViewModel;
            set
            {
                _noteViewModel = value;
                RaisePropertyChanged();
            }
        }

        public Settings Settings => Hub.Instance.Settings;

        public bool IsSecureNotesEnabled
        {
            get => _secureNotesEnabled;
            set
            {
                _secureNotesEnabled = value;
                RaisePropertyChanged();
            }
        }

        public TextToFlowDocumentConverter MarkdownConverter { get; set; }

        public bool Loaded { get; internal set; }

        private void UpdateSelectedNote(SelectedNoteChanged obj)
        {
            if (MarkdownConverter != null)
            {
                if (obj.SelectedNote != null)
                    MarkdownConverter.CurrentNote = obj.SelectedNote.Note;
                else
                    MarkdownConverter.CurrentNote = null;
            }

            CurrentNote = obj.SelectedNote;
        }

        private void DocumentCheckBoxChecked(object arg)
        {
            var cb = arg as CheckBox;
            if (cb != null)
            {
                var number = (int) cb.Tag;

                var regString = "^" + SharedSettings.MarkerToDo;
                var reg = new Regex(regString, RegexOptions.Compiled | RegexOptions.Singleline);
                var replaceRegex = @"\[(?:\s|x)\]";
                var cbNumber = 0;
                var lines = CurrentNote.Note.DecryptedText.Split('\n');
                for (var index = 0; index < lines.Length; index++)
                {
                    var line = lines[index];
                    if (reg.IsMatch(line))
                    {
                        if (cbNumber == number)
                        {
                            var isChecked = cb.IsChecked ?? false;

                            var isCheckedString = isChecked ? "[x]" : "[ ]";
                            //string text = line.Substring(line.LastIndexOf("]", StringComparison.Ordinal) + 2);
                            lines[index] = Regex.Replace(line, replaceRegex, isCheckedString);
                            break;
                        }

                        cbNumber++;
                    }
                }

                CurrentNote.Note.DecryptedText = string.Join("\n", lines);
                CheckBoxCheckUpdatedTextCommand?.Execute(CurrentNote.Note.DecryptedText);
            }
        }

        private void NoteViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Note")
            {
            }
            else if (e.PropertyName == "SecureNotesEnabled")
            {
                IsSecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;
            }
        }
    }
}
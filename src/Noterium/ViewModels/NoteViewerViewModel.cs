using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using Noterium.Code.Commands;
using Noterium.Code.Helpers;
using Noterium.Code.Markdown;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using Noterium.Code.Messages;
using System;

namespace Noterium.ViewModels
{
	public class NoteViewerViewModel : NoteriumViewModelBase
	{
		private NoteViewModel _noteViewModel;

		public ICommand EditNoteCommand { get; set; }
		public ICommand SaveNoteCommand { get; set; }
		public ICommand DocumentCheckBoxCheckedCommand { get; set; }
		public ICommand CheckBoxCheckUpdatedTextCommand { get; set; }
		public ICommand CopyNoteCommand { get; set; }
		public ICommand RenameNoteCommand { get; set; }

		private bool _secureNotesEnabled;
		private NoteFile _selectedNoteFile;
		private TextToFlowDocumentConverter _markdownConverter;

		public NoteViewModel CurrentNote
		{
			get { return _noteViewModel; }
			set { _noteViewModel = value; RaisePropertyChanged(); }
		}

		public Settings Settings => Hub.Instance.Settings;

		public bool IsSecureNotesEnabled
		{
			get { return _secureNotesEnabled; }
			set { _secureNotesEnabled = value; RaisePropertyChanged(); }
		}

		public TextToFlowDocumentConverter MarkdownConverter
		{
			get { return _markdownConverter; }
			set { _markdownConverter = value; }
		}

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

		private void UpdateSelectedNote(SelectedNoteChanged obj)
		{
			if(MarkdownConverter != null)
				MarkdownConverter.CurrentNote = obj.SelectedNote.Note;
			CurrentNote = obj.SelectedNote;
		}

		private void DocumentCheckBoxChecked(object arg)
		{
			CheckBox cb = arg as CheckBox;
			if (cb != null)
			{
				int number = (int)cb.Tag;

				string regString = "^" + SharedSettings.MarkerToDo;
				Regex reg = new Regex(regString, RegexOptions.Compiled | RegexOptions.Singleline);
				string replaceRegex = @"\[(?:\s|x)\]";
				int cbNumber = 0;
				string[] lines = CurrentNote.Note.DecryptedText.Split('\n');
				for (int index = 0; index < lines.Length; index++)
				{
					string line = lines[index];
					if (reg.IsMatch(line))
					{
						if (cbNumber == number)
						{
							bool isChecked = cb.IsChecked ?? false;

							string isCheckedString = isChecked ? "[x]" : "[ ]";
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

		void NoteViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
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
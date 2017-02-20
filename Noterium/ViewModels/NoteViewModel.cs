using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using Noterium.Code.Commands;
using Noterium.Code.Helpers;
using Noterium.Code.Markdown;
using Noterium.Controls;
using Noterium.Core;
using Noterium.Core.DataCarriers;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;
using Settings = Noterium.Core.Settings;

namespace Noterium.ViewModels
{
	public class NoteViewModel : NoteriumViewModelBase, IDragSource, IDropTarget
	{
		public ICommand EditNoteCommand { get; set; }
		public ICommand SaveNoteCommand { get; set; }
		public ICommand DocumentCheckBoxCheckedCommand { get; set; }
		public ICommand CheckBoxCheckUpdatedTextCommand { get; set; }

		public ICommand OpenFileCommand { get; set; }
		public ICommand RenameFileCommand { get; set; }
		public ICommand DeleteFileCommand { get; set; }

		private string _newToDoItemText;
		private bool _visible = true;
		private bool _secureNotesEnabled;
		private Notebook _notebook;
		private bool _isSelected;
		private NoteFile _selectedNoteFile;

		public ObservableCollection<TokenizedTagItem> Tags { get; internal set; }

		public Note Note { get; }

		public Settings Settings => Hub.Instance.Settings;

		public string NewToDoItemText
		{
			get { return _newToDoItemText; }
			set { _newToDoItemText = value; RaisePropertyChanged(); }
		}

		public bool IsSecureNotesEnabled
		{
			get { return _secureNotesEnabled; }
			set { _secureNotesEnabled = value; RaisePropertyChanged(); }
		}

		public string CreatedDateText
		{
			get
			{
				if (Note.Created.Year == DateTime.Now.Year)
				{
					TimeSpan ts = DateTime.Now - Note.Created;
					if (ts.TotalMinutes < 1)
						return "now";
					if (ts.TotalHours < 1)
						return Convert.ToInt32(Math.Floor(ts.TotalMinutes)) + " min";
					if (ts.TotalDays > 1 && ts.TotalDays < 31)
					{
						int totalDays = Convert.ToInt32(Math.Floor(ts.TotalDays));
						string suffix = totalDays == 1 ? " day" : " days";
						return totalDays + suffix;
					}
					if (Note.Created.Month != DateTime.Now.Month)
						return Note.Created.ToString("dd MMM");
				}

				return Note.Created.ToString("d MMMM, yyyy");
			}
		}

		public bool Visible
		{
			get { return _visible; }
			set { _visible = value; RaisePropertyChanged(); }
		}

		public bool IsDirty { get; set; }

		public NoteViewModel(Note note)
		{
			Note = note;
			Notebook = Hub.Instance.Storage.GetNotebook(note.Notebook);

			InitCommands();

			Tags = new ObservableCollection<TokenizedTagItem>(Note.Tags.ToList().ConvertAll(t => new TokenizedTagItem(t)));
			Tags.CollectionChanged += TagsCollectionChanged;

			Note.PropertyChanged += NotePropertyChanged;

			PropertyChanged += NoteViewModelPropertyChanged;

			Hub.Instance.EncryptionManager.PropertyChanged += NoteViewModelPropertyChanged;
			IsSecureNotesEnabled = Hub.Instance.EncryptionManager.SecureNotesEnabled;

			OpenFileCommand = new SimpleCommand(OpenFile);
			RenameFileCommand = new SimpleCommand(RenameFile);
			DeleteFileCommand = new SimpleCommand(DeleteFile);
		}

		public override string ToString()
		{
			return Note.ToString();
		}

		private void DeleteFile(object obj)
		{
			NoteFile nf = SelectedNoteFile;
			if (nf != null)
			{
				MessageBox.Show("Delete " + nf.Name);
			}
		}

		private void RenameFile(object obj)
		{
			NoteFile nf = SelectedNoteFile;
			if (nf != null)
			{
				string name = nf.Name;
				var settings = DialogHelpers.GetDefaultDialogSettings();
				settings.DefaultText = name;
				MainWindowInstance.ShowInputAsync("Rename", "Enter new name:", settings).ContinueWith(delegate (Task<string> task)
				{
					string newName = task.Result;
					if (task.IsCanceled)
						return;

					if (!string.IsNullOrWhiteSpace(newName))
					{
						var existingNoteFile = Note.Files.FirstOrDefault(enf => enf.Name.Equals(newName));
						if (existingNoteFile == null)
						{
							nf.Name = newName;
							Note.DecryptedText = Note.DecryptedText.Replace($"[!{name}]", $"[!{newName}]");
							IsDirty = true;
						}
						else
						{
							InvokeOnCurrentDispatcher(() =>
							{
								MainWindowInstance.ShowMessageAsync("Error", "You already have a file in this note called " + newName);
								RenameFile(obj);
							});
						}
					}
				});
			}
		}

		private void OpenFile(object arg)
		{
			if (arg != null)
			{
				NoteFile nf = (NoteFile)arg;
				if (File.Exists(nf.FullName))
					System.Diagnostics.Process.Start(nf.FullName);
			}
		}

		public Notebook Notebook
		{
			get { return _notebook; }
			private set { _notebook = value; RaisePropertyChanged(); }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; RaisePropertyChanged(); }
		}


		private void InitCommands()
		{
			DocumentCheckBoxCheckedCommand = new BasicCommand(DocumentCheckBoxChecked);
		}

		private bool DocumentCheckBoxChecked(object arg)
		{
			CheckBox cb = arg as CheckBox;
			if (cb != null)
			{
				int number = (int)cb.Tag;

				string regString = "^" + SharedSettings.MarkerToDo;
				Regex reg = new Regex(regString, RegexOptions.Compiled | RegexOptions.Singleline);
				string replaceRegex= @"\[(?:\s|x)\]";
				int cbNumber = 0;
				string[] lines = Note.DecryptedText.Split('\n');
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

				Note.DecryptedText = string.Join("\n", lines);
				CheckBoxCheckUpdatedTextCommand?.Execute(Note.DecryptedText);
			}
			return true;
		}

		void TagsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (object o in e.OldItems)
				{
					string tagName = (string)o;
					if (tagName != null)
					{
						Note.Tags.Remove(tagName);
					}
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (object o in e.NewItems)
				{
					string tagName = (string)o;
					if (tagName != null)
					{
						Note.Tags.Add(tagName);
					}
				}
			}
		}

		void NotePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Text")
				return;

			if (e.PropertyName != "Changed")
				IsDirty = true;

			if (e.PropertyName == "Encrypted")
			{
				// Force save note so we encrypt correct things
				Hub.Instance.EncryptionManager.SwitchTextEncryptionMode(Note);
				SaveNote(true);
			}
			else if (e.PropertyName == "Notebook")
			{
				Notebook = Hub.Instance.Storage.GetNotebook(Note.Notebook);
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

		public void SaveNote(bool force = false)
		{
			if (!force)
				if (!IsDirty || IsSaving)
					return;

			// NOTE: Possibly a bit unsafe when editing a note, might miss chars, but what the heck.
			IsDirty = false;
			IsSaving = true;
			Note.Save();
			IsSaving = false;

			InvokeOnCurrentDispatcher(() =>
			{
				Hub.Instance.Settings.RefreshTags();
			});
		}

		public bool IsSaving { get; set; }

		public NoteFile SelectedNoteFile
		{
			get { return _selectedNoteFile; }
			set { _selectedNoteFile = value; RaisePropertyChanged(); }
		}

		public void StartDrag(IDragInfo dragInfo)
		{
			DragDrop.DefaultDragHandler.StartDrag(dragInfo);
		}

		public bool CanStartDrag(IDragInfo dragInfo)
		{
			return true;
		}

		public void Dropped(IDropInfo dropInfo)
		{
		}

		public void DragCancelled()
		{
		}

		public bool TryCatchOccurredException(Exception exception)
		{
			return true;
		}

		public void DragOver(IDropInfo dropInfo)
		{
			dropInfo.Effects = DragDropEffects.Copy;

			TextBox box = dropInfo.VisualTarget as TextBox;
			if (box != null)
			{
				var index = box.GetCharacterIndexFromPoint(dropInfo.DropPosition, true);
				if (index > -1)
				{
					box.Focus();
					box.CaretIndex = index;
				}
			}
		}

		public void Drop(IDropInfo dropInfo)
		{
			if (dropInfo.Data is NoteFile)
			{
				TextBox box = dropInfo.VisualTarget as TextBox;
				if (box != null)
				{
					AddFilesToTextBox(box, dropInfo, new List<NoteFile> { (NoteFile)dropInfo.Data });
				}
			}
			else if (dropInfo.Data is DataObject)
			{
				DataObject dataObject = (DataObject)dropInfo.Data;
				if (dataObject.ContainsFileDropList())
				{
					var fileList = dataObject.GetFileDropList();
					List<NoteFile> createdFiles = new List<NoteFile>();
					foreach (string filePath in fileList)
					{
						if (File.Exists(filePath))
						{
							FileInfo fi = new FileInfo(filePath);
							var file = File.ReadAllBytes(filePath);

							string ext = Path.GetExtension(filePath);
							if (!string.IsNullOrWhiteSpace(ext))
								ext = ext.ToLower();

							string noteFileName = NoteFile.ProposeNoteFileName(fi.Name, Note);

							string mime = MimeTypes.MimeTypeMap.GetMimeType(ext);
							var nf = NoteFile.Create(noteFileName, mime, file, Note, ext);
							createdFiles.Add(nf);
						}
					}
					if (dropInfo.VisualTarget is TextBox)
					{
						AddFilesToTextBox((TextBox)dropInfo.VisualTarget, dropInfo, createdFiles);
					}
				}
			}
		}



		private void AddFilesToTextBox(TextBox box, IDropInfo dropInfo, List<NoteFile> files)
		{
			var index = box.GetCharacterIndexFromPoint(dropInfo.DropPosition, true);
			if (index > -1)
			{
				box.Focus();
				box.CaretIndex = index;
			}

			StringBuilder builder = new StringBuilder();
			foreach (NoteFile nf in files)
			{
				if (nf.IsImage)
					builder.Append(nf.GetAsImageMarkdown());
				else
					builder.AppendLine(nf.GetAsFileLink());
			}

			box.SelectedText = builder.ToString();
		}
	}
}
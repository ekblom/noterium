using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using MimeTypes;
using Noterium.Code.Commands;
using Noterium.Core.DataCarriers;
using Noterium.Core.Helpers;
using Noterium.ViewModels;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Noterium.Components.NoteMenu
{
	public class NoteMenuViewModel : NoteriumViewModelBase, IDragSource, IDropTarget
	{
		private NoteViewModel _selectedNote;
		private string _sortMode = "Index";

		public NoteMenuViewModel()
		{
			DataSource = new ObservableCollection<NoteViewModel>();

			FilterNotesCommand = new SimpleCommand(FilterNotes);
			ClearFilterCommand = new SimpleCommand(ClearFilter);
			ShowNoteCommandsCommand = new SimpleCommand(ShowNoteCommands);
			
			PropertyChanged += OnPropertyChanged;

			var saveTimer = new Timer(1000)
			{
				AutoReset = true,
				Enabled = true
			};
			saveTimer.Elapsed += SaveNotIfDirty;
			saveTimer.Start();
		}

		
		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SortMode")
			{
				UpdateDataSource(new List<NoteViewModel>(DataSource));
			}
		}

		private void ShowNoteCommands(object o)
		{
			if (o is ListViewItem)
			{

			}
		}

		private void SaveNotIfDirty(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (SelectedNote != null && SelectedNote.IsDirty)
				SelectedNote.SaveNote();
		}

		private void ClearFilter(object arg)
		{
			foreach (NoteViewModel model in DataSource)
				model.Visible = true;
		}

		private void FilterNotes(object o)
		{
			var arg = o as TextChangedEventArgs;
			if (arg != null)
			{
				TextBox tb = (TextBox)arg.OriginalSource;

				foreach (NoteViewModel model in DataSource)
				{
					if (model.Note.Name.Contains(tb.Text) || ContainsTag(model, tb.Text) || model.Note.DecryptedText.Contains(tb.Text))
					{
						model.Visible = true;
					}
					else
					{
						model.Visible = false;
					}
				}
			}
		}

		private bool ContainsTag(NoteViewModel model, string text)
		{
			return model.Tags.Any(t => t.Text.Contains(text));
		}

		public ICommand SelectedItemChangedCommand { get; set; }
		public ICommand DeleteItemCommand { get; set; }
		public ICommand EditItemCommand { get; set; }
		public ICommand FilterNotesCommand { get; set; }
		public ICommand ClearFilterCommand { get; set; }
		public ICommand RenameItemCommand { get; set; }
		public ICommand AddNoteCommand { get; set; }

		public ICommand CopyNoteCommand { get; set; }

		public ICommand ShowNoteCommandsCommand { get; set; }

		public ObservableCollection<NoteViewModel> DataSource { get; }

		public string SortMode
		{
			get { return _sortMode; }
			set { _sortMode = value; RaisePropertyChanged(); }
		}

		public NoteViewModel SelectedNote
		{
			get { return _selectedNote; }
			set
			{
				Stopwatch clock = Stopwatch.StartNew();

				if (_selectedNote != null)
				{
					_selectedNote.SaveNote();
					_selectedNote.IsSelected = false;
				}
				_selectedNote = value;
				if (_selectedNote != null)
					_selectedNote.IsSelected = true;

				Console.WriteLine($"Change selected note took {clock.ElapsedMilliseconds} ms.");

				RaisePropertyChanged();

				clock.Stop();
				Console.WriteLine($"Raise property changes took {clock.ElapsedMilliseconds} ms");
			}
		}


		#region dragdrop


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
			ObservableCollection<NoteViewModel> targetList = dropInfo.TargetCollection as ObservableCollection<NoteViewModel>;
			if (targetList != null)
			{
				for (int i = 0; i < targetList.Count; i++)
				{
					NoteViewModel nvm = targetList[i];
					nvm.Note.SortIndex = i;
					nvm.SaveNote();
				}
			}
		}

		public void DragCancelled()
		{

		}

		public bool TryCatchOccurredException(Exception exception)
		{
			return false;
		}

		#endregion

		public void DragOver(IDropInfo dropInfo)
		{
			if (SortMode != "Index")
				dropInfo.Effects = DragDropEffects.None;
			else
				DragDrop.DefaultDropHandler.DragOver(dropInfo);
		}

		public void Drop(IDropInfo dropInfo)
		{
			if (SortMode != "Index")
				return;

			DragDrop.DefaultDropHandler.Drop(dropInfo);
		}

		public void UpdateDataSource(List<NoteViewModel> models)
		{
			if (SortMode == "Index")
				models.Sort((x, y) => x.Note.SortIndex.CompareTo(y.Note.SortIndex));
			else
			{
				models.Sort((x, y) => String.Compare(x.Note.Name, y.Note.Name, StringComparison.Ordinal));
				if (SortMode == "ZA")
					models.Reverse();
			}

			DataSource.Clear();
			models.ForEach(DataSource.Add);
		}
	}
}
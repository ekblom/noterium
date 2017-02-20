using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using Noterium.Core;
using Noterium.ViewModels;
using Noterium.Windows;
using Portable.Licensing.Prime;

namespace Noterium
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private bool _loaded;

		public bool IsLocked { get; set; }
		public MainViewModel Model { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			SystemEvents.PowerModeChanged += OnPowerChange;

			if (Hub.Instance.LicenseManager.ValidLicense && Hub.Instance.LicenseManager.License.Type == LicenseType.Trial)
			{
				Title = Title + " - Trail";
			}
		}

		private void AuthenticationForm1_OnAuthenticated()
		{
			AuthenticationForm1.Password.Clear();
			Unlock();
		}

		public void Lock(bool authenticate = false)
		{
			AuthenticationForm1.OnlyVerifyPassword = !authenticate;

			SettingsFlyout.IsOpen = false;
			IsLocked = true;
			MainGrid.Effect = new BlurEffect { Radius = 17 };
			AuthenticationForm1.Reset();
			OverlayPanel.Visibility = Visibility.Visible;
			LockButton.Visibility = Visibility.Collapsed;
			SettingsButton.Visibility = Visibility.Collapsed;

			//if (Model.NoteMenuContext.SelectedNote != null && Model.NoteMenuContext.SelectedNote.Note.Encrypted)
			//{
			// _selectedNoteWhenLocked = Model.NoteMenuContext.SelectedNote;

			//}
		}

		public void Unlock()
		{
			if (Model.SelectedMenuItem == null)
				Model.SelectedNoteContainerChangedCommand.Execute(Model.MenuContext.Notebooks.FirstOrDefault());

			MainGrid.Effect = null;
			OverlayPanel.Visibility = Visibility.Collapsed;
			IsLocked = false;
			LockButton.Visibility = Model.IsSecureNotesEnabled ? Visibility.Visible : Visibility.Collapsed;
			SettingsButton.Visibility = Visibility.Visible;
		}

		#region Treeview drag drop

		//private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
		//{
		//	if (e.ChangedButton == MouseButton.Left)
		//	{
		//		_lastMouseDown = e.GetPosition(tvParameters);
		//	}
		//}

		//private void treeView_MouseMove(object sender, MouseEventArgs e)
		//{
		//	try
		//	{
		//		if (e.LeftButton == MouseButtonState.Pressed)
		//		{
		//			Point currentPosition = e.GetPosition(tvParameters);

		//			if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
		//				(Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
		//			{
		//				draggedItem = (TreeViewItem)tvParameters.SelectedItem;
		//				if (draggedItem != null)
		//				{
		//					DragDropEffects finalDropEffect =
		//		DragDrop.DoDragDrop(tvParameters,
		//			tvParameters.SelectedValue,
		//						DragDropEffects.Move);
		//					//Checking target is not null and item is
		//					//dragging(moving)
		//					if ((finalDropEffect == DragDropEffects.Move) &&
		//			(_target != null))
		//					{
		//						// A Move drop was accepted
		//						if (!draggedItem.Header.ToString().Equals
		//			(_target.Header.ToString()))
		//						{
		//							CopyItem(draggedItem, _target);
		//							_target = null;
		//							draggedItem = null;
		//						}
		//					}
		//				}
		//			}
		//		}
		//	}
		//	catch (Exception)
		//	{
		//	}
		//}

		//private void treeView_DragOver(object sender, DragEventArgs e)
		//{
		//	try
		//	{
		//		Point currentPosition = e.GetPosition(tvParameters);

		//		if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
		//		   (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
		//		{
		//			// Verify that this is a valid drop and then store the drop target
		//			TreeViewItem item = GetNearestContainer
		//			(e.OriginalSource as UIElement);
		//			if (CheckDropTarget(draggedItem, item))
		//			{
		//				e.Effects = DragDropEffects.Move;
		//			}
		//			else
		//			{
		//				e.Effects = DragDropEffects.None;
		//			}
		//		}
		//		e.Handled = true;
		//	}
		//	catch (Exception)
		//	{
		//	}
		//}

		//private void treeView_Drop(object sender, DragEventArgs e)
		//{
		//	try
		//	{
		//		e.Effects = DragDropEffects.None;
		//		e.Handled = true;

		//		// Verify that this is a valid drop and then store the drop target
		//		TreeViewItem TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
		//		if (TargetItem != null && draggedItem != null)
		//		{
		//			_target = TargetItem;
		//			e.Effects = DragDropEffects.Move;
		//		}
		//	}
		//	catch (Exception)
		//	{
		//	}
		//}

		#endregion

		private void AddNoteButtonClick(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			if (button == null)
				return;

			button.ContextMenu.IsEnabled = true;
			button.ContextMenu.PlacementTarget = button;
			button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			button.ContextMenu.IsOpen = true;
		}

		private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (IsLocked)
			{
				if (e.Key == Key.Tab)
					e.Handled = true;
				else if (Keyboard.Modifiers == ModifierKeys.Control)
				{
					if (e.Key == Key.N || e.Key == Key.F)
						e.Handled = true;
				}
			}
		}

		private void SettingsFlyout_OnIsOpenChanged(object sender, RoutedEventArgs e)
		{
			if (SettingsFlyout.IsOpen)
			{
				ShowOverlayPanel();
				LockButton.IsEnabled = false;
			}
			else
			{
				LockButton.IsEnabled = true;
				HideOverlayPanel();
			}
		}

		private void SearchFlyout_OnIsOpenChanged(object sender, RoutedEventArgs e)
		{
			if (SearchFlyout.IsOpen)
			{
				ShowOverlayPanel();
				SearchFlyoutTextBox.Text = string.Empty;
				SearchFlyoutTextBox.Focus();
			}
			else
			{
				SearchFlyoutTextBox.Text = string.Empty;
				HideOverlayPanel();
			}
		}

		private void ShowOverlayPanel()
		{
			MainGrid.Effect = new BlurEffect { Radius = 3 };
			OverlayPanel.Visibility = Visibility.Visible;
			OverlayPanel.MouseDown += OverlayPanelMouseDown;
			LockButton.IsEnabled = false;
			AuthenticationForm.Visibility = Visibility.Collapsed;
		}

		private void HideOverlayPanel()
		{
			OverlayPanel.Visibility = Visibility.Collapsed;
			OverlayPanel.MouseDown -= OverlayPanelMouseDown;
			MainGrid.Effect = null;
			LockButton.IsEnabled = true;
			AuthenticationForm.Visibility = Visibility.Visible;
		}

		private void OverlayPanelMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (SettingsFlyout.IsOpen)
				SettingsFlyout.IsOpen = false;
			if (SearchFlyout.IsOpen)
			{
				Model.CloseSearchCommand.Execute(null);
			}
		}

		private void ColumnOnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!_loaded)
				return;

			bool changed = false;
			if (NoteColumn.ActualWidth > 0)
			{
				Hub.Instance.AppSettings.NoteColumnWidth = NoteColumn.ActualWidth;
				changed = true;
			}
			if (NotebookColumn.ActualWidth > 0)
			{
				Hub.Instance.AppSettings.NotebookColumnWidth = NotebookColumn.ActualWidth;
				changed = true;
			}
			if (changed)
				Hub.Instance.AppSettings.Save();
		}

		private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!_loaded)
				return;

			Hub.Instance.AppSettings.WindowSize = e.NewSize;
			Hub.Instance.AppSettings.Save();
		}

		private void MainWindow_OnStateChanged(object sender, EventArgs e)
		{
			if (!_loaded)
				return;

			Hub.Instance.AppSettings.WindowState = WindowState;
			Hub.Instance.AppSettings.Save();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			NoteColumn.Width = new GridLength(Hub.Instance.AppSettings.NoteColumnWidth);
			NotebookColumn.Width = new GridLength(Hub.Instance.AppSettings.NotebookColumnWidth);

			_loaded = true;
		}

		private void SearchResultList_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (!Model.SearchResult.Any())
				return;

			if (e.Key == Key.Enter)
			{
				Model.SearchResultSelectedCommand.Execute(SearchResultList.SelectedValue);
			}
			else if (e.Key == Key.Escape)
			{
				Model.CloseSearchCommand.Execute(null);
				SearchFlyoutTextBox.Text = string.Empty;
			}
		}

		private void SearchFlyoutTextBox_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				if (Model.SearchResult.Any())
				{
					SearchResultList.SelectedIndex = 0;
					SearchResultList.Focus();
				}
			}
			else if (e.Key == Key.Escape)
			{
				Model.CloseSearchCommand.Execute(null);
				SearchFlyoutTextBox.Text = string.Empty;
			}
		}

		private void SearchResultList_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Up && SearchResultList.SelectedIndex == 0)
			{
				e.Handled = true;
				SearchResultList.SelectedItem = null;
				SearchFlyoutTextBox.Focus();
			}
		}

		private void SearchResultList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (SearchResultList.SelectedValue != null)
				Model.SearchResultSelectedCommand.Execute(SearchResultList.SelectedValue);
		}

		private void HelpOverlayPanel_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsLocked)
				return;

			MainGrid.Effect = HelpOverlayPanel.Visibility == Visibility.Visible ? new BlurEffect { Radius = 3 } : null;
		}

		private void OpenBackupManager(object sender, RoutedEventArgs e)
		{
			BackupManagerViewModel model = new BackupManagerViewModel();
			BackupManager manager = new BackupManager
			{
				DataContext = model,
				Owner = this
			};
			model.Window = manager;
			manager.ShowDialog();
		}

		private void SearchFlyoutTextBox_OnTextChangedTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox tb = (TextBox)sender;
			Model.SearchTerm = tb.Text;
		}

		private void ToggleSortMode(object sender, RoutedEventArgs e)
		{
			StackPanel sp = SortModeButton.Content as StackPanel;
			if (sp == null)
				return;

			PackIconModern icon = (PackIconModern)sp.FindName("SortButtonIcon");

			if (Model.NoteMenuContext.SortMode == "AZ")
			{
				Model.NoteMenuContext.SortMode = "ZA";
				icon.Kind = PackIconModernKind.SortAlphabeticalDescending;
			}
			else if (Model.NoteMenuContext.SortMode == "ZA")
			{
				Model.NoteMenuContext.SortMode = "Index";
				icon.Kind = PackIconModernKind.SortNumeric;
			}
			else if (Model.NoteMenuContext.SortMode == "Index")
			{
				Model.NoteMenuContext.SortMode = "AZ";
				icon.Kind = PackIconModernKind.SortAlphabeticalAscending;
			}
		}

		void OnPowerChange(Object sender, PowerModeChangedEventArgs e)
		{
			switch (e.Mode)
			{
				case PowerModes.Suspend:
					Model.NoteMenuContext.SelectedNote.SaveNote();
					break;
			}
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e)
		{
			NoteView.NoteEditor.SaveNoteText();
			if (Model != null && Model.NoteMenuContext != null)
				Model.NoteMenuContext.SelectedNote.SaveNote();
			SystemEvents.PowerModeChanged -= OnPowerChange;
		}
	}
}

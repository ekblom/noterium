using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Noterium.Core.Helpers;

namespace Noterium.Controls.Behaviors
{
	public class PasteBehavior : Behavior<UIElement>
	{
		public delegate void FilePasted(FileInfo file);
		public event FilePasted OnFilePasted;

		public delegate void ImagePasted(System.Drawing.Image image, string fileName);
		public event ImagePasted OnImagePasted;

		public delegate void HyperLinkPasted(Uri uri);
		public event HyperLinkPasted OnHyperLinkPasted;

		protected override void OnAttached()
		{
			base.OnAttached();

			CommandManager.AddPreviewCanExecuteHandler(AssociatedObject, onPreviewCanExecute);
			CommandManager.AddPreviewExecutedHandler(AssociatedObject, OnPreviewExecuted);
		}

		private void onPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			// In this case, we just say it always can be executed (only for a Paste command), but you can 
			// write some checks here
			if (e.Command == ApplicationCommands.Paste)
			{
				e.CanExecute = true;
				e.Handled = true;
			}
		}

		private void OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			// If it is a paste command..
			if (e.Command == ApplicationCommands.Paste)
			{
				// .. and the clipboard contains an image
				if (Clipboard.ContainsImage() || Clipboard.ContainsFileDropList() || Clipboard.ContainsData(DataFormats.FileDrop))
				{
					if (OnImagePasted != null)
					{
						System.Drawing.Image img = ClipboardHelper.GetImageFromClipboard();
						if (img != null)
						{
							string fileName = null;
							if (Clipboard.ContainsFileDropList())
							{
								var fileNames = Clipboard.GetFileDropList();
								if (fileNames.Count > 0)
								{
									fileName = fileNames[0];
								}
							}
							OnImagePasted(img, fileName);
							return;
						}
					}

					if (OnFilePasted != null)
					{
						if (Clipboard.ContainsData(DataFormats.FileDrop))
						{
							var files = Clipboard.GetFileDropList();
							if (files.Count == 1)
							{
								string fileName = files[0];
								if (File.Exists(fileName))
								{
									FileInfo fi = new FileInfo(fileName);
									OnFilePasted(fi);
								}
							}
						}
					}

					if (OnHyperLinkPasted != null)
					{
						string text = Clipboard.GetText();
						if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
						{
							OnHyperLinkPasted(new Uri(text));
						}
					}

					e.Handled = true;
				}
			}
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			CommandManager.RemovePreviewExecutedHandler(AssociatedObject, OnPreviewExecuted);
			CommandManager.RemovePreviewCanExecuteHandler(AssociatedObject, onPreviewCanExecute);
		}
	}
}
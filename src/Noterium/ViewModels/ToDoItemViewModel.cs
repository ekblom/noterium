using System;
using System.Windows;
using Noterium.Core.DataCarriers;

namespace Noterium.ViewModels
{
	public class ToDoItemViewModel : NoteriumViewModelBase
	{
		private Uri _uri;
		private Visibility _textVisibility;
		private Visibility _linkVisibility;

		public ToDoItem ToDoItem { get; set; }

		public Visibility LinkVisibility
		{
			get { return _linkVisibility; }
			set { _linkVisibility = value; RaisePropertyChanged(); }
		}

		public Visibility TextVisibility
		{
			get { return _textVisibility; }
			set { _textVisibility = value; RaisePropertyChanged(); }
		}

		public Uri Link
		{
			get { return _uri; }
			private set
			{
				_uri = value;

				var linkVisibility = Link == null ? Visibility.Collapsed : Visibility.Visible;
				var textVisibility = Link != null ? Visibility.Collapsed : Visibility.Visible;

				if (LinkVisibility != linkVisibility)
					LinkVisibility = linkVisibility;

				if (TextVisibility != textVisibility)
					TextVisibility = textVisibility;

				RaisePropertyChanged();
			}
		}

		public ToDoItemViewModel(ToDoItem toDoItem)
		{
			ToDoItem = toDoItem;

			InitUri();

			ToDoItem.PropertyChanged -= ToDoItemPropertyChanged;
			ToDoItem.PropertyChanged += ToDoItemPropertyChanged;
		}

		private void InitUri()
		{
			Uri uri;
			Uri.TryCreate(ToDoItem.Text, UriKind.Absolute, out uri);
			if (Link == null || uri != Link)
				Link = uri;
		}

		private void ToDoItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Text")
			{
				InitUri();
			}
		}
	}
}
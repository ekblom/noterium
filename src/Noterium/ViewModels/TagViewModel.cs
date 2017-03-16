using Noterium.Core.DataCarriers;
using Noterium.Code.Interfaces;
using Noterium.ViewModels;
using System;
using System.Collections.Generic;
using Noterium.Code.Messages;
using System.Collections.ObjectModel;

namespace Noterium.ViewModels
{
	public class TagViewModel : NoteriumViewModelBase, IMainMenuItem
    {
		public Tag Tag { get; set; }
		private bool _isSelected;

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; RaisePropertyChanged(); }
		}

		public TagViewModel(Tag tag)
		{
			Tag = tag;
        }

	    public string Name { get => Tag.Name; }

		public MenuItemType MenuItemType => MenuItemType.Tag;

		public ObservableCollection<NoteViewModel> Notes => throw new NotImplementedException();
	}
}
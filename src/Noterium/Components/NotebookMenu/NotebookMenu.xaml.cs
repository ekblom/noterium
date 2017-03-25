using Noterium.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Noterium.Components.NotebookMenu
{
    // https://github.com/punker76/gong-wpf-dragdrop


    /// <summary>
    /// Interaction logic for NotebookMenu.xaml
    /// </summary>
    public partial class NotebookMenu
	{
        private NotebookMenuViewModel _model;

        private readonly List<ListView> _lists = new List<ListView>();

		public NotebookMenu()
		{
			InitializeComponent();

            _lists.Add(Tree);
            _lists.Add(FnissTree);
            _lists.Add(TagsTree);

            Tree.SelectionChanged += ListSelectionChanged;
            FnissTree.SelectionChanged += ListSelectionChanged;
            TagsTree.SelectionChanged += ListSelectionChanged;
		}

        private void ListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            List<ListView> temp = _lists.Where(l => !Equals(l, sender)).ToList();
            temp.ForEach(l => l.SelectedItem = null);
        }

        public AdornerLayer AdornerLayer
		{
			get { return (AdornerLayer)GetValue(AdornerLayerProperty); }
			set { SetValue(AdornerLayerProperty, value); }
		}

		public static readonly DependencyProperty AdornerLayerProperty = DependencyProperty.Register("AdornerLayer", typeof(AdornerLayer), typeof(NotebookMenu), new PropertyMetadata(null));


		public UIElement AdornedElement
		{
			get { return (UIElement)GetValue(AdornedElementProperty); }
			set { SetValue(AdornedElementProperty, value); }
		}

		public static readonly DependencyProperty AdornedElementProperty = DependencyProperty.Register("AdornedElement", typeof(UIElement), typeof(NotebookMenu), new PropertyMetadata(null));

		public NotebookMenuViewModel Model => DataContext as NotebookMenuViewModel;

        //private void ProcessDrop(object sender, ProcessDropEventArgs<IMainMenuItem> e)
		//{
		//	Log.Debug(e.DataItem.ToJson());

		//	/*
		//		if (e.OldIndex > -1)
		//		{
		//			ContextNote.ToDos.Move(e.OldIndex, e.NewIndex);
		//			e.ItemsSource.Move(e.OldIndex, e.NewIndex);
		//		}
		//		else
		//		{
		//			ContextNote.ToDos.Insert(e.NewIndex, e.DataItem.ToDoItem);
		//			e.ItemsSource.Insert(e.NewIndex, e.DataItem);
		//		}

		//		ContextNote.Save();
		//	 */
		//}

		private void NotebookMenu_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_model = DataContext as NotebookMenuViewModel;
			if (_model == null || _model.Notebooks == null)
				return;

		    NotebookViewModel first = _model?.Notebooks.FirstOrDefault();
		    if (first != null)
		    {
		        _model.SelectedItemChangedCommand?.Execute(first);
		    }
		}

		private void NotebookMenu_OnLoaded(object sender, RoutedEventArgs e)
		{
			//CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Tree.ItemsSource);
			//PropertyGroupDescription groupDescription = new PropertyGroupDescription("Parent");
			//view.GroupDescriptions.Add(groupDescription);

			//var listViewDragDropManager = new ListViewDragDropManager<IMainMenuItem>(Tree, true);
			//listViewDragDropManager.ProcessDrop += ProcessDrop;
			//listViewDragDropManager.AdornerLayer = AdornerLayer;
			//listViewDragDropManager.AdornedElement = AdornedElement;
		}
	}
}

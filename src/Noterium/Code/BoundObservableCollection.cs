using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Noterium.Code
{
	public class BoundObservableCollection<T, TSource> : ObservableCollection<T>
	{
		private readonly ObservableCollection<TSource> _source;
		private readonly Func<TSource, T> _converter;
		private readonly Func<T, TSource, bool> _isSameSource;

		public BoundObservableCollection(
			ObservableCollection<TSource> source,
			Func<TSource, T> converter,
			Func<T, TSource, bool> isSameSource)
		{
			_source = source;
			_converter = converter;
			_isSameSource = isSameSource;

			// Copy items
			AddItems(_source);

			// Subscribe to the source's CollectionChanged event
			_source.CollectionChanged += SourceCollectionChanged;
		}

		private void AddItems(IEnumerable<TSource> items)
		{
			foreach (var sourceItem in items)
			{
				Add(_converter(sourceItem));
			}
		}

		void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(e.NewItems.Cast<TSource>());
					break;
				case NotifyCollectionChangedAction.Move:
					// Not sure what to do here...
					
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (var sourceItem in e.OldItems.Cast<TSource>())
					{
						var toRemove = this.First(item => _isSameSource(item, sourceItem));
						Remove(toRemove);
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					for (int i = e.NewStartingIndex; i < e.NewItems.Count; i++)
					{
						this[i] = _converter((TSource)e.NewItems[i]);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					Clear();
					AddItems(_source);
					break;
			}
		}
	}
}
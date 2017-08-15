using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Windows.ViewModels
{
	[PublicAPI]
	public class SelectionViewModel<T> : ViewModel
	{
		private readonly CollectionViewModel<T> mItems;
		private readonly IList<T> mSelectedItems;

		~SelectionViewModel()
		{
			try
			{
				var inc = mItems.Items.CastTo<INotifyCollectionChanged>();
				if (inc == null)
				{
					return;
				}

				inc.CollectionChanged -= OnCollectionChanged;
			}
			catch { /*ignored */ }
		}
		public SelectionViewModel([NotNull] CollectionViewModel<T> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			mItems = items;
			mSelectedItems = new ObservableCollection<T>();

			SubscribeCollectionChanged();
			SubscribeSelectionChanged();
		}

		public T SelectedItem
		{
			get { return mSelectedItems.FirstOrDefault(); }
			set
			{
				if (Equals(value, SelectedItem))
				{
					return;
				}

				SelectedItems.Clear();

				if (value != null)
				{
					SelectedItems.Add(value);
				}

				OnPropertyChanged();
			}
		}
		public IList<T> SelectedItems => mSelectedItems;

		public bool HasSelection => mSelectedItems.Any();
		public bool IsSingleSelection => mSelectedItems.Count == 1;
		public bool IsMultiOrEmptySelection => !IsSingleSelection;

		public T SingleSelectedItem => IsSingleSelection ? SelectedItem : default(T);

		public void SelectFirst()
		{
			SelectedItem = mItems.Items.FirstOrDefault();
		}
		public void Select(params T[] items)
		{
			SelectedItems.Reset(items);
		}

		public event ChangeEventHandler<IEnumerable<T>> SelectionChanged;

		private void SubscribeCollectionChanged()
		{
			var inc = mItems.Items.CastTo<INotifyCollectionChanged>();
			if (inc == null)
			{
				return;
			}

			inc.CollectionChanged += OnCollectionChanged;
		}
		private void SubscribeSelectionChanged()
		{
			var inc = mSelectedItems.CastTo<INotifyCollectionChanged>();
			if (inc == null)
			{
				return;
			}

			inc.CollectionChanged += OnSelectionChanged;
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				var notPresentAnymore = SelectedItems.Where(x => !mItems.Items.Contains(x)).ToArray();
				notPresentAnymore.Foreach(x => SelectedItems.Remove(x));
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
			{
				var list = e.OldItems?.OfType<T>().ToList() ?? new List<T>();
				var counter = 0;

				var newCount = mItems.Items.Count - list.Count;
				var newSelection = new List<T>();

				foreach (var item in list)
				{
					var index = Math.Min(counter + e.OldStartingIndex, newCount);

					SelectedItems.Remove(item);

					if (!HasSelection && index >= 0)
					{
						newSelection.Add(mItems.Items[index]);
					}

					counter++;
				}

				if (newSelection.Any())
				{
					Select(newSelection.ToArray());
				}
				else
				{
					SelectFirst();
				}
			}
		}
		private void OnSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(SelectedItem));
			OnPropertyChanged(nameof(HasSelection));
			OnPropertyChanged(nameof(IsSingleSelection));
			OnPropertyChanged(nameof(IsMultiOrEmptySelection));
			OnPropertyChanged(nameof(SingleSelectedItem));

			var args = new ChangeEventArgs<IEnumerable<T>>(e.OldItems?.OfType<T>().ToArray(), e.NewItems?.OfType<T>().ToArray());
			SelectionChanged?.Invoke(this, args);
		}
	}
}
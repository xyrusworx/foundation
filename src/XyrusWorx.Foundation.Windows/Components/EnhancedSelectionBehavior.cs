using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class EnhancedSelectionBehavior : Behavior<Selector>
	{
		public static readonly DependencyProperty SelectedItemsSourceProperty = DependencyProperty.Register(
			nameof(SelectedItemsSource),
			typeof(IEnumerable),
			typeof(EnhancedSelectionBehavior),
			new PropertyMetadata(
				new ObservableCollection<object>(),
				OnSelectedItemsSourcePropertyChanged));

		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
			nameof(SelectedItem),
			typeof(object),
			typeof(EnhancedSelectionBehavior),
			new PropertyMetadata(
				null,
				OnSelectedItemPropertyChanged));

		public static readonly DependencyProperty PreventSelectionChangeProperty = DependencyProperty.Register(
			nameof(PreventSelectionChange),
			typeof(bool),
			typeof(EnhancedSelectionBehavior),
			new PropertyMetadata(false));

		public static readonly DependencyProperty RestrictToSingleSelectionProperty = DependencyProperty.Register(
			nameof(RestrictToSingleSelection),
			typeof(bool),
			typeof(EnhancedSelectionBehavior),
			new PropertyMetadata(false));

		private Scope mSelectionChangedScope;

		public EnhancedSelectionBehavior()
		{
			mSelectionChangedScope = new Scope();
		}

		public event CancelEventHandler BeforeSelectionChange;
		public event EventHandler SelectionChangePrevented;

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}
		public IEnumerable SelectedItemsSource
		{
			get { return (IEnumerable)GetValue(SelectedItemsSourceProperty); }
			set { SetValue(SelectedItemsSourceProperty, value); }
		}
		public bool PreventSelectionChange
		{
			get { return (bool)GetValue(PreventSelectionChangeProperty); }
			set { SetValue(PreventSelectionChangeProperty, value); }
		}
		public bool RestrictToSingleSelection
		{
			get { return (bool)GetValue(RestrictToSingleSelectionProperty); }
			set { SetValue(RestrictToSingleSelectionProperty, value); }
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.SelectionChanged += OnTargetSelectedItemsChanged;
		}
		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.SelectionChanged -= OnTargetSelectedItemsChanged;
		}

		private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = d.CastTo<EnhancedSelectionBehavior>();
			if (instance == null)
			{
				return;
			}

			if (instance.mSelectionChangedScope.IsInScope)
			{
				return;
			}

			using (instance.mSelectionChangedScope.Enter())
			{
				instance.AssociatedObject.SelectedItem = e.NewValue;
			}
		}
		private static void OnSelectedItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			NotifyCollectionChangedEventHandler handler = ((EnhancedSelectionBehavior)sender).OnSourceSelectedItemsChanged;

			var oldValue = args.OldValue as INotifyCollectionChanged;
			var newValue = args.NewValue as INotifyCollectionChanged;

			if (oldValue != null)
			{
				oldValue.CollectionChanged -= handler;
			}

			if (newValue != null)
			{
				newValue.CollectionChanged += handler;
			}
		}

		private void OnSourceSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (mSelectionChangedScope.IsInScope)
			{
				return;
			}

			var multiSelector = AssociatedObject as MultiSelector;
			if (multiSelector == null)
			{
				using (mSelectionChangedScope.Enter())
				{
					AssociatedObject.SelectedItem = SelectedItemsSource?.OfType<object>().FirstOrDefault();
				}

				return;
			}

			var source = sender as IEnumerable;
			var collection = multiSelector.SelectedItems;

			using (mSelectionChangedScope.Enter())
			{
				if (e.Action == NotifyCollectionChangedAction.Reset)
				{
					collection.Clear();

					foreach (var item in source ?? new List<object>())
					{
						if (!collection.Contains(item))
						{
							collection.Add(item);
						}
					}
				}
				else if (
					e.Action == NotifyCollectionChangedAction.Remove ||
					e.Action == NotifyCollectionChangedAction.Replace ||
					e.Action == NotifyCollectionChangedAction.Add)
				{
					foreach (var item in e.OldItems ?? new List<object>())
					{
						if (collection.Contains(item))
						{
							collection.Remove(item);
						}
					}

					foreach (var item in e.NewItems ?? new List<object>())
					{
						if (!collection.Contains(item))
						{
							collection.Add(item);
						}
					}
				}
			}
		}
		private void OnTargetSelectedItemsChanged(object sender, SelectionChangedEventArgs e)
		{
			if (mSelectionChangedScope.IsInScope || !ReferenceEquals(sender, e.OriginalSource))
			{
				return;
			}

			e.Handled = true;

			using (mSelectionChangedScope.Enter())
			{
				var collection = SelectedItemsSource as IList;
				if (collection == null)
				{
					return;
				}

				var cancel = new CancelEventArgs();

				BeforeSelectionChange?.Invoke(this, cancel);

				if (cancel.Cancel || PreventSelectionChange)
				{
					var multiSelector = AssociatedObject as MultiSelector;
					if (multiSelector != null)
					{
						multiSelector.SelectedItems.Clear();
						foreach (var item in collection)
						{
							multiSelector.SelectedItems.Add(item);
						}
					}
					else
					{
						AssociatedObject.SelectedItem = collection.OfType<object>().FirstOrDefault();
					}

					SelectionChangePrevented?.Invoke(this, new EventArgs());

					return;
				}

				if (RestrictToSingleSelection && e.AddedItems.Count > 1)
				{
					AssociatedObject.SelectedItem = collection.OfType<object>().FirstOrDefault();
					return;
				}

				foreach (var item in e.RemovedItems)
				{
					if (collection.Contains(item))
					{
						collection.Remove(item);
					}
				}

				foreach (var item in e.AddedItems)
				{
					if (!collection.Contains(item))
					{
						collection.Add(item);
					}
				}

				if (collection.Count == 0)
				{
					SetCurrentValue(SelectedItemProperty, null);
				}
				else
				{
					SetCurrentValue(SelectedItemProperty, collection[0]);
				}
			}
		}
	}
}

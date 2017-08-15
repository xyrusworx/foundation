using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Collections;
using XyrusWorx.Threading;
using XyrusWorx.Windows.Input;

namespace XyrusWorx.Windows.ViewModels
{
	[PublicAPI]
	public abstract class SearchableCollectionViewModel<TViewModel> : CollectionViewModel<TViewModel> where TViewModel : class, IHideable
	{
		private ObservableCollection<TViewModel> mInnerVisibleItems;
		private CancellationTokenSource mCancel;
		private SearchExpression mExpression;

		private bool mIsSearching;
		private bool mAutomaticallyUpdateSearchResults;
		private bool mHasVisibleItems;

		private TimeSpan mInputDelay;
		private DelayAction mInputDelayAction;

		protected SearchableCollectionViewModel()
		{
			var collection = new ObservableCollection<TViewModel>();

			collection.CollectionChanged += (o, e) => OnCollectionChanged(e);

			Items = collection;
			VisibleItems = new ReadOnlyObservableCollection<TViewModel>(mInnerVisibleItems = new ObservableCollection<TViewModel>());

			TriggerSearchCommand = new AsyncRelayCommand(UpdateSearchResults, () => !IsSearching);
			CancelSearchCommand = new RelayCommand(CancelSearch, () => IsSearching);
			SearchBoxKeyPressCommand = RelayCommand.WrapKeyPress(TriggerSearchCommand, Key.Enter, Key.Return);

			mExpression = new SearchExpression(null);
		}

		[NotNull]
		public sealed override IList<TViewModel> Items { get; }

		public bool HasVisibleItems
		{
			get { return mHasVisibleItems; }
			private set
			{
				if (value == mHasVisibleItems) return;
				mHasVisibleItems = value;
				OnPropertyChanged();
			}
		}
		public IReadOnlyCollection<TViewModel> VisibleItems { get; }

		public ICommand TriggerSearchCommand { get; }
		public ICommand SearchBoxKeyPressCommand { get; }
		public ICommand CancelSearchCommand { get; }

		public bool AutomaticallyUpdateSearchResults
		{
			get { return mAutomaticallyUpdateSearchResults; }
			set
			{
				if (value == mAutomaticallyUpdateSearchResults) return;
				mAutomaticallyUpdateSearchResults = value;
				OnPropertyChanged();
			}
		}
		public bool IsSearching
		{
			get { return mIsSearching; }
			private set
			{
				if (value == mIsSearching) return;
				mIsSearching = value;
				OnPropertyChanged();
			}
		}
		public string SearchString
		{
			get { return mExpression.Input; }
			set
			{
				if (value == mExpression.Input) return;

				mExpression = new SearchExpression(value);
				OnPropertyChanged();

				if (AutomaticallyUpdateSearchResults)
				{
					if (InputDelay == TimeSpan.Zero)
					{
						BeginUpdateSearchResults();
					}
					else
					{
						mInputDelayAction?.CancelInvocation();
						mInputDelayAction = new DelayAction(BeginUpdateSearchResults, InputDelay);
						mInputDelayAction.QueueInvocation();
					}
				}
			}
		}
		public TimeSpan InputDelay
		{
			get { return mInputDelay; }
			set
			{
				if (value.Equals(mInputDelay)) return;
				mInputDelay = value;
				OnPropertyChanged();
			}
		}

		[NotNull]
		protected SearchExpression Expression => mExpression;
		protected abstract bool IsVisible([NotNull] TViewModel element);

		protected virtual void OnCollectionChangedOverride(NotifyCollectionChangedEventArgs args)
		{
		}

		protected async void BeginUpdateVisibleItems() => await UpdateVisibleItems();
		protected async void BeginUpdateVisibleItems(NotifyCollectionChangedEventArgs args) => await UpdateVisibleItems(args);

		protected async Task UpdateVisibleItems() => await UpdateVisibleItems(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		protected async Task UpdateVisibleItems(NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				var viewModels = (args.NewItems?.OfType<TViewModel>() ?? new TViewModel[0]).AsEnumerable().AsArray();
				foreach (var item in viewModels)
				{
					var index = Items.IndexOf(item);
					if (item.IsVisible)
					{
						Execution.ExecuteOnUiThread(() => mInnerVisibleItems.Insert(index, item), DispatcherPriority.Background);
					}
				}

				return;
			}

			if (args.Action == NotifyCollectionChangedAction.Replace)
			{
				var viewModels = (args.NewItems?.OfType<TViewModel>() ?? new TViewModel[0]).AsEnumerable().AsArray();
				foreach (var item in viewModels)
				{
					var index = Items.IndexOf(item);
					if (item.IsVisible)
					{
						var innerIndex = mInnerVisibleItems.IndexOf(item);
						Execution.ExecuteOnUiThread(() =>
						{
							if (innerIndex >= 0)
							{
								mInnerVisibleItems[innerIndex] = item;
							}
							else
							{
								mInnerVisibleItems.Insert(index, item);
							}
						}, DispatcherPriority.Background);
					}
				}

				return;
			}

			await Task.Run(() =>
			{
				Execution.ExecuteOnUiThread(() => mInnerVisibleItems.Clear());
				HasVisibleItems = false;

				foreach (var item in Items.ToArray())
				{
					if (mCancel?.IsCancellationRequested ?? false)
					{
						break;
					}

					if (item.IsVisible)
					{
						Execution.ExecuteOnUiThread(() => mInnerVisibleItems.Add(item), DispatcherPriority.Background);
						HasVisibleItems = true;
					}
				}
			});
		}

		protected async void BeginUpdateSearchResults()
		{
			await UpdateSearchResults();
		}
		protected async Task UpdateSearchResults()
		{
			using (new Scope(OnBeginSearch, OnEndSearch).Enter())
			{
				var token = mCancel.Token;

				// ReSharper disable once MethodSupportsCancellation
				await Task.Run(() =>
				{
					foreach (var item in Items)
					{
						if (token.IsCancellationRequested)
						{
							break;
						}

						item.IsVisible = IsVisible(item);
					}
				});
			}

			await UpdateVisibleItems();
		}
		protected void CancelSearch()
		{
			mCancel?.Cancel();
		}

		private void OnBeginSearch()
		{
			mInputDelayAction = null;
			mCancel = new CancellationTokenSource();
			IsSearching = true;
		}
		private void OnEndSearch()
		{
			mCancel = null;
			IsSearching = false;
			mInputDelayAction = null;
		}

		private async void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			await UpdateVisibleItems(args);
			OnCollectionChangedOverride(args);
		}
	}
}
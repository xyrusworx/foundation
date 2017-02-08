using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class DelayQueue<T> : Resource
	{
		private readonly ConcurrentQueue<T> mItemQueue;
		private readonly Timer mTimer;
		private readonly object mLock;

		private bool mIsDelayEnabled;
		private TimeSpan mInterval;

		public DelayQueue(TimeSpan interval)
		{
			mItemQueue = new ConcurrentQueue<T>();
			mTimer = new Timer(OnTimerTick, null, TimeSpan.Zero, interval);
			mLock = new object();
			mInterval = interval;
		}

		public TimeSpan FlushInterval
		{
			get { return mInterval; }
			set
			{
				mInterval = value;
				mTimer.Change(TimeSpan.Zero, mInterval);
			}
		}
		public Action<IList<T>> Callback { get; set; }

		public void Enqueue(T item)
		{
			if (IsDelayEnabled)
			{
				lock (mLock)
				{
					mItemQueue.Enqueue(item);
				}
			}
			else
			{
				Callback?.Invoke(new List<T> { item });
			}
		}
		public void Enqueue(IEnumerable<T> items)
		{
			if (IsDelayEnabled)
			{
				lock (mLock)
				{
					items?.Foreach(x => mItemQueue.Enqueue(x));
				}
			}
			else
			{
				Callback?.Invoke(items?.ToList() ?? new List<T>());
			}
		}

		public bool IsDelayEnabled
		{
			get { return mIsDelayEnabled; }
			set
			{
				if (!value)
				{
					Flush();
				}
				mIsDelayEnabled = value;
			}
		}

		public void Flush()
		{
			if (mLock == null)
			{
				return;
			}

			var itemsInQueue = new List<T>();
			lock (mLock)
			{
				while (!mItemQueue.IsEmpty)
				{
					T item;
					mItemQueue.TryDequeue(out item);
					itemsInQueue.Add(item);
				}
			}

			Callback?.Invoke(itemsInQueue);
		}

		protected sealed override void DisposeOverride()
		{
			Flush();
			mTimer.Dispose();
		}
		protected sealed override void FinalizeOverride()
		{
		}

		private void OnTimerTick(object state)
		{
			if (!mIsDelayEnabled)
			{
				return;
			}

			Flush();
		}
	}
}
using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class Clock : Resource
	{
		private Timer mTimer;
		private bool mIsEnabled;
		private TimeSpan mInterval;
		private Action mTickAction;

		public Clock()
		{
			mInterval = TimeSpan.FromSeconds(1);
		}

		public bool IsEnabled
		{
			get { return mIsEnabled; }
			set
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(nameof(IterationCounter));
				}

				mIsEnabled = value;

				if (!value)
				{
					mTimer?.Dispose();
					mTimer = null;
				}
				else
				{
					mTimer = new Timer(OnTimerTick, null, mInterval, mInterval);
				}
			}
		}
		public TimeSpan TickInterval
		{
			get { return mInterval; }
			set
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(nameof(IterationCounter));
				}

				if (value.TotalMilliseconds < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				mInterval = value;
				mTimer?.Change(TimeSpan.Zero, mInterval);
			}
		}
		public Action TickAction
		{
			get { return mTickAction; }
			set
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(nameof(IterationCounter));
				}

				mTickAction = value;
			}
		}

		protected override void FinalizeOverride()
		{
			mTickAction = null;
		}

		private void OnTimerTick(object state)
		{
			TickAction?.Invoke();
		}
	}
}
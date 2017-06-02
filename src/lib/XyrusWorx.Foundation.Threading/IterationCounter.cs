using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class IterationCounter : Resource
	{
		private readonly object mLock = new object();
		private readonly List<double> mRecentSamples = new List<double>();

		private readonly Stopwatch mOverwatch;
		private readonly Stopwatch mStopwatch;
		private Clock mClock;

		private TimeSpan mElapsed;
		private long mIterations;

		public IterationCounter()
		{
			mStopwatch = new Stopwatch();
			mOverwatch = new Stopwatch();

			mClock = new Clock();
			mClock.TickAction = Update;
		}

		public double IterationsPerSecond { get; private set; }
		public double MaxIterationsPerSecond { get; private set; }
		public double MinIterationsPerSecond { get; private set; }
		public double AverageIterationsPerSecond { get; private set; }

		public long TotalIterationsDone { get; private set; }

		public Duration? GetElapsedDuration()
		{
			return mElapsed;
		}
		public Duration? GetRemainingDuration(long iterationsToGo)
		{
			if (AverageIterationsPerSecond == 0)
			{
				return null;
			}

			return TimeSpan.FromSeconds(iterationsToGo / AverageIterationsPerSecond);
		}

		public IterationCounterSnapshot GetSnapshot()
		{
			lock (mLock)
			{
				return new IterationCounterSnapshot(this);
			}
		}

		public bool IsAutomaticUpdateEnabled
		{
			get { return mClock.IsEnabled; }
			set { mClock.IsEnabled = value; }
		}
		public TimeSpan AutomaticUpdateInterval
		{
			get { return mClock.TickInterval; }
			set { mClock.TickInterval = value; }
		}

		public void Increment(int count = 1)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(IterationCounter));
			}

			lock (mLock)
			{
				mIterations += Math.Max(0, count);
				TotalIterationsDone += Math.Max(0, count);
			}
		}
		public void Reset()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(IterationCounter));
			}

			lock (mLock)
			{
				mStopwatch.Reset();

				mIterations = 0;
				MaxIterationsPerSecond = 0;
				MinIterationsPerSecond = 0;
				IterationsPerSecond = 0;
				AverageIterationsPerSecond = 0;
				TotalIterationsDone = 0;
				mElapsed = TimeSpan.Zero;

				mRecentSamples.Clear();

				mOverwatch.Stop();
				mOverwatch.Reset();
				mOverwatch.Start();
			}
		}
		public void Update()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(IterationCounter));
			}

			lock (mLock)
			{
				var delta = mStopwatch.Elapsed.TotalSeconds;

				if (mIterations <= 0)
				{
					return;
				}

				mStopwatch.Stop();

				if (delta > 0)
				{
					IterationsPerSecond = mIterations / delta;
					MaxIterationsPerSecond = Math.Max(MaxIterationsPerSecond, IterationsPerSecond);
					MinIterationsPerSecond = mRecentSamples.Count > 0 ? Math.Min(MinIterationsPerSecond, IterationsPerSecond) : IterationsPerSecond;
					AverageIterationsPerSecond = mRecentSamples.Count > 0 ? (mRecentSamples.Sum() + IterationsPerSecond) / (mRecentSamples.Count + 1) : IterationsPerSecond;

					mIterations = 0;

					var overflow = mRecentSamples.Count - 49;
					if (overflow > 0)
					{
						for (var i = 0; i < overflow; i++)
						{
							mRecentSamples.RemoveAt(0);
						}
					}

					mRecentSamples.Add(IterationsPerSecond);
				}

				try
				{
					if (delta > 0)
					{
						Updated?.Invoke(this, new EventArgs());
					}
				}
				finally
				{
					mStopwatch.Reset();
					mStopwatch.Start();

					mElapsed = mOverwatch.Elapsed;
				}
			}
		}

		public event EventHandler Updated;

		protected sealed override void DisposeOverride()
		{
			mOverwatch.Stop();
			mStopwatch.Stop();

			mClock?.Dispose();
			mClock = null;
		}
		protected sealed override void FinalizeOverride()
		{
			Updated = null;
		}
	}
}
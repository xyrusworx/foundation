using System;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class IterationCounterSnapshot
	{
		private readonly Duration? mElapsed;

		internal IterationCounterSnapshot([NotNull] IterationCounter counter)
		{
			if (counter == null)
			{
				throw new ArgumentNullException(nameof(counter));
			}

			IterationsPerSecond = counter.IterationsPerSecond;
			MaxIterationsPerSecond = counter.MaxIterationsPerSecond;
			MinIterationsPerSecond = counter.MinIterationsPerSecond;
			AverageIterationsPerSecond = counter.AverageIterationsPerSecond;

			TotalIterationsDone = counter.TotalIterationsDone;

			mElapsed = counter.GetElapsedDuration();
		}

		public double IterationsPerSecond { get; }
		public double MaxIterationsPerSecond { get; }
		public double MinIterationsPerSecond { get; }
		public double AverageIterationsPerSecond { get; }

		public long TotalIterationsDone { get; }

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
	}
}
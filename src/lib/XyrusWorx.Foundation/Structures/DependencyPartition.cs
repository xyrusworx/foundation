using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public sealed class DependencyPartition<T> : IEnumerable<T>
	{
		private readonly int mDependencyLevel;
		private readonly IEnumerable<T> mPartitionContent;

		internal DependencyPartition(int dependencyLevel, [NotNull] IEnumerable<T> partitionContent)
		{
			if (dependencyLevel < 0) throw new ArgumentOutOfRangeException(nameof(dependencyLevel));
			if (partitionContent == null) throw new ArgumentNullException(nameof(partitionContent));

			mDependencyLevel = dependencyLevel;
			mPartitionContent = partitionContent;
		}

		public int Level => mDependencyLevel;
		public IEnumerator<T> GetEnumerator() => mPartitionContent.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public abstract class DependencyGraph<T>
	{
		private readonly DirectedGraph<T> mInnerGraph;

		protected DependencyGraph()
		{
			mInnerGraph = new DirectedGraph<T>(false);
		}

		public void Register([NotNull] T element)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));

			VerifyNode(element);

			mInnerGraph.AddFreeNode(element);
		}
		public void SetupDependency([NotNull] T from, [NotNull] T to)
		{
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			if (AreEqual(from, to))
			{
				throw new ArgumentException($"A \"{typeof(T).FullName}\" can't be depdendent on itself.", nameof(to));
			}

			VerifyNode(from);
			VerifyNode(to);

			mInnerGraph.AddEdge(from, to);
		}

		[NotNull]
		public IEnumerable<T> GetKnownElements()
		{
			return mInnerGraph.GetNodes().Select(x => x.Data);
		}

		[NotNull] public IEnumerable<DependencyPartition<T>> GetPartitionsByDependencyDepth()
		{
			var nodes = mInnerGraph.GetNodes().Select(x => x.Data);

			return GetPartitionsByDependencyDepth(nodes);
		}
		[NotNull] public IEnumerable<DependencyPartition<T>> GetPartitionsByDependencyDepth([NotNull] IEnumerable<T> nodes)
		{
			if (nodes == null)
			{
				throw new ArgumentNullException(nameof(nodes));
			}

			var remaining = nodes.ToList();
			var treated = new HashSet<T>();

			var maxIterations = remaining.Count;
			var counter = 0;
			var outputCounter = 0;

			while (remaining.Count > 0)
			{
				var currentPartition = new List<T>(
					from element in remaining
					let elementDependencies = mInnerGraph.GetEdgesFrom(element).Select(x => x.To.Data).ToList()
					where !elementDependencies.Any() || elementDependencies.All(x => treated.Contains(x))
					select element);

				counter++;

				if (currentPartition.Count > 0)
				{
					outputCounter++;

					yield return new DependencyPartition<T>(outputCounter, currentPartition);
					foreach (var item in currentPartition)
					{
						remaining.Remove(item);
						treated.Add(item);
					}
				}

				if (counter <= maxIterations)
				{
					continue;
				}

				var circularExceptionMessage = new StringBuilder();
				foreach (var element in remaining)
				{
					circularExceptionMessage.AppendLine($"- {element} ({element?.GetType().FullName ?? typeof(object).FullName}");
				}

				throw new InvalidOperationException(
					$"Failed to partition the dependency graph because a circular reference involving the following elements has been detected: {circularExceptionMessage}");
			}
		}

		[NotNull]
		protected internal DirectedGraph<T> InnerGraph => mInnerGraph;
		protected internal abstract bool AreEqual([NotNull] T from, [NotNull] T to);

		protected virtual void VerifyNode([NotNull] T node) { }
	}

	[PublicAPI]
	public abstract class DependencyGraph<T, TNode> : DependencyGraph<T> where TNode : DependencyGraphNode<T>
	{
		[NotNull]
		public TNode Element([NotNull] T element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			return CreateNode(element);
		}

		[NotNull]
		protected abstract TNode CreateNode(T element);
	}
}
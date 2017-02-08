using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class DependencyGraphNode<T>
	{
		private readonly DependencyGraph<T> mGraph;
		private readonly T mElement;

		protected internal DependencyGraphNode([NotNull] DependencyGraph<T> graph, [NotNull] T element)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (element == null) throw new ArgumentNullException(nameof(element));

			mGraph = graph;
			mElement = element;
		}

		public bool IsDependentOn([NotNull] T on)
		{
			if (on == null) throw new ArgumentNullException(nameof(on));
			if (mGraph.AreEqual(mElement, on)) return false;

			return mGraph.InnerGraph.HasEdge(mElement, on);
		}

		[NotNull]
		public IEnumerable<T> GetDependencies(int maxDepth = 1)
		{
			if (maxDepth < 1) throw new ArgumentOutOfRangeException(nameof(maxDepth));

			return SearchDependencies(mElement, 1, maxDepth);
		}

		[NotNull] protected DependencyGraph<T> Graph => mGraph;
		[NotNull] protected T Element => mElement;

		public bool Remove()
		{
			return mGraph.InnerGraph.RemoveNode(mElement);
		}

		private IEnumerable<T> SearchDependencies(T currentNode, int currentDepth, int maxDepth)
		{
			if (currentDepth > maxDepth)
			{
				yield break;
			}

			foreach (var edge in mGraph.InnerGraph.GetEdgesFrom(currentNode))
			{
				yield return edge.To.Data;

				foreach (var child in SearchDependencies(edge.To.Data, currentDepth + 1, maxDepth))
				{
					yield return child;
				}
			}
		}
	}
}
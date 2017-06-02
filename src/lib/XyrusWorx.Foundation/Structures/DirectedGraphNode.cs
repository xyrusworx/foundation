using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class DirectedGraphNode<T>
	{
		private readonly DirectedGraph<T> mGraph;
		private readonly Lazy<IEnumerable<DirectedGraphNode<T>>> mNext;

		internal DirectedGraphNode([NotNull] DirectedGraph<T> graph, [NotNull] T node)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (node == null) throw new ArgumentNullException(nameof(node));

			mGraph = graph;
			Data = node;

			mNext = new Lazy<IEnumerable<DirectedGraphNode<T>>>(GetNextNodes);
		}

		[NotNull]
		public T Data { get; }

		[NotNull]
		public IEnumerable<DirectedGraphNode<T>> Next => mNext.Value;

		private IEnumerable<DirectedGraphNode<T>> GetNextNodes()
		{
			foreach (var edge in mGraph.GetEdgesFrom(Data))
			{
				yield return edge.To;
			}
		}
	}
}
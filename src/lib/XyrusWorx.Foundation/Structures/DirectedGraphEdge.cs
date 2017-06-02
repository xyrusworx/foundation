using System;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class DirectedGraphEdge<T>
	{
		internal DirectedGraphEdge([NotNull] DirectedGraph<T> graph, [NotNull] T from, [NotNull] T to)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			From = new DirectedGraphNode<T>(graph, from);
			To = new DirectedGraphNode<T>(graph, to);
		}
		internal DirectedGraphEdge([NotNull] DirectedGraph<T> graph, [NotNull] DirectedGraphNode<T> from, [NotNull] T to)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			From = from;
			To = new DirectedGraphNode<T>(graph, to);
		}
		internal DirectedGraphEdge([NotNull] DirectedGraph<T> graph, [NotNull] T from, [NotNull] DirectedGraphNode<T> to)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			From = new DirectedGraphNode<T>(graph, from);
			To = to;
		}
		internal DirectedGraphEdge([NotNull] DirectedGraph<T> graph, [NotNull] DirectedGraphNode<T> from, [NotNull] DirectedGraphNode<T> to)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			From = from;
			To = to;
		}

		[NotNull]
		public DirectedGraphNode<T> From { get; }

		[NotNull]
		public DirectedGraphNode<T> To { get; }
	}
}
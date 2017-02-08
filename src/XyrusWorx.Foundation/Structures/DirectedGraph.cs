using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class DirectedGraph<T>
	{
		private readonly Dictionary<T, HashSet<T>> mEdgesFromView;
		private readonly Dictionary<T, HashSet<T>> mEdgesToView;

		private readonly bool mAllowSelfReference;

		public DirectedGraph() : this(true)
		{
		}
		public DirectedGraph(bool allowSelfReference)
		{
			mEdgesFromView = new Dictionary<T, HashSet<T>>();
			mEdgesToView = new Dictionary<T, HashSet<T>>();

			mAllowSelfReference = allowSelfReference;
		}

		public bool AllowSelfReference => mAllowSelfReference;

		[NotNull] public IEnumerable<DirectedGraphNode<T>> GetNodes()
		{
			return mEdgesFromView.Keys.Union(mEdgesToView.Keys).Select(x => new DirectedGraphNode<T>(this, x));
		}
		[NotNull] public IEnumerable<DirectedGraphEdge<T>> GetEdges()
		{
			foreach (var sourceNode in mEdgesFromView.Keys)
			{
				foreach (var targetNode in mEdgesFromView[sourceNode])
				{
					yield return new DirectedGraphEdge<T>(this, sourceNode, targetNode);
				}
			}
		}

		[NotNull] public IEnumerable<DirectedGraphEdge<T>> GetEdgesFrom([NotNull] T node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if (!mEdgesFromView.ContainsKey(node))
			{
				yield break;
			}

			foreach (var targetNode in mEdgesFromView[node])
			{
				yield return new DirectedGraphEdge<T>(this, node, targetNode);
			}
		}
		[NotNull] public IEnumerable<DirectedGraphEdge<T>> GetEdgesFrom([NotNull] DirectedGraphNode<T> node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if (!mEdgesFromView.ContainsKey(node.Data))
			{
				yield break;
			}

			foreach (var targetNode in mEdgesFromView[node.Data])
			{
				yield return new DirectedGraphEdge<T>(this, node, targetNode);
			}
		}

		[NotNull] public IEnumerable<DirectedGraphEdge<T>> GetEdgesTo([NotNull] T node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if (!mEdgesToView.ContainsKey(node))
			{
				yield break;
			}

			foreach (var sourceNode in mEdgesToView[node])
			{
				yield return new DirectedGraphEdge<T>(this, sourceNode, node);
			}
		}
		[NotNull] public IEnumerable<DirectedGraphEdge<T>> GetEdgesTo([NotNull] DirectedGraphNode<T> node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if (!mEdgesToView.ContainsKey(node.Data))
			{
				yield break;
			}

			foreach (var sourceNode in mEdgesToView[node.Data])
			{
				yield return new DirectedGraphEdge<T>(this, sourceNode, node);
			}
		}

		public bool HasNode([NotNull] T node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));

			if (mEdgesFromView.ContainsKey(node))
			{
				return true;
			}

			if (mEdgesToView.ContainsKey(node))
			{
				return true;
			}

			return false;
		}
		public bool HasEdge([NotNull] T from, [NotNull] T to)
		{
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			if (!mEdgesFromView.ContainsKey(from))
			{
				return false;
			}

			if (!mEdgesFromView[from].Contains(to))
			{
				return false;
			}

			return true;
		}

		public void AddEdge([NotNull] T from, [NotNull] T to)
		{
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			var fromList = mEdgesFromView.GetValueByKeyOrDefault(from);
			var toList = mEdgesToView.GetValueByKeyOrDefault(to);

			if (Equals(from, to) && !AllowSelfReference)
			{
				throw new InvalidOperationException("Self reference is not allowed in this graph instance.");
			}

			if (fromList == null)
			{
				mEdgesFromView.Add(from, fromList = new HashSet<T>());
			}

			if (toList == null)
			{
				mEdgesToView.Add(to, toList = new HashSet<T>());
			}

			fromList.Add(to);
			toList.Add(from);
		}
		public void AddFreeNode([NotNull] T node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			mEdgesFromView.AddIfMissing(node, new HashSet<T>());
		}

		public bool RemoveEdge([NotNull] T from, [NotNull] T to)
		{
			if (from == null) throw new ArgumentNullException(nameof(from));
			if (to == null) throw new ArgumentNullException(nameof(to));

			var fromList = mEdgesFromView.GetValueByKeyOrDefault(from);
			var toList = mEdgesToView.GetValueByKeyOrDefault(to);

			var fromResult = fromList?.Remove(to) ?? false;
			var toResult = toList?.Remove(from) ?? false;

			return fromResult || toResult;
		}
		public bool RemoveNode([NotNull] T node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			var fromResult = mEdgesFromView.Remove(node);
			var toResult = mEdgesToView.Remove(node);

			foreach (var sourceNode in mEdgesFromView.Keys)
			{
				fromResult |= mEdgesFromView[sourceNode].Remove(node);
			}

			foreach (var targetNode in mEdgesToView.Keys)
			{
				toResult |= mEdgesToView[targetNode].Remove(node);
			}

			return fromResult || toResult;
		}

		public void Clear()
		{
			mEdgesFromView.Clear();
			mEdgesToView.Clear();
		}
	}
}

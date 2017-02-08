using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class Hierarchy<TKey, TValue>
	{
		private readonly IDictionary<TKey, Hierarchy<TKey, TValue>> mChildren;

		public Hierarchy()
		{
			mChildren = new Dictionary<TKey, Hierarchy<TKey, TValue>>();
			Children = new HierarchyNodeChildren<TKey, TValue>(mChildren);
		}

		[NotNull]
		public HierarchyNodeChildren<TKey, TValue> Children { get; }
		public TValue Value { get; set; }
	}
}
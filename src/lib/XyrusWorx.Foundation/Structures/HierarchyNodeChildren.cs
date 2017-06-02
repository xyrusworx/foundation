using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class HierarchyNodeChildren<TKey, TValue>
	{
		private IDictionary<TKey, Hierarchy<TKey, TValue>> mHierarchy;

		internal HierarchyNodeChildren([NotNull] IDictionary<TKey, Hierarchy<TKey, TValue>> dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException(nameof(dictionary));
			}

			mHierarchy = dictionary;
		}

		public int Count => mHierarchy.Keys.Count;

		[NotNull]
		public IEnumerable<TKey> Keys => mHierarchy.Keys.ToArray();

		public bool Contains(TKey key)
		{
			return mHierarchy.ContainsKey(key);
		}

		[CanBeNull]
		public Hierarchy<TKey, TValue> this[TKey key]
		{
			get { return mHierarchy.GetValueByKeyOrDefault(key); }
			set
			{
				if (value == null)
				{
					mHierarchy.Remove(key);
					return;
				}

				mHierarchy.AddOrUpdate(key, value);
			}
		}
	}
}
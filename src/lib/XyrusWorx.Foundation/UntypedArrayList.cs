using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;

namespace XyrusWorx
{
	class UntypedArrayList : IList
	{
		private readonly List<object> mElements = new List<object>();
		private readonly object mLock = new object();

		IEnumerator IEnumerable.GetEnumerator() => mElements.GetEnumerator();

		public object this[int index]
		{
			get { return mElements[index]; }
			set { mElements[index] = value; }
		}

		public bool IsSynchronized => true;
		public bool IsReadOnly => false;
		public bool IsFixedSize => false;
		public object SyncRoot => mLock;
		public int Count => mElements.Count;

		public int Add(object value)
		{
			mElements.Add(value);
			return mElements.Count;
		}
		public bool Contains(object value)
		{
			return mElements.Contains(value);
		}
		public void Clear()
		{
			mElements.Clear();
		}
		public void CopyTo(Array array, int index)
		{
			mElements.CastTo<IList>()?.CopyTo(array, index);
		}
		public int IndexOf(object value)
		{
			return mElements.IndexOf(value);
		}
		public void Insert(int index, object value)
		{
			mElements.Insert(index, value);
		}
		public void Remove(object value)
		{
			mElements.Remove(value);
		}
		public void RemoveAt(int index)
		{
			mElements.RemoveAt(index);
		}

		[NotNull]
		public Array ToArray([NotNull] Type elementType)
		{
			if (elementType == null)
			{
				throw new ArgumentNullException(nameof(elementType));
			}

			var source = (IList) mElements;
			var target = Array.CreateInstance(elementType, mElements.Count);

			source.CopyTo(target, 0);

			return target;
		}
	}
}
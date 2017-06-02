using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.Collections
{
	[PublicAPI]
	public class LinkableList<T> : IList<T>
	{
		private readonly object mLockHandle;
		private readonly List<T> mList;

		public LinkableList(object lockHandle = null)
		{
			mLockHandle = lockHandle;
			mList = new List<T>();
		}

		public IEnumerator<T> GetEnumerator() => mList.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add([NotNull] T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (!mList.Contains(item))
			{
				HandleInsert(item);
			}

			if (mLockHandle != null)
			{
				lock (mLockHandle) mList.Add(item);
			}
			else
			{
				mList.Add(item);
			}
		}
		public bool Remove([NotNull] T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (!mList.Contains(item))
			{
				return false;
			}

			bool result;

			if (mLockHandle != null)
			{
				lock (mLockHandle)
				{
					result = mList.Remove(item);
				}
			}
			else
			{
				result = mList.Remove(item);
			}

			if (result)
			{
				HandleRemove(item);
			}

			return true;
		}
		public void Clear()
		{
			var elements = mList.ToArray();

			if (mLockHandle != null)
			{
				lock (mLockHandle) mList.Clear();
			}
			else
			{
				mList.Clear();
			}

			foreach(var item in elements)
			{
				HandleRemove(item);
			}
		}

		public int Count => mList.Count;
		public int IndexOf(T item) => mList.IndexOf(item);

		public bool IsReadOnly => false;
		public bool Contains(T item) => mList.Contains(item);

		public void Insert(int index, T item)
		{
			if (!mList.Contains(item))
			{
				HandleInsert(item);
			}

			if (mLockHandle != null)
			{
				lock (mLockHandle) mList.Insert(index, item);
			}
			else
			{
				mList.Insert(index, item);
			}
		}
		public void CopyTo(T[] array, int arrayIndex) => mList.CopyTo(array, arrayIndex);
		public void RemoveAt(int index)
		{
			var item = mList[index];

			if (mLockHandle != null)
			{
				lock (mLockHandle) mList.RemoveAt(index);
			}
			else
			{
				mList.RemoveAt(index); 
			}

			HandleRemove(item);
		}

		public T this[int index]
		{
			get { return mList[index]; }
			set
			{
				if (ReferenceEquals(mList[index], value))
				{
					return;
				}

				HandleRemove(mList[index]);
				HandleInsert(value);

				if (mLockHandle != null)
				{
					lock (mLockHandle) mList[index] = value;
				}
				else
				{
					mList[index] = value;
				}
			}
		}

		public Action<T> InsertAction { get; set; }
		public Action<T> RemoveAction { get; set; }

		protected virtual void HandleInsertOverride(T item) { }
		protected virtual void HandleRemoveOverride(T item) { }

		private void HandleInsert(T item)
		{
			HandleInsertOverride(item);
			InsertAction?.Invoke(item);
		}
		private void HandleRemove(T item)
		{
			HandleRemoveOverride(item);
			RemoveAction?.Invoke(item);
		}
	}
}
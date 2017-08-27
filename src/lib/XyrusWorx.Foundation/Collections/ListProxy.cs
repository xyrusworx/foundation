using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.Collections 
{
	[PublicAPI]
	public class ListProxy<TSource> : IList<TSource>
	{
		private readonly IList<TSource> mSource;

		public ListProxy() : this(new List<TSource>()) { }
		public ListProxy([NotNull] IList<TSource> source)
		{
			mSource = source ?? throw new ArgumentNullException(nameof(source));
		}

		public virtual IEnumerator<TSource> GetEnumerator() => mSource.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => mSource.GetEnumerator();

		public virtual void Add(TSource item) => mSource.Add(item);
		public virtual void Insert(int index, TSource item) => mSource.Insert(index, item);
		public virtual void Clear() => mSource.Clear();
		public virtual bool Contains(TSource item) => mSource.Contains(item);
		public virtual bool Remove(TSource item) => mSource.Remove(item);
		public virtual void RemoveAt(int index) => mSource.RemoveAt(index);
		public virtual void CopyTo(TSource[] array, int arrayIndex) => Array.Copy(mSource.ToArray(), array, arrayIndex);
		public virtual int IndexOf(TSource item) => mSource.IndexOf(item);

		public virtual int Count
		{
			get => mSource.Count;
		}
		public virtual bool IsReadOnly
		{
			get => mSource.IsReadOnly;
		}
		public virtual TSource this[int index]
		{
			get => mSource[index];
			set => mSource[index] = value;
		}
	}
}
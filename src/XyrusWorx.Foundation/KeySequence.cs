using System;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public struct KeySequence<T> where T: struct
	{
		private readonly Key<T>[] mSegments;

		public KeySequence([NotNull] T[] segments)
		{
			if (segments == null)
			{
				throw new ArgumentNullException(nameof(segments));
			}

			if (segments.Length == 0)
			{
				throw new ArgumentException("Segment array can't be empty.", nameof(segments));
			}

			mSegments = segments.Select(x => new Key<T>(x)).ToArray();
		}
		public KeySequence(T rootSegment, params T[] pathSegments) : this(new[] { rootSegment }.Concat(pathSegments ?? new T[0]).ToArray())
		{
		}

		public KeySequence([NotNull] Key<T>[] segments)
		{
			if (segments == null)
			{
				throw new ArgumentNullException(nameof(segments));
			}

			if (segments.Length == 0)
			{
				throw new ArgumentException("Segment array can't be empty.", nameof(segments));
			}

			mSegments = segments.ToArray();
		}
		public KeySequence(Key<T> rootSegment, params Key<T>[] pathSegments) : this(new[] { rootSegment }.Concat(pathSegments ?? new Key<T>[0]).ToArray())
		{
		}

		public T[] RawData => mSegments.Select(x => x.RawData).ToArray();
		public bool IsEmpty => mSegments == null || mSegments.Length == 0;

		public Key<T>[] Segments => mSegments ?? new Key<T>[0];

		public KeySequence<T> Concat(Key<T> key) => new KeySequence<T>((mSegments ?? new Key<T>[0]).Concat(new [] {key}).ToArray());
		public KeySequence<T> Concat(KeySequence<T> keySequence) => new KeySequence<T>((mSegments ?? new Key<T>[0]).Concat(keySequence.mSegments ?? new Key<T>[0]).ToArray());

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;

				foreach (var key in mSegments)
				{
					hash = hash * 31 + key.GetHashCode();
				}

				return hash;
			}
		}
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return 
				obj is KeySequence<T> && 
				Equals((KeySequence<T>)obj);
		}
		public bool Equals(KeySequence<T> other)
		{
			if (! IsEmpty &&   other.IsEmpty) return false;
			if (  IsEmpty && ! other.IsEmpty) return false;
			if (  IsEmpty &&   other.IsEmpty) return true;

			if (mSegments.Length != other.mSegments.Length)
			{
				return false;
			}

			for (var i = 0; i < mSegments.Length; i++)
			{
				if (!Equals(mSegments[i], other.mSegments[i]))
				{
					return false;
				}
			}

			return true;
		}

		public override string ToString() => ToString("/");
		public string ToString(string separator)
		{
			return string.Join(separator ?? string.Empty, (mSegments ?? new Key<T>[0]).Select(x => x.ToString()));
		}
	}
}
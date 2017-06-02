using System;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public struct StringKeySequence
	{
		private readonly StringKey[] mSegments;

		public StringKeySequence([NotNull] string[] segments)
		{
			if (segments == null)
			{
				throw new ArgumentNullException(nameof(segments));
			}

			mSegments = segments.Where(x => !string.IsNullOrEmpty(x)).Select(x => new StringKey(x)).ToArray();
		}
		public StringKeySequence(string rootSegment, params string[] pathSegments) : this(new[] { rootSegment }.Concat(pathSegments ?? new string[0]).Where(x => !string.IsNullOrEmpty(x)).ToArray())
		{
		}

		public StringKeySequence([NotNull] StringKey[] segments)
		{
			if (segments == null)
			{
				throw new ArgumentNullException(nameof(segments));
			}

			mSegments = segments.Where(x => !x.IsEmpty).ToArray();
		}
		public StringKeySequence(StringKey rootSegment, params StringKey[] pathSegments) : this(new[] { rootSegment }.Concat(pathSegments ?? new StringKey[0]).Where(x => !string.IsNullOrEmpty(x)).ToArray())
		{
		}

		public string[] RawData => mSegments.Select(x => x.RawData).ToArray();
		public StringKeySequence Normalize() => IsEmpty
			? new StringKeySequence()
			: new StringKeySequence(mSegments.Select(x => x.Normalize()).ToArray());
		public bool IsEmpty => mSegments == null || mSegments.Length == 0;

		public StringKey[] Segments => mSegments ?? new StringKey[0];

		public StringKeySequence Concat(StringKey key) => new StringKeySequence((mSegments ?? new StringKey[0]).Concat(new[] { key }.Where(x => !x.IsEmpty)).ToArray());
		public StringKeySequence Concat(StringKeySequence keySequence) => new StringKeySequence((mSegments ?? new StringKey[0]).Concat(keySequence.mSegments ?? new StringKey[0]).ToArray());

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;

				foreach (var key in mSegments ?? new StringKey[0])
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
				obj is StringKeySequence &&
				Equals((StringKeySequence)obj);
		}
		public bool Equals(StringKeySequence other)
		{
			if (!IsEmpty && other.IsEmpty) return false;
			if (IsEmpty && !other.IsEmpty) return false;
			if (IsEmpty && other.IsEmpty) return true;

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
			return string.Join(separator ?? string.Empty, (mSegments ?? new StringKey[0]).Select(x => x.ToString()));
		}
	}
}
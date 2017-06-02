using System;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public struct StringKey
	{
		private readonly string mRawData;

		public StringKey([NotNull] string rawData)
		{
			if (string.IsNullOrEmpty(rawData))
			{
				throw new ArgumentNullException(nameof(rawData));
			}

			mRawData = rawData;
		}

		public string RawData => mRawData;
		public StringKey Normalize() => IsEmpty 
			? new StringKey() 
			: new StringKey(mRawData.ToLowerInvariant().Trim());
		public bool IsEmpty => string.IsNullOrEmpty(mRawData);

		public StringKeySequence Concat(StringKey key)
		{
			var segments = new[] {this, key}.Where(x => !x.IsEmpty).ToArray();
			if (segments.Length == 0)
			{
				return new StringKeySequence();
			}

			return new StringKeySequence(segments);
		}
		public StringKeySequence Concat(StringKeySequence keySequence)
		{
			var segments = new[] { this }.Concat(keySequence.Segments).Where(x => !x.IsEmpty).ToArray();
			if (segments.Length == 0)
			{
				return new StringKeySequence();
			}

			return new StringKeySequence(segments);
		}

		public override int GetHashCode() => mRawData?.GetHashCode() ?? 0;
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is StringKey && Equals((StringKey)obj);
		}
		public bool Equals(StringKey other)
		{
			return string.Equals(mRawData, other.mRawData);
		}

		public override string ToString() => mRawData ?? string.Empty;

		public static implicit operator StringKey(string rawData) => string.IsNullOrEmpty(rawData) ? new StringKey() : new StringKey(rawData);
		public static implicit operator string(StringKey key) => key.mRawData;
	}
}
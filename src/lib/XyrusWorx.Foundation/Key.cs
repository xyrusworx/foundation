using JetBrains.Annotations;
using System.Linq;

namespace XyrusWorx
{
	[PublicAPI]
	public struct Key<T> where T: struct
	{
		private readonly T mRawData;

		public Key(T rawData)
		{
			mRawData = rawData;
		}
		public T RawData => mRawData;

		public KeySequence<T> Concat(Key<T> key)
		{
			var segments = new[] { this, key }.ToArray();
			if (segments.Length == 0)
			{
				return new KeySequence<T>();
			}

			return new KeySequence<T>(segments);
		}
		public KeySequence<T> Concat(KeySequence<T> keySequence)
		{
			var segments = new[] { this }.Concat(keySequence.Segments).ToArray();
			if (segments.Length == 0)
			{
				return new KeySequence<T>();
			}

			return new KeySequence<T>(segments);
		}

		public override int GetHashCode()
		{
			// ReSharper disable once ImpureMethodCallOnReadonlyValueField
			return mRawData.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Key<T> && Equals((Key<T>)obj);
		}
		public bool Equals(Key<T> other)
		{
			return Equals(mRawData, other.mRawData);
		}

		// ReSharper disable once ImpureMethodCallOnReadonlyValueField
		public override string ToString() => mRawData.ToString();
	}
}
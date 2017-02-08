using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public abstract class BlobStore : Resource, IBlobStore
	{
		public BinaryContainer this[string key]
		{
			[NotNull]
			get
			{
				return new StorageOwnedBinaryContainer(this, key.AsKey());
			}

			[CanBeNull]
			set
			{
				if (value == null)
				{
					Erase(key.AsKey());
					return;
				}

				using (var writer = Open(key.AsKey()).Write())
				{
					value.Copy(writer);
				}
			}
		}

		public abstract StringKeySequence Identifier { get; }
		public virtual bool IsReadOnly => false;

		public abstract bool Exists(StringKey key);
		public abstract void Erase(StringKey key);

		public BinaryContainer Open(StringKey key) => this[key.ToString()];

		public void Append(StringKey key, BinaryContainer data)
		{
			if (IsReadOnly)
			{
				throw new NotSupportedException($"This storage instance (\"{this}\") does not allow write operations.");
			}

			using (var writer = Open(key).Append())
			{
				data.Copy(writer);
			}
		}
		public void Write(StringKey key, BinaryContainer data)
		{
			if (IsReadOnly)
			{
				throw new NotSupportedException($"This storage instance (\"{this}\") does not allow write operations.");
			}

			using (var writer = Open(key).Write())
			{
				data.Copy(writer);
			}
		}

		public void Append(StringKey key, TextContainer data) => Append(key, data.AsBinary());
		public void Write(StringKey key, TextContainer data) => Write(key, data.AsBinary());

		[NotNull]
		protected internal abstract Stream OpenStream(StringKey key, AccessMode accessMode);

		public void Clear()
		{
			foreach (var key in Enumerate())
			{
				Erase(key);
			}
		}

		public IEnumerable<string> Keys => Enumerate().Select(x => x.ToString());

		public abstract IBlobStore GetChildStore(StringKey childStorageKey, bool? isReadOnly = null);
		public abstract IEnumerable<StringKey> GetChildStoreKeys();

		public abstract bool HasChildStore(StringKey childStorageKey);

		IEnumerator<KeyValuePair<string, BinaryContainer>> IEnumerable<KeyValuePair<string, BinaryContainer>>.GetEnumerator() => EnumerateKeyValuePairs().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => EnumerateKeyValuePairs().GetEnumerator();
		
		protected abstract IEnumerable<StringKey> Enumerate();

		void ICollection<KeyValuePair<string, BinaryContainer>>.Add(KeyValuePair<string, BinaryContainer> item) => this[item.Key] = item.Value;
		bool ICollection<KeyValuePair<string, BinaryContainer>>.Contains(KeyValuePair<string, BinaryContainer> item) => Exists(new StringKey(item.Key));
		void ICollection<KeyValuePair<string, BinaryContainer>>.CopyTo(KeyValuePair<string, BinaryContainer>[] array, int arrayIndex) => EnumerateKeyValuePairs().ToArray().CopyTo(array, arrayIndex);
		bool ICollection<KeyValuePair<string, BinaryContainer>>.Remove(KeyValuePair<string, BinaryContainer> item)
		{
			var key = new StringKey(item.Key);
			if (!Exists(key))
			{
				return false;
			}

			Erase(key);
			return true;
		}
		int ICollection<KeyValuePair<string, BinaryContainer>>.Count => EnumerateKeyValuePairs().Count();

		void IDictionary<string, BinaryContainer>.Add(string key, BinaryContainer value) => this[key] = value;
		bool IDictionary<string, BinaryContainer>.ContainsKey(string key) => Exists(new StringKey(key));
		bool IDictionary<string, BinaryContainer>.TryGetValue(string key, out BinaryContainer value)
		{
			var k = new StringKey(key);
			if (!Exists(k))
			{
				value = null;
				return false;
			}

			value = this[k.ToString()];
			return true;
		}
		bool IDictionary<string, BinaryContainer>.Remove(string key)
		{
			var k = new StringKey(key);
			if (!Exists(k))
			{
				return false;
			}

			Erase(k);
			return true;
		}

		ICollection<string> IDictionary<string, BinaryContainer>.Keys => Keys.AsArray();
		ICollection<BinaryContainer> IDictionary<string, BinaryContainer>.Values => Keys.Select(x => this[x]).AsArray();

		private IEnumerable<KeyValuePair<string, BinaryContainer>> EnumerateKeyValuePairs()
		{
			foreach (var key in Enumerate())
			{
				yield return new KeyValuePair<string, BinaryContainer>(key.ToString(), this[key.ToString()]);
			}
		}
	}
}
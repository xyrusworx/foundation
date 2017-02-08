using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public abstract class KeyValueStore : KeyValueStore<object>
	{
	}

	[PublicAPI]
	public abstract class KeyValueStore<T> : Resource, IKeyValueStore<T>, IKeyValueStore
	{
		public void Clear()
		{
			foreach (var key in Enumerate().ToArray())
			{
				Write(key, default(T));
			}
		}
		public T this[string key]
		{
			get { return Read(new StringKey(key)); }
			set { Write(new StringKey(key), value); }
		}
		
		public virtual bool IsReadOnly => false;
		public virtual bool Exists(StringKey key)
		{
			return GetValue(key) != null;
		}

		public T Read(StringKey key)
		{
			if (Exists(key))
			{
				return GetValue(key);
			}

			return default(T);
		}
		public void Write(StringKey key, T value)
		{
			if (IsReadOnly)
			{
				throw new NotSupportedException($"This storage instance (\"{this}\") does not allow write operations.");
			}

			SetValue(key, value);
		}

		protected abstract T GetValue(StringKey key);
		protected abstract void SetValue(StringKey key, T value);

		public void SetDefault(StringKey key, T defaultValue)
		{
			if (!Exists(key))
			{
				Write(key, defaultValue);
			}
		}
		public IEnumerable<StringKey> GetKeys() => Enumerate();

		IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator() => EnumerateKeyValuePairs().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => EnumerateKeyValuePairs().GetEnumerator();
		
		protected abstract IEnumerable<StringKey> Enumerate();

		void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item) => Write(new StringKey(item.Key), item.Value);
		bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item) => Exists(new StringKey(item.Key));
		void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) => EnumerateKeyValuePairs().ToArray().CopyTo(array, arrayIndex);
		bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
		{
			var key = new StringKey(item.Key);
			if (!Exists(key))
			{
				return false;
			}

			Write(key, default(T));
			return true;
		}
		int ICollection<KeyValuePair<string, T>>.Count => EnumerateKeyValuePairs().Count();

		void IDictionary<string, T>.Add(string key, T value) => Write(new StringKey(key), value);
		bool IDictionary<string, T>.ContainsKey(string key) => Exists(new StringKey(key));
		bool IDictionary<string, T>.TryGetValue(string key, out T value)
		{
			var k = new StringKey(key);
			if (!Exists(k))
			{
				value = default(T);
				return false;
			}

			value = Read(k);
			return true;
		}
		bool IDictionary<string, T>.Remove(string key)
		{
			var k = new StringKey(key);
			if (!Exists(k))
			{
				return false;
			}

			Write(k, default(T));
			return true;
		}

		ICollection<string> IDictionary<string, T>.Keys => GetKeys().Select(x => x.ToString()).AsArray();
		ICollection<T> IDictionary<string, T>.Values => GetKeys().Select(Read).AsArray();

		object IKeyValueStore.Read(StringKey key) => Read(key);
		void IKeyValueStore.Write(StringKey key, object value) => Write(key, (T) value);
		void IKeyValueStore.SetDefault(StringKey key, object defaultValue) => SetDefault(key, (T) defaultValue);

		private IEnumerable<KeyValuePair<string, T>> EnumerateKeyValuePairs()
		{
			foreach (var key in Enumerate())
			{
				yield return new KeyValuePair<string, T>(key.ToString(), Read(key));
			}
		}
	}
}

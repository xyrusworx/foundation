using System.Collections.Generic;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public class MemoryKeyValueStore<T> : KeyValueStore<T>
	{
		private readonly IDictionary<StringKey, T> mData;

		public MemoryKeyValueStore()
		{
			mData = new Dictionary<StringKey, T>();
		}
		public MemoryKeyValueStore(IDictionary<StringKey, T> data)
		{
			mData = data ?? new Dictionary<StringKey, T>();
		}

		public override bool Exists(StringKey key)
		{
			return mData.ContainsKey(key);
		}

		protected override T GetValue(StringKey key)
		{
			return mData.GetValueByKeyOrDefault(key);
		}
		protected override void SetValue(StringKey key, T value)
		{
			mData.AddOrUpdate(key, value);
		}
		protected override IEnumerable<StringKey> Enumerate()
		{
			return mData.Keys;
		}
	}

	[PublicAPI]
	public class MemoryKeyValueStore : MemoryKeyValueStore<object>
	{
		public MemoryKeyValueStore()
		{
		}
		public MemoryKeyValueStore(IDictionary<StringKey, object> data) : base(data)
		{
		}
	}
}
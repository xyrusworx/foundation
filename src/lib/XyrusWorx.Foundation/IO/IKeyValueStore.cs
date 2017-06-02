using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public interface IKeyValueStore<T> : IDictionary<string, T>
	{
		bool Exists(StringKey key);
		IEnumerable<StringKey> GetKeys();

		T Read(StringKey key);
		void Write(StringKey key, T value);
		void SetDefault(StringKey key, T defaultValue);
	}

	[PublicAPI]
	public interface IKeyValueStore
	{
		bool Exists(StringKey key);
		IEnumerable<StringKey> GetKeys();

		object Read(StringKey key);
		void Write(StringKey key, object value);
		void SetDefault(StringKey key, object defaultValue);
	}
}
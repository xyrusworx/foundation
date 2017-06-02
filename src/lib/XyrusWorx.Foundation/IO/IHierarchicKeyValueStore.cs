using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public interface IHierarchicKeyValueStore : IHierarchicKeyValueStore<object>
	{
	}

	[PublicAPI]
	public interface IHierarchicKeyValueStore<T>
	{
		bool Exists(StringKeySequence sequence);
		IEnumerable<StringKeySequence> GetKeys();

		T Read(StringKeySequence key);
		void Write(StringKeySequence sequence, T value);
		void SetDefault(StringKeySequence key, T defaultValue);
	}
}
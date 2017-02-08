using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public interface IBlobStore : IDictionary<string, BinaryContainer>
	{
		StringKeySequence Identifier { get; }

		bool Exists(StringKey key);
		void Erase(StringKey key);

		BinaryContainer Open(StringKey key);

		void Append(StringKey key, BinaryContainer data);
		void Append(StringKey key, TextContainer data);

		void Write(StringKey key, BinaryContainer data);
		void Write(StringKey key, TextContainer data);

		[NotNull]
		IBlobStore GetChildStore(StringKey childStorageKey, bool? isReadOnly = null);

		[NotNull]
		IEnumerable<StringKey> GetChildStoreKeys();

		bool HasChildStore(StringKey childStorageKey);
	}
}
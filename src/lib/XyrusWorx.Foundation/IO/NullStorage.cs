using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public sealed class NullStorage : BlobStore
	{
		private Guid mIdentifier;

		public NullStorage()
		{
			mIdentifier = Guid.NewGuid();
			Identifier = new StringKeySequence(new StringKey(mIdentifier.ToString("N").Substring(8)));
		}
		public NullStorage(Guid identifier)
		{
			mIdentifier = identifier;
			Identifier = new StringKeySequence(new StringKey(mIdentifier.ToString("N").Substring(8)));
		}

		public override StringKeySequence Identifier { get; }

		public override bool Exists(StringKey key) => false;
		public override void Erase(StringKey key) {}

		protected internal override Stream OpenStream(StringKey key, AccessMode accessMode)
		{
			return new MemoryStream();
		}
		protected override IEnumerable<StringKey> Enumerate()
		{
			yield break;
		}

		public override IBlobStore GetChildStore(StringKey childStorageKey, bool? isReadOnly = null) => new NullStorage(mIdentifier);
		public override IEnumerable<StringKey> GetChildStoreKeys()
		{
			yield break;
		}

		public override bool HasChildStore(StringKey childStorageKey) => false;
	}
}
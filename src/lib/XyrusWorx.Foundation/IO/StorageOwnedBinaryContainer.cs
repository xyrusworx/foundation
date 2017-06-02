using System;
using System.IO;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	class StorageOwnedBinaryContainer : BinaryContainer
	{
		private readonly BlobStore mStore;
		private readonly StringKey mKey;

		public StorageOwnedBinaryContainer([NotNull] BlobStore store, StringKey key)
		{
			if (store == null)
			{
				throw new ArgumentNullException(nameof(store));
			}

			mStore = store;
			mKey = key;
		}

		protected override Stream OpenStream(AccessMode accessMode)
		{
			return mStore.OpenStream(mKey, accessMode);
		}

		public override StringKey Identifier => mKey.ToString().AsKey();
	}
}
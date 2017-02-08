using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	class CompressedBlobStoreSection : BlobStore
	{
		private readonly ZipArchive mArchive;
		private readonly IDictionary<StringKey, CompressedBlobStoreSection> mChildFolders;
		private readonly HashSet<StringKey> mElementNames;
		private readonly List<StringKey> mOriginalNames;
		private readonly List<StringKey> mOriginalChildFolderNames;

		private StringKeySequence mThisFolderName;

		internal CompressedBlobStoreSection(StringKeySequence thisFolderName, [NotNull] ZipArchive archive)
		{
			if (archive == null)
			{
				throw new ArgumentNullException(nameof(archive));
			}

			mArchive = archive;
			mThisFolderName = thisFolderName;
			mChildFolders = new Dictionary<StringKey, CompressedBlobStoreSection>();
			mElementNames = new HashSet<StringKey>();
			mOriginalNames = new List<StringKey>();
			mOriginalChildFolderNames = new List<StringKey>();
		}

		public override StringKeySequence Identifier => mThisFolderName;

		public override bool Exists(StringKey key)
		{
			return mElementNames.Contains(key.Normalize());
		}
		public override void Erase(StringKey key)
		{
			if (mArchive.Mode == ZipArchiveMode.Create || !Exists(key))
			{
				return;
			}

			var completeName = mThisFolderName.Concat(key).ToString("\\");
			var existingEntry = mArchive.GetEntry(completeName);

			existingEntry.Delete();
		}

		public override IBlobStore GetChildStore(StringKey childStorageKey, bool? isReadOnly = null)
		{
			if (!mChildFolders.ContainsKey(childStorageKey.Normalize()))
			{
				var childStore = new CompressedBlobStoreSection(mThisFolderName.Concat(childStorageKey), mArchive);

				AddChildFolder(childStorageKey, childStore);

				return childStore;
			}

			return mChildFolders[childStorageKey.Normalize()];
		}
		public override IEnumerable<StringKey> GetChildStoreKeys()
		{
			return mOriginalChildFolderNames;
		}
		public override bool HasChildStore(StringKey childStorageKey)
		{
			return mChildFolders.ContainsKey(childStorageKey.Normalize());
		}

		internal void AddChildFolder(StringKey key, [NotNull] CompressedBlobStoreSection folder)
		{
			if (folder == null)
			{
				throw new ArgumentNullException(nameof(folder));
			}

			if (mChildFolders.ContainsKey(key.Normalize()))
			{
				mChildFolders[key.Normalize()] = folder;
				return;
			}

			mChildFolders.Add(key.Normalize(), folder);
			mOriginalChildFolderNames.Add(key);
		}
		internal void AddElement(StringKey key)
		{
			if (mElementNames.Contains(key.Normalize()))
			{
				return;
			}

			mElementNames.Add(key.Normalize());
			mOriginalNames.Add(key);
		}

		internal IEnumerable<StringKey> Elements() => Enumerate();

		protected internal override Stream OpenStream(StringKey key, AccessMode accessMode)
		{
			if (accessMode.HasFlag(AccessMode.Append))
			{
				throw new NotSupportedException("Appending is not supported for compressed BLOB stores.");
			}

			var completeName = mThisFolderName.Concat(key).ToString("\\");

			if (!Exists(key))
			{
				if (accessMode == AccessMode.Read)
				{
					return new MemoryStream();
				}

				AddElement(key);

				return mArchive.CreateEntry(completeName).Open();
			}

			return mArchive.GetEntry(completeName).Open();
		}
		protected override IEnumerable<StringKey> Enumerate()
		{
			return mOriginalNames;
		}
	}
}
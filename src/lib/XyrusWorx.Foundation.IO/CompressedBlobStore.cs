using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public class CompressedBlobStore : BlobStore
	{
		private readonly CompressedBlobStoreSection mRootFolder;
		private readonly IList<CompressedBlobStoreEntryInfo> mEntries;
		private readonly ZipArchive mArchive;

		private CompressedBlobStore([NotNull] ZipArchive archive, [NotNull] IList<CompressedBlobStoreEntryInfo> entries)
		{
			if (archive == null)
			{
				throw new ArgumentNullException(nameof(archive));
			}
			if (entries == null)
			{
				throw new ArgumentNullException(nameof(entries));
			}

			mArchive = archive;
			mEntries = entries;
			mRootFolder = CreateFolderStructure();
		}

		[NotNull]
		public static CompressedBlobStore Create() => FromBinaryContainer(new MemoryBinaryContainer());

		[NotNull]
		public static CompressedBlobStore FromBinaryContainer([NotNull] BinaryContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			var entries = new List<CompressedBlobStoreEntryInfo>();
			var exists = true;

			using (var reader = container.Read())
			{
				if (reader.BaseStream.Length == 0)
				{
					exists = false;
				}
				else
				{
					using (var readArchive = new ZipArchive(reader.BaseStream, ZipArchiveMode.Read, true))
					{
						foreach (var entry in readArchive.Entries)
						{
							var path = entry.FullName.Split('\\');
							if (path.Length == 0)
							{
								continue;
							}

							var head = path.Length == 1 ? new string[0] : path.Take(path.Length - 1).ToArray();
							var tail = path.Last();

							var info = new CompressedBlobStoreEntryInfo
							{
								Path = head.Length == 0 ? new StringKeySequence() : new StringKeySequence(head),
								Key = new StringKey(tail)
							};

							entries.Add(info);
						}
					}
				}
			}

			if (!exists)
			{
				using (new ZipArchive(container.ReadWrite(), ZipArchiveMode.Create, false)) { }
			}
			
			var writeArchive = new ZipArchive(container.ReadWrite(), ZipArchiveMode.Update, false);

			return new CompressedBlobStore(writeArchive, entries);
		}

		public override bool Exists(StringKey key) => mRootFolder.Exists(key);
		public override void Erase(StringKey key) => mRootFolder.Erase(key);

		public override IBlobStore GetChildStore(StringKey childStorageKey, bool? isReadOnly = null) => mRootFolder.GetChildStore(childStorageKey, isReadOnly);
		public override IEnumerable<StringKey> GetChildStoreKeys() => mRootFolder.GetChildStoreKeys();
		public override bool HasChildStore(StringKey childStorageKey) => mRootFolder.HasChildStore(childStorageKey);

		public override StringKeySequence Identifier => mRootFolder.Identifier;

		protected override IEnumerable<StringKey> Enumerate() => mRootFolder.Elements();
		protected override Stream OpenStream(StringKey key, AccessMode accessMode) => mRootFolder.GetStream(key, accessMode);

		protected override void DisposeOverride()
		{
			mArchive.Dispose();
		}

		private CompressedBlobStoreSection CreateFolderStructure(StringKeySequence baseName = default(StringKeySequence))
		{
			var folder = new CompressedBlobStoreSection(baseName, mArchive);

			var childFolderKeys =
				from entry in mEntries

				where entry.Path.Segments.Length == baseName.Segments.Length + 1

				let m1 = baseName.IsEmpty && entry.Path.Segments.Length == 1
				let m2 = baseName.Segments.Select((k, i) => entry.Path.Segments[i].Equals(k)).All(x => x)

				where m1 || m2
				select entry.Path;

			foreach (var key in childFolderKeys)
			{
				folder.AddChildFolder(key.Segments.Last(), CreateFolderStructure(key));
			}

			var elementKeys = new List<StringKey>(

				from entry in mEntries
				where baseName.Equals(entry.Path)
				select entry.Key);

			foreach (var element in elementKeys)
			{
				folder.AddElement(element);
			}

			return folder;
		}
	}
}
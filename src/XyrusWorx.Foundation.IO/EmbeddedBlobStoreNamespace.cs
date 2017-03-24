using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	class EmbeddedBlobStoreNamespace : BlobStore
	{
		private StringKeySequence mName;

		private readonly Assembly mAssembly;
		private readonly IDictionary<StringKey, EmbeddedBlobStoreNamespace> mChildren;
		private readonly HashSet<StringKey> mNames;

		internal EmbeddedBlobStoreNamespace(
			StringKeySequence baseName, 
			StringKeySequence name, 
			[NotNull] Assembly assembly, 
			[NotNull] IDictionary<StringKey, EmbeddedBlobStoreNamespace> children, 
			[NotNull] IEnumerable<StringKey> names)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (children == null)
			{
				throw new ArgumentNullException(nameof(children));
			}

			if (names == null)
			{
				throw new ArgumentNullException(nameof(names));
			}

			mAssembly = assembly;
			mName = baseName.Concat(name);
			mChildren = children;
			mNames = new HashSet<StringKey>(names);
		}

		public override bool Exists(StringKey key)
		{
			return mNames.Contains(key);
		}
		public override void Erase(StringKey key)
		{
			throw new NotSupportedException("Removing objects from embedded BLOB stores is not supported.");
		}

		internal Stream GetStream(StringKey key, AccessMode accessMode) => OpenStream(key, accessMode);
		protected override Stream OpenStream(StringKey key, AccessMode accessMode)
		{
			if (accessMode.HasFlag(AccessMode.Write) || accessMode.HasFlag(AccessMode.Append))
			{
				throw new NotSupportedException("Writing objects to embedded BLOB stores is not supported.");
			}

			var completeName = mName.Concat(key).ToString(".");

			var stream = mAssembly.GetManifestResourceStream(completeName);
			if (stream == null)
			{
				return new MemoryStream();
			}

			return stream;
		}
		protected override IEnumerable<StringKey> Enumerate()
		{
			return mNames;
		}

		public override IBlobStore GetChildStore(StringKey childStorageKey, bool? isReadOnly = null)
		{
			if (!mChildren.ContainsKey(childStorageKey))
			{
				return new NullStorage();
			}

			return mChildren[childStorageKey];
		}
		public override IEnumerable<StringKey> GetChildStoreKeys()
		{
			return mChildren.Keys;
		}
		public override bool HasChildStore(StringKey childStorageKey)
		{
			return mChildren.ContainsKey(childStorageKey);
		}

		public override StringKeySequence Identifier => mName;
	}
}
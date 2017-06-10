using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public class FileSystemStore : BlobStore
	{
		private readonly string mDirectoryName;
		private readonly Encoding mEncoding;
		private readonly bool mIsReadOnly;

		public FileSystemStore([NotNull] string directoryName, [CanBeNull] Encoding encoding = null, bool isReadOnly = false)
		{
			if (directoryName == null)
			{
				throw new ArgumentNullException(nameof(directoryName));
			}

			if (string.IsNullOrWhiteSpace(directoryName))
			{
				directoryName = Directory.GetCurrentDirectory();
			}

			if (Path.GetInvalidPathChars().Any(x => directoryName.ToString().Contains(x)))
			{
				var sb = new StringBuilder();

				sb.AppendLine($"The directory name contains invalid characters: {directoryName}.");
				sb.Append("None of the following characters are allowed:");

				foreach (var c in Path.GetInvalidPathChars())
				{
					sb.Append($" {c}");
				}

				throw new ArgumentException(sb.ToString(), nameof(directoryName));
			}

			var tokens = directoryName.Split(Path.PathSeparator);

			mDirectoryName = directoryName;
			Identifier = new StringKeySequence(tokens.Select(x => new StringKey(x)).ToArray());

			mEncoding = encoding;
			mIsReadOnly = isReadOnly;

			if (!Directory.Exists(directoryName))
			{
				try
				{
					if (!isReadOnly)
					{
						Directory.CreateDirectory(directoryName);
					}
				}
				catch (UnauthorizedAccessException)
				{
					mIsReadOnly = true;
				}
			}
		}

		public override StringKeySequence Identifier { get; }
		public override bool IsReadOnly => mIsReadOnly;

		public override bool Exists(StringKey key)
		{
			var fileName = Path.Combine(mDirectoryName, key);

			return File.Exists(fileName);
		}
		public override void Erase(StringKey key)
		{
			var fileName = Path.Combine(mDirectoryName, key);

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		protected internal override Stream OpenStream(StringKey key, AccessMode accessMode)
		{
			FileMode mode;
			FileAccess access;

			switch (accessMode)
			{
				case AccessMode.Read:
					mode = FileMode.OpenOrCreate;
					access = FileAccess.Read;
					break;
				case AccessMode.Write:
					mode = FileMode.OpenOrCreate;
					access = FileAccess.Write;
					break;
				case AccessMode.Append:
					mode = FileMode.Append;
					access = FileAccess.Write;
					break;
				case AccessMode.Read | AccessMode.Write:
					mode = FileMode.OpenOrCreate;
					access = FileAccess.ReadWrite;
					break;
				default:
					throw new NotSupportedException($"Invalid file access flags: {accessMode}");
			}

			var fileName = Path.Combine(mDirectoryName, key);

			return File.Open(fileName, mode, access, FileShare.ReadWrite);
		}
		protected override IEnumerable<StringKey> Enumerate()
		{
			if (!Directory.Exists(mDirectoryName))
			{
				yield break;
			}

			var files = Directory.GetFiles(mDirectoryName);
			foreach (var file in files)
			{
				var fileName = Path.GetFileName(file);
				if (string.IsNullOrWhiteSpace(fileName))
				{
					continue;
				}

				yield return new StringKey(fileName);
			}
		}

		public override bool HasChildStore(StringKey childStorageKey)
		{
			var directoryName = Path.Combine(mDirectoryName, childStorageKey);

			return Directory.Exists(directoryName);
		}

		public override IBlobStore GetChildStore(StringKey childStorageKey, bool? isReadOnly = null)
		{
			var directoryName = Path.Combine(mDirectoryName, childStorageKey);

			return new FileSystemStore(directoryName, mEncoding, isReadOnly ?? mIsReadOnly);
		}
		public override IEnumerable<StringKey> GetChildStoreKeys() =>
			from directoryName in Directory.GetDirectories(mDirectoryName)
			select new StringKey(new DirectoryInfo(directoryName).Name);
	}
}
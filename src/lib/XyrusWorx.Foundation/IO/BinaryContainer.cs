using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public abstract class BinaryContainer
	{
		public abstract StringKey Identifier { get; }

		[NotNull] public Stream Read() => OpenStream(AccessMode.Read);
		[NotNull] public Stream Write() => OpenStream(AccessMode.Write);
		[NotNull] public Stream Append() => OpenStream(AccessMode.Append);
		[NotNull] public Stream ReadWrite() => OpenStream(AccessMode.Read | AccessMode.Write);

		[NotNull]
		protected abstract Stream OpenStream(AccessMode accessMode);

		[NotNull]
		public byte[] ReadBytes()
		{
			using (var reader = Read())
			{
				var buffer = new byte[16 * 1024]; // 16k;

				using (var memory = new MemoryStream())
				{
					int read;

					while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
					{
						memory.Write(buffer, 0, read);
					}

					return memory.ToArray();
				}
			}
		}
		public void WriteBytes(byte[] bytes)
		{
			if (bytes == null)
			{
				return;
			}

			using (var writer = new BinaryWriter(Write()))
			{
				writer.Write(bytes);
			}
		}
		public void AppendBytes(byte[] bytes)
		{
			if (bytes == null)
			{
				return;
			}

			using (var writer = new BinaryWriter(Append()))
			{
				writer.Write(bytes);
			}
		}

		public void Copy([NotNull] Stream target)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			using (var reader = Read())
			{
				var buffer = new byte[16 * 1024]; // 16k;

				int read;

				while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
				{
					target.Write(buffer, 0, read);
				}

			}
		}

		[NotNull] public TextContainer AsText()
		{
			return new TextContainer(this);
		}
		[NotNull] public TextContainer AsText([NotNull] Encoding encoding)
		{
			if (encoding == null)
			{
				throw new ArgumentNullException(nameof(encoding));
			}

			return new TextContainer(this, encoding);
		}
	}
}
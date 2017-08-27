using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public class TextContainer
	{
		private readonly BinaryContainer mRawData;
		private readonly Encoding mEncoding;

		public TextContainer([NotNull] BinaryContainer rawData, [CanBeNull] Encoding encoding = null)
		{
			if (rawData == null)
			{
				throw new ArgumentNullException(nameof(rawData));
			}

			mRawData = rawData;
			mEncoding = encoding;
		}

		[NotNull]
		public BinaryContainer AsBinary() => mRawData;

		[NotNull] public TextReader Read()
		{
			var stream = mRawData.Read();

			return new StreamReader(stream, mEncoding ?? Encoding.UTF8);
		}
		[NotNull] public TextWriter Write()
		{
			var stream = mRawData.Write();

			return new StreamWriter(stream, mEncoding ?? Encoding.UTF8);
		}
		[NotNull] public TextWriter Append()
		{
			var stream = mRawData.Append();

			return new StreamWriter(stream, mEncoding ?? Encoding.UTF8);
		}

		[NotNull]
		public string ReadString()
		{
			using (var reader = Read())
			{
				return reader.ReadToEnd();
			}
		}
		public void WriteString(string text)
		{
			using (var writer = Write())
			{
				writer.Write(text ?? string.Empty);
			}
		}
		public void AppendString(string text)
		{
			using (var writer = Append())
			{
				writer.Write(text ?? string.Empty);
			}
		}

		public void Copy([NotNull] TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			using (var reader = Read())
			{
				var buffer = new char[16 * 1024]; // 16k;

				int read;

				while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
				{
					writer.Write(buffer, 0, read);
				}

			}
		}
	}
}
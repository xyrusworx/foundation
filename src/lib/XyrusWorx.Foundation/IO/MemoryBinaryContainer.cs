using System;
using System.IO;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public class MemoryBinaryContainer : BinaryContainer
	{
		private byte[] mBuffer;

		public MemoryBinaryContainer() : this(Guid.NewGuid().ToString("N").Substring(8).ToLower().AsKey()) { }
		public MemoryBinaryContainer(StringKey identifier)
		{
			Identifier = identifier;
		}

		public override StringKey Identifier { get; }

		protected override Stream OpenStream(AccessMode accessMode)
		{
			var memoryStream = new ExtendingMemoryStream();

			if (accessMode.HasFlag(AccessMode.Read))
			{
				memoryStream.Write(mBuffer, 0, mBuffer.Length);
			}

			if (accessMode.HasFlag(AccessMode.Append))
			{
				memoryStream.OnClose += (o, e) =>
				{
					var l = mBuffer.Length;
					mBuffer = new byte[l + memoryStream.Length];
					memoryStream.Write(mBuffer, l, (int)memoryStream.Length);
				};
			}
			else if (accessMode.HasFlag(AccessMode.Write))
			{
				memoryStream.Seek(0, SeekOrigin.Begin);
				memoryStream.OnClose += (o, e) =>
				{
					mBuffer = memoryStream.ToArray();
				};
			}

			return memoryStream;
		}
	}
}
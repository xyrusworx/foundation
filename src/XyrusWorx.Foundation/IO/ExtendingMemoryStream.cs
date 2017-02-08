using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public sealed class ExtendingMemoryStream : Stream
	{
		private List<byte> mData;
		private long mCursor;

		public ExtendingMemoryStream()
		{
			mData = new List<byte>();
		}
		public ExtendingMemoryStream(byte[] data) : this()
		{
			mData.AddRange(data);
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count + offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			var j = 0;
			var c = mCursor;

			for (var i = 0; i < count; i++)
			{
				var k = c + i;
				if (k >= mData.Count)
				{
					break;
				}

				buffer[i + offset] = mData[(int)k];

				j++;
				mCursor++;
			}
			return j;
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count + offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			var c = mCursor;
			for (var i = 0; i < count; i++)
			{
				var k = c + i;

				if (k >= mData.Count)
				{
					mData.Add(buffer[i + offset]);
				}
				else
				{
					mData[(int)k] = buffer[i + offset];
				}

				mCursor++;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					mCursor = offset;
					break;
				case SeekOrigin.Current:
					mCursor += offset;
					break;
				case SeekOrigin.End:
					mCursor = mData.Count + offset;
					break;
			}

			return mCursor;
		}
		public override void SetLength(long value)
		{
			if (value < mData.Count)
			{
				mData = mData.Take((int)value).ToList();
			}
			else if (value > mData.Count)
			{
				var append = new byte[value - mData.Count];
				mData.AddRange(append);
			}
		}

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => true;

		public override long Length => mData.Count;
		public override long Position
		{
			get { return mCursor; }
			set { mCursor = value; }
		}

		[NotNull]
		public byte[] ToArray() => mData.ToArray();

		public event EventHandler OnClose;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				OnClose?.Invoke(this, new EventArgs());
				mData = new List<byte>();
			}

			base.Dispose(disposing);
		}
	}
}
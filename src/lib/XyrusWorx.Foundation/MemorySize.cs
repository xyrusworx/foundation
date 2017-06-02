using System.Diagnostics;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI, DebuggerDisplay("DisplayString")]
	public struct MemorySize
	{
		private readonly long mBytes;

		public MemorySize(long valueInBytes)
		{
			mBytes = valueInBytes;
		}
		public long Bytes => mBytes;

		[NotNull]
		public string DisplayString
		{
			get
			{
				if (mBytes < 1024) return $"{mBytes:0.000} Bytes";

				if (KiB < 1024) return $"{KiB:0,000} KiB";
				if (MiB < 1024) return $"{MiB:0,000.00} MiB";
				if (GiB < 1024) return $"{GiB:0,000.00} GiB";

				return $"{TiB:###,###,###,##0,000.00} TiB";
			}
		}

		[Pure] public override int GetHashCode() => mBytes.GetHashCode();
		[Pure] public override bool Equals(object obj) => obj is MemorySize && Equals(((MemorySize)obj).mBytes, mBytes);
		[Pure] public override string ToString() => mBytes.ToString();

		// base 2
		public long KiB => Bytes / 1024;
		public long MiB => KiB / 1024;
		public long GiB => MiB / 1024;
		public long TiB => GiB / 1024;

		// base 10
		public long KB => Bytes / 1000;
		public long MB => KB / 1000;
		public long GB => MB / 1000;
		public long TB => GB / 1000;
	}
}
using System;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI, Flags]
	public enum AccessMode
	{
		Read = 1,
		Write = 2,
		Append = 4
	}
}
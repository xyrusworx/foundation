using System;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Provider
{
	[Flags, PublicAPI]
	public enum WebServiceVerbs
	{
		None = 0,
		Get = 1,
		Post = 2,
		Put = 4,
		Patch = 8,
		Delete = 16,
		Copy = 32,
		Head = 64,
		Options = 128,
		Link = 256,
		Unlink = 512,
		Purge = 1024,
		Lock = 2048,
		Unlock = 4096,
		Propfind = 8192,
		View = 16384,
		Index = 32768,
		All = 0x7fffffff
	}
}
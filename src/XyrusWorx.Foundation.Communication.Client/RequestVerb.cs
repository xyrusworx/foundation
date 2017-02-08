using JetBrains.Annotations;

namespace XyrusWorx.Foundation.Communication.Client
{
	[PublicAPI]
	public enum RequestVerb
	{
		Get = 0,
		Post,
		Put,
		Patch,
		Delete,
		Copy,
		Head,
		Options,
		Link,
		Unlink,
		Purge,
		Lock,
		Unlock,
		Propfind,
		View,
		Index,
	}
}
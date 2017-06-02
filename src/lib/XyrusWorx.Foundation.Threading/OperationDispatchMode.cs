using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public enum OperationDispatchMode
	{
		Synchronous,
		BackgroundThread,
		ThreadPoolUserWorkItem
	}
}
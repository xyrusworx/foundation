using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public delegate void OperationUnhandledExceptionEventHandler(object sender, OperationUnhandledExceptionEventArgs args);
}
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public enum SequenceErrorBehavior
	{
		Ignore,
		Continue,
		Abort
	}
}
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public enum OperationSequenceSchedulingMode
	{
		Sequential,
		Parallel
	}
}
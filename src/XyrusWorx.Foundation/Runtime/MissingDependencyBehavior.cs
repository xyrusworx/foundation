using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public enum MissingDependencyBehavior
	{
		Exception,
		ResolveNull
	}
}
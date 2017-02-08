using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public interface IDependencyDefinition<in TBaseType> where TBaseType : class
	{
		[NotNull]
		IDependencyDefinition<TBaseType> AndOn([NotNull] TBaseType element);
	}
}
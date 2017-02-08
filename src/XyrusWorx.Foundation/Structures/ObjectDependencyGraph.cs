using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class ObjectDependencyGraph : ObjectDependencyGraph<object>
	{
		
	}

	[PublicAPI]
	public class ObjectDependencyGraph<TBaseType> : DependencyGraph<TBaseType, ObjectDependencyGraphNode<TBaseType>> where TBaseType : class
	{
		protected sealed override ObjectDependencyGraphNode<TBaseType> CreateNode(TBaseType element)
		{
			return new ObjectDependencyGraphNode<TBaseType>(this, element);
		}
		protected internal sealed override bool AreEqual(TBaseType from, TBaseType to)
		{
			return ReferenceEquals(from, to);
		}
	}
}
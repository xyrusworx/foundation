using System;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public sealed class ObjectDependencyGraphNode<TBaseType> : DependencyGraphNode<TBaseType>, IDependencyDefinition<TBaseType> where TBaseType : class
	{
		internal ObjectDependencyGraphNode([NotNull] ObjectDependencyGraph<TBaseType> graph, [NotNull] TBaseType sourceNode) : base(graph, sourceNode)
		{
		}

		public IDependencyDefinition<TBaseType> DependsOn(TBaseType element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			Graph.SetupDependency(Element, element);
			return this;
		}

		IDependencyDefinition<TBaseType> IDependencyDefinition<TBaseType>.AndOn(TBaseType element) => DependsOn(element);
	}
}
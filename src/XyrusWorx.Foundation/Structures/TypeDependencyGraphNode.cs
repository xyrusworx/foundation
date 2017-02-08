using System;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public sealed class TypeDependencyGraphNode<TBaseType> : DependencyGraphNode<Type>, ITypeDependencyDefinition<TBaseType> where TBaseType : class
	{
		internal TypeDependencyGraphNode([NotNull] TypeDependencyGraph<TBaseType> graph, [NotNull] Type sourceType) : base(graph, sourceType)
		{
		}

		public ITypeDependencyDefinition<TBaseType> DependsOn(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (!Element.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
			{
				throw new ArgumentException($"A type which is implicitly convertible to \"{Element.FullName}\" is required.", nameof(type));
			}

			Graph.SetupDependency(Element, type);
			return this;
		}
		public ITypeDependencyDefinition<TBaseType> DependsOn<T>() where T : class, TBaseType => DependsOn(typeof(T));

		ITypeDependencyDefinition<TBaseType> ITypeDependencyDefinition<TBaseType>.AndOn(Type type) => DependsOn(type);
		ITypeDependencyDefinition<TBaseType> ITypeDependencyDefinition<TBaseType>.AndOn<T>() => DependsOn<T>();
	}
}
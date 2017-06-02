using System;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public class TypeDependencyGraph : TypeDependencyGraph<object>
	{
	}

	[PublicAPI]
	public class TypeDependencyGraph<TBaseType> : DependencyGraph<Type, TypeDependencyGraphNode<TBaseType>> where TBaseType : class
	{
		public void Register<T>() where T : class, TBaseType => Register(typeof(T));

		public void SetupDependency<TFrom>([NotNull] Type to) where TFrom : class, TBaseType => SetupDependency(typeof(TFrom), to);
		public void SetupDependency<TFrom, TTo>() where TFrom : class, TBaseType where TTo : class, TBaseType => SetupDependency(typeof(TFrom), typeof(TTo));

		protected internal sealed override bool AreEqual(Type from, Type to)
		{
			return from == to;
		}

		protected sealed override TypeDependencyGraphNode<TBaseType> CreateNode(Type element)
		{
			return new TypeDependencyGraphNode<TBaseType>(this, element);
		}
		protected sealed override void VerifyNode(Type node)
		{
			if (!typeof(TBaseType).GetTypeInfo().IsAssignableFrom(node.GetTypeInfo()))
			{
				throw new ArgumentException($"The type \"{node.FullName}\" must be implicitly convertible to \"{typeof(TBaseType).FullName}\".");
			}
		}
	}
}
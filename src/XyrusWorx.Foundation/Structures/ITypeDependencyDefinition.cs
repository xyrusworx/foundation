using System;
using JetBrains.Annotations;

namespace XyrusWorx.Structures
{
	[PublicAPI]
	public interface ITypeDependencyDefinition<in TBaseType> where TBaseType : class
	{
		[NotNull]
		ITypeDependencyDefinition<TBaseType> AndOn([NotNull] Type type);

		[NotNull]
		ITypeDependencyDefinition<TBaseType> AndOn<T>() where T : class, TBaseType;
	}
}
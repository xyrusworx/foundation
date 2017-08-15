using System;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public interface IServiceLocator
	{
		MissingDependencyBehavior MissingDependencyBehavior { get; set; }

		void Clear();

		void Register([NotNull] Type type);
		void Register([NotNull] Type interfaceType, [NotNull] Type implementationType);
		void Register([NotNull] Type type, [NotNull] object instance);

		void RegisterSingleton([NotNull] Type type);
		void RegisterSingleton([NotNull] Type interfaceType, [NotNull] Type implementationType);

		[NotNull]
		object Resolve([NotNull] Type type);

		[NotNull]
		object CreateInstance([NotNull] Type type);
	}
}
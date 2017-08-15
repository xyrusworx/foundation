using System;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public static class ServiceLocatorExtensions
	{
		public static void Register<T>([NotNull] this IServiceLocator serviceLocator) => serviceLocator.Register(typeof(T));
		public static void Register<TInterface, TImplementation>([NotNull] this IServiceLocator serviceLocator) => serviceLocator.Register(typeof(TInterface), typeof(TImplementation));
		public static void Register<T>([NotNull] this IServiceLocator serviceLocator, [NotNull] T instance) => serviceLocator.Register(typeof(T), instance);

		public static void RegisterSingleton<T>([NotNull] this IServiceLocator serviceLocator) => serviceLocator.RegisterSingleton(typeof(T));
		public static void RegisterSingleton<TInterface, TImplementation>([NotNull] this IServiceLocator serviceLocator) => serviceLocator.RegisterSingleton(typeof(TInterface), typeof(TImplementation));

		[NotNull]
		public static T Resolve<T>([NotNull] this IServiceLocator serviceLocator) => (T)serviceLocator.Resolve(typeof(T));

		[NotNull]
		public static Result<T> TryResolve<T>([NotNull] this IServiceLocator serviceLocator)
		{
			try
			{
				return new Result<T>(serviceLocator.Resolve<T>());
			}
			catch (Exception exception)
			{
				return Result.CreateError(exception).Specialize<Result<T>>();
			}
		}

		[NotNull]
		public static Result<object> TryResolve([NotNull] this IServiceLocator serviceLocator, [NotNull] Type type)
		{
			try
			{
				return new Result<object>(serviceLocator.Resolve(type));
			}
			catch (Exception exception)
			{
				return Result.CreateError(exception).Specialize<Result<object>>();
			}
		}

		[NotNull]
		public static Result<object> TryCreateInstance([NotNull] this IServiceLocator serviceLocator, [NotNull] Type type)
		{
			try
			{
				return new Result<object>(serviceLocator.CreateInstance(type));
			}
			catch (Exception exception)
			{
				return Result.CreateError(exception).Specialize<Result<object>>();
			}
		}

		[NotNull]
		public static T CreateInstance<T>([NotNull] this IServiceLocator serviceLocator) => (T)serviceLocator.CreateInstance(typeof(T));
	}
}
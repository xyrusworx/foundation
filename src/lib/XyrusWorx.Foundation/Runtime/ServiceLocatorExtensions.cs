using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.Collections;

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

		public static void AutoRegister([NotNull] this IServiceLocator serviceLocator, [NotNull] Assembly assembly, [NotNull] Type baseType, params Type[] additionalBaseTypes)
			=> AutoRegister(serviceLocator, assembly, new[]{baseType}.Concat(additionalBaseTypes ?? new Type[0]));

		public static void AutoRegister([NotNull] this IServiceLocator serviceLocator, [NotNull] Assembly assembly, [NotNull] IEnumerable<Type> baseTypes)
		{
			if (serviceLocator == null)
			{
				throw new ArgumentNullException(nameof(serviceLocator));
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (baseTypes == null)
			{
				throw new ArgumentNullException(nameof(baseTypes));
			}

			IEnumerable<Type> GetDirectInterfaces(Type type)
			{
				var allInterfaces = new List<Type>();
				var childInterfaces = new List<Type>();

				if (type.BaseType != null)
				{
					childInterfaces.AddRange(type.BaseType.GetInterfaces());
				}

				foreach (var i in type.GetInterfaces())
				{
					allInterfaces.Add(i);
					childInterfaces.AddRange(i.GetInterfaces());
				}

				return allInterfaces.Except(childInterfaces);
			}

			var templates =
				from type in assembly.GetLoadableTypes()

				where !type.IsAbstract && !type.IsInterface
				where baseTypes.Any(x => x.IsAssignableFrom(type))

				let isSingleton = type.GetCustomAttribute<SingletonAttribute>() != null
				let interfaceList = GetDirectInterfaces(type).ToArray()
				let interfaceAttribute = type.GetCustomAttribute<ServiceInterfaceAttribute>()

				let significantInterface =
					interfaceAttribute != null ? interfaceAttribute.InterfaceType : 
					interfaceList.Length == 1 ? interfaceList[0] : 
					null

				select new
				{
					Type = type,
					IsSingleton = isSingleton,
					Interface = significantInterface ?? type
				};

			foreach (var template in templates)
			{
				if (template.IsSingleton)
				{
					serviceLocator.RegisterSingleton(template.Interface, template.Type);
				}
				else
				{
					serviceLocator.Register(template.Interface, template.Type);
				}
			}
		}
	}

}
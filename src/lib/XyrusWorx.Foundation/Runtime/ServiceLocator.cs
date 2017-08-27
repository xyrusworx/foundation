using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public class ServiceLocator : IServiceLocator
	{
		private static Lazy<ServiceLocator> mDefault;
		private Dictionary<ServiceHandle, ServiceHandle> mServices;

		public ServiceLocator()
		{
			mServices = new Dictionary<ServiceHandle, ServiceHandle>();
		}
		static ServiceLocator()
		{
			mDefault = new Lazy<ServiceLocator>(() => new ServiceLocator());
		}

		public MissingDependencyBehavior MissingDependencyBehavior { get; set; }

		[NotNull]
		public static ServiceLocator Default => mDefault.Value;

		public void Clear()
		{
			mServices.Clear();
		}

		public void Register(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var info = type.GetTypeInfo();

			if (info.IsAbstract || info.IsInterface)
			{
				throw new ArgumentException($"The provided type \"{type.FullName}\" is an interface or abstract and can't be directly constructed.");
			}

			var handle = new ServiceHandle(type);
			mServices.AddOrUpdate(handle, handle);
		}
		public void Register(Type interfaceType, Type implementationType)
		{
			if (interfaceType == null)
			{
				throw new ArgumentNullException(nameof(interfaceType));
			}

			if (implementationType == null)
			{
				throw new ArgumentNullException(nameof(implementationType));
			}

			var handle = new ServiceHandle(interfaceType, implementationType);
			mServices.AddOrUpdate(handle, handle);
		}
		public void Register(Type type, object instance)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			var handle = new ServiceHandle(type, instance);
			mServices.AddOrUpdate(handle, handle);
		}

		public void RegisterSingleton(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var instance = CreateInstance(type, null);
			var handle = new ServiceHandle(type, instance);

			mServices.AddOrUpdate(handle, handle);
		}
		public void RegisterSingleton(Type interfaceType, Type implementationType)
		{
			if (interfaceType == null)
			{
				throw new ArgumentNullException(nameof(interfaceType));
			}

			if (implementationType == null)
			{
				throw new ArgumentNullException(nameof(implementationType));
			}

			var instance = CreateInstance(implementationType, null);
			var handle = new ServiceHandle(interfaceType, instance);

			mServices.AddOrUpdate(handle, handle);
		}

		public object Resolve(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return Resolve(type, null);
		}

		public object CreateInstance(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type.GetTypeInfo().IsInterface || type.GetTypeInfo().IsAbstract)
			{
				throw new ArgumentException($"The type \"{type}\" is abstract or an interface.");
			}

			return CreateInstance(type, null);
		}

		private object Resolve(Type type, Type[] path)
		{
			if (path != null && path.Contains(type))
			{
				var builder = new StringBuilder();

				builder.AppendLine($"Resolving a service for type \"{type.FullName}\" failed because a cyclic dependency has been detected:");
				builder.AppendLine($"    --> {type.FullName}");

				foreach (var node in path)
				{
					builder.AppendLine($"    --> {node.FullName}");
				}

				throw new InvalidOperationException();
			}

			var handle = mServices.GetValueByKeyOrDefault(new ServiceHandle(type));
			if (handle == null)
			{
				var builder = new StringBuilder();
				if (path != null && path.Any())
				{
					if (MissingDependencyBehavior == MissingDependencyBehavior.ResolveNull)
					{
						return null;
					}

					builder.AppendLine($"Resolving a service for type \"{type.FullName}\" failed because a dependent service can't be resolved:");
					builder.AppendLine($"    --> {type.FullName}");

					foreach (var node in path)
					{
						builder.AppendLine($"    --> {node.FullName}");
					}
				}
				else
				{
					builder.Append($"Resolving a service for type \"{type.FullName}\" failed.");
				}

				throw new KeyNotFoundException(builder.ToString());
			}

			if (handle.HasInstance)
			{
				return handle.Instance;
			}

			return CreateInstance(handle.ConcreteType, path);
		}
		private object CreateInstance(Type type, Type[] path)
		{
			var typeInfo = type.GetTypeInfo();

			var suitableConstructor = typeInfo
				.DeclaredConstructors
				.OrderByDescending(x => x.GetParameters().Length)
				.ThenBy(x => x.IsPublic ? 0 : 1)
				.FirstOrDefault();

			if (suitableConstructor == null)
			{
				throw new MissingMethodException($"Resolving a service for type \"{type.FullName}\" failed because no suitable constructor could be found.");
			}

			var dependencies = new List<object>();

			foreach (var dependencyType in suitableConstructor.GetParameters().Select(x => x.ParameterType))
			{
				dependencies.Add(Resolve(dependencyType, (path ?? new Type[0]).Concat(new[] { type }).ToArray()));
			}

			try
			{
				var instance = suitableConstructor.Invoke(dependencies.Any()
					? dependencies.ToArray()
					: null);

				return instance;
			}
			catch (Exception exception)
			{
				throw new TargetInvocationException($"Resolving a service for type \"{type.FullName}\" failed.", exception);
			}
		}
	}
}
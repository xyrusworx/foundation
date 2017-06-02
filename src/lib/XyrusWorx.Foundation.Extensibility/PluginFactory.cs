using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public abstract class PluginFactory : Resource, IPluginFactory
	{
		public Result<object> CreateInstance<TInterface>(PluginInfo pluginInfo, IPluginHostContext context) where TInterface : class, IPlugin
		{
			if (pluginInfo == null) throw new ArgumentNullException(nameof(pluginInfo));
			if (context == null) throw new ArgumentNullException(nameof(context));

			return CreateInstance(pluginInfo, typeof(TInterface), context);
		}
		public Result<object> CreateInstance(PluginInfo pluginInfo, Type interfaceType, IPluginHostContext context)
		{
			if (pluginInfo == null) throw new ArgumentNullException(nameof(pluginInfo));
			if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));
			if (context == null) throw new ArgumentNullException(nameof(context));

			Assembly assembly;
			Type implementationType;
			object instance;

			try
			{
				assembly = LoadAssembly(pluginInfo.AssemblyLocation);
			}
			catch (Exception exception)
			{
				return Result.CreateError<Result<object>>(new Exception($"Failed load plugin assembly at \"{pluginInfo.AssemblyLocation}\": {exception.Message}", exception));
			}

			try
			{
				implementationType = GetPluginImplementationType(assembly, interfaceType);

				if (implementationType == null)
				{
					return Result.CreateError<Result<object>>(new Exception($"Failed load plugin assembly at \"{pluginInfo.AssemblyLocation}\": no suitable plugin implementation type fouund."));
				}
			}
			catch (Exception exception)
			{
				return Result.CreateError<Result<object>>(new Exception($"Failed locate plugin implementation type: {exception.Message}", exception));
			}

			try
			{
				instance = Activator.CreateInstance(implementationType, context);
			}
			catch (Exception exception)
			{
				return Result.CreateError<Result<object>>(new Exception($"Failed create plugin instance: {exception.Message}", exception));
			}

			return new Result<object>(instance);
		}

		public Result<PluginInfo> FindPlugin<TInterface>(FileSystemStore directory) where TInterface : class, IPlugin
		{
			if (directory == null)
			{
				throw new ArgumentNullException(nameof(directory));
			}

			return FindPlugin(directory, typeof(TInterface));
		}
		public Result<PluginInfo> FindPlugin(FileSystemStore directory, Type interfaceType)
		{
			if (directory == null)
			{
				throw new ArgumentNullException(nameof(directory));
			}

			if (interfaceType == null)
			{
				throw new ArgumentNullException(nameof(interfaceType));
			}

			var files = directory.Keys.Where(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToArray();

			var loadedTypes = new List<Type>();
			var loadingProblems = new List<string>();

			PluginInfo info = null;

			foreach (var file in files)
			{
				try
				{
					var fullName = directory.Identifier.Concat(new StringKey(file)).ToString();
					var assembly = LoadAssembly(fullName);
					var implementation = GetPluginImplementationType(assembly, interfaceType);

					if (implementation != null)
					{
						var instance = (IPlugin)Activator.CreateInstance(implementation, new NullHostContext());

						info = new PluginInfo
						{
							AssemblyLocation = fullName,
							DisplayName = instance.DisplayName,
							TypeName = instance.GetType().Name,
							TypeFullName = instance.GetType().FullName,
							Id = instance.Id,
							Version = instance.Version
						};

						break;
					}
				}
				catch (ReflectionTypeLoadException rtle)
				{
					loadedTypes.AddRange(rtle.Types.Where(x => x != null));
					loadingProblems.AddRange(rtle.LoaderExceptions.Select(x => $"Error loading \"{file}\": {x.Message}"));
				}
				catch (Exception exception)
				{
					loadingProblems.Add($"Loading {file}: {exception.Message}");
				}
			}

			if (info == null)
			{
				if (!files.Any())
				{
					return Result.CreateError<Result<PluginInfo>>("The specified directory doesn't contain any assemblies.");
				}

				var diagnosticsStringBuilder = new StringBuilder();
				var counter = 0;

				diagnosticsStringBuilder.AppendLine("The specified directory didn't contain any valid plugin assembly or the plugin assembly failed to load.");
				diagnosticsStringBuilder.AppendLine("Processed assemblies:");

				foreach (var file in files)
				{
					diagnosticsStringBuilder.AppendLine($"  {++counter}: {Path.GetFileName(file)}");
				}

				counter = 0;
				diagnosticsStringBuilder.AppendLine("Discovered types:");
				foreach (var type in loadedTypes)
				{
					diagnosticsStringBuilder.AppendLine($"  {++counter}: {type.FullName}");
				}

				counter = 0;
				diagnosticsStringBuilder.AppendLine("Registered loading exceptions:");
				foreach (var exception in loadingProblems)
				{
					diagnosticsStringBuilder.AppendLine($"  {++counter}: {exception}");
				}

				return Result.CreateError<Result<PluginInfo>>(diagnosticsStringBuilder.ToString());
			}

			return new Result<PluginInfo> { Data = info };
		}

		public Type GetPluginImplementationType<TInterface>(Assembly pluginAssembly) where TInterface : class, IPlugin
		{
			if (pluginAssembly == null)
			{
				throw new ArgumentNullException(nameof(pluginAssembly));
			}

			return GetPluginImplementationType(pluginAssembly, typeof(TInterface));
		}
		public Type GetPluginImplementationType(Assembly pluginAssembly, Type interfaceType)
		{
			if (pluginAssembly == null)
			{
				throw new ArgumentNullException(nameof(pluginAssembly));
			}

			if (interfaceType == null)
			{
				throw new ArgumentNullException(nameof(interfaceType));
			}

			var implementationType = pluginAssembly.GetLoadableTypes().FirstOrDefault(x => IsPlugin(x, interfaceType));

			return implementationType;
		}

		public bool IsPlugin<TInterface>(Type typeToCheck) where TInterface : class, IPlugin
		{
			if (typeToCheck == null)
			{
				throw new ArgumentNullException(nameof(typeToCheck));
			}

			return IsPlugin(typeToCheck, typeof(TInterface));
		}
		public bool IsPlugin(Type typeToCheck, Type interfaceType)
		{
			if (typeToCheck == null)
			{
				throw new ArgumentNullException(nameof(typeToCheck));
			}

			if (interfaceType == null)
			{
				throw new ArgumentNullException(nameof(interfaceType));
			}

			var pluginInterface = typeToCheck.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(x => x.FullName == interfaceType.FullName);
			if (pluginInterface == null)
			{
				return false;
			}

			var constructors = typeToCheck.GetTypeInfo().DeclaredConstructors;
			var hasMatchingConstructor = false;

			foreach (var constructor in constructors)
			{
				var parameters = constructor.GetParameters();
				if (parameters.Length != 1)
				{
					continue;
				}

				var parameter = parameters.First();
				if (parameter.ParameterType.FullName != typeof(IPluginHostContext).FullName)
				{
					continue;
				}

				hasMatchingConstructor = true;
			}

			return hasMatchingConstructor;
		}

		[NotNull]
		protected abstract Assembly LoadAssembly(string assemblyLocation);
	}
}
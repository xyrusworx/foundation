using System;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public interface IPluginFactory : IDisposable
	{
		[NotNull] Result<object> CreateInstance<TInterface>([NotNull] PluginInfo pluginInfo, IPluginHostContext context) where TInterface : class, IPlugin;
		[NotNull] Result<object> CreateInstance([NotNull] PluginInfo pluginInfo, Type interfaceType, IPluginHostContext context);

		[NotNull] Result<PluginInfo> FindPlugin<TInterface>([NotNull] FileSystemStore directory) where TInterface : class, IPlugin;
		[NotNull] Result<PluginInfo> FindPlugin([NotNull] FileSystemStore directory, [NotNull] Type interfaceType);

		[CanBeNull] Type GetPluginImplementationType<TInterface>([NotNull] Assembly pluginAssembly) where TInterface : class, IPlugin;
		[CanBeNull] Type GetPluginImplementationType([NotNull] Assembly pluginAssembly, [NotNull] Type interfaceType);

		bool IsPlugin<TInterface>([NotNull] Type typeToCheck) where TInterface : class, IPlugin;
		bool IsPlugin([NotNull] Type typeToCheck, [NotNull] Type interfaceType);
	}
}
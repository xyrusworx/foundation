using System;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public interface IPlugin
	{
		Guid Id { get; }

		[NotNull]
		string DisplayName { get; }
		string Version { get; }

		[CanBeNull]
		IKeyValueStore Configuration { get; }

		void Initialize([NotNull] PluginInitializationArgs args);
		void Shutdown([NotNull] PluginShutdownArgs args);
	}
}
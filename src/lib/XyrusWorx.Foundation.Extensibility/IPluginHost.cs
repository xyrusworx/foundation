using System;
using JetBrains.Annotations;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public interface IPluginHost
	{
		Guid Id { get; }

		[NotNull]
		string DisplayName { get; }

		[NotNull]
		string SafeName { get; }

		[NotNull]
		string Version { get; }

		bool IsInitialized { get; }

		[NotNull]
		Result Initialize();
		void Shutdown();
	}
}
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public class PluginInitializationArgs
	{
		public string HostVersion { get; set; }
		public ILogWriter HostLog { get; set; }

		public IKeyValueStore InitializationArgs { get; } = new MemoryKeyValueStore();
	}
}

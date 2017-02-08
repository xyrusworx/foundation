using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public class NullHostContext : IPluginHostContext
	{
		public IBlobStore DataStorage { get; } = new NullStorage();
		public IBlobStore DiagnosticsStorage { get; } = new NullStorage();
	}
}
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public interface IPluginHostContext
	{
		IBlobStore DataStorage { get; }
		IBlobStore DiagnosticsStorage { get; }
	}
}
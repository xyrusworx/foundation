using System;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	class PluginHostContext : IPluginHostContext
	{
		private readonly IBlobStore mPluginRoot;

		public PluginHostContext([NotNull] IBlobStore pluginRoot)
		{
			if (pluginRoot == null)
			{
				throw new ArgumentNullException(nameof(pluginRoot));
			}

			mPluginRoot = pluginRoot;
		}

		public IBlobStore DataStorage => mPluginRoot.GetChildStore("data".AsKey(), false);
		public IBlobStore DiagnosticsStorage => mPluginRoot.GetChildStore("diagnostics".AsKey(), false);
	}
}
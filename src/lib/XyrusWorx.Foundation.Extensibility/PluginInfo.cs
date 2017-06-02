using System;
using JetBrains.Annotations;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public class PluginInfo
	{
		public Guid Id { get; set; }
		public string DisplayName { get; set; }
		public string TypeName { get; set; }
		public string TypeFullName { get; set; }
		public string AssemblyLocation { get; set; }
		public string Version { get; set; }
	}
}
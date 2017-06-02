using JetBrains.Annotations;
using System;
using System.IO;
using System.Linq;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public class ApplicationExecutionContext
	{
		internal ApplicationExecutionContext()
		{
			
		}

		internal StringKey GetMachineDataDirectoryName(params string[] @namespace)
		{
			var appData = Path.GetFullPath(Path.Combine(Environment.ExpandEnvironmentVariables("%programdata%")));
			var fullNamespace = new[] { appData }.Concat(@namespace).ToArray();
			var path = Path.Combine(fullNamespace);

			return new StringKey(path);
		}
		internal StringKey GetUserDataDirectoryName(params string[] @namespace)
		{
			var appData = Path.GetFullPath(Path.Combine(Environment.ExpandEnvironmentVariables("%appdata%"), ".."));
			var fullNamespace = new[] { appData }.Concat(@namespace).ToArray();
			var path = Path.Combine(fullNamespace);

			return new StringKey(path);
		}
	}
}
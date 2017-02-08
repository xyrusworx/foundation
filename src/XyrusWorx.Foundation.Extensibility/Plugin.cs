using System;
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public abstract class Plugin : IPlugin
	{
		protected Plugin([NotNull] IPluginHostContext hostContext)
		{
			if (hostContext == null)
			{
				throw new ArgumentNullException(nameof(hostContext));
			}

			Context = hostContext;
		}

		public abstract Guid Id { get; }
		public abstract string DisplayName { get; }
		public abstract string Version { get; }

		public virtual IKeyValueStore Configuration => null;

		public ILogWriter Log { get; private set; }
		public IPluginHostContext Context { get; private set; }

		public void Initialize(PluginInitializationArgs args)
		{
			if (args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}

			Log = args.HostLog ?? new NullLogWriter();
			Log?.WriteVerbose(
				"Plugin is initializing:\r\n" +
				$"    Id:      {Id}\r\n" +
				$"    Type:    {GetType().Name}\r\n" +
				$"    Name:    {DisplayName}\r\n" +
				$"    Version: {Version.NormalizeNull()??"(unknown)"}");

			OnInitialize(args);
		}
		public void Shutdown(PluginShutdownArgs args)
		{
			if (args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}

			OnShutdown(args);

			Context = null;
			Log = null;
		}

		protected virtual void OnInitialize([NotNull] PluginInitializationArgs args) { }
		protected virtual void OnShutdown([NotNull] PluginShutdownArgs args) { }
	}
}
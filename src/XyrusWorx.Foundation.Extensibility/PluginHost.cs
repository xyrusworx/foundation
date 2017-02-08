using System;
using System.IO;
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;
using XyrusWorx.Runtime;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public abstract class PluginHost<TInterface> : Resource, IPlugin, IPluginHost where TInterface : class, IPlugin
	{
		[CanBeNull]
		private readonly Application mHostApplication;
		private readonly PluginInfo mPluginInfo;

		[CanBeNull]
		private readonly ILogWriter mLog;
		private readonly IPluginHostContext mContext;

		[CanBeNull]
		private object mInstance;

		protected PluginHost([NotNull] PluginInfo plugin, [CanBeNull] ILogWriter log = null)
		{
			if (plugin == null)
			{
				throw new ArgumentNullException(nameof(plugin));
			}

			mPluginInfo = plugin;
			mContext = new PluginHostContext(new FileSystemStore((Path.GetDirectoryName(plugin.AssemblyLocation)??Directory.GetCurrentDirectory()).AsKey(), isReadOnly: true));
			mLog = log;
		}
		protected PluginHost([NotNull] PluginInfo plugin, [NotNull] Application hostApplication)
		{
			if (plugin == null)
			{
				throw new ArgumentNullException(nameof(plugin));
			}

			if (hostApplication == null)
			{
				throw new ArgumentNullException(nameof(hostApplication));
			}

			if (string.IsNullOrWhiteSpace(plugin.AssemblyLocation))
			{
				throw new ArgumentException("The plugin doesn't contain a valid assembly location.", nameof(plugin));
			}

			if (!File.Exists(plugin.AssemblyLocation))
			{
				throw new FileNotFoundException("The assembly specified in the plugin info can't be found.", plugin.AssemblyLocation);
			}

			mHostApplication = hostApplication;
			mPluginInfo = plugin;

			var directory = Path.GetDirectoryName(plugin.AssemblyLocation) ?? Directory.GetCurrentDirectory();

			mContext = new PluginHostContext(new FileSystemStore(directory.AsKey(), isReadOnly:true));
			mLog = hostApplication.Log;
		}

		public Guid Id => mPluginInfo.Id;
		public string SafeName => mPluginInfo.TypeName;
		public string DisplayName => mPluginInfo.DisplayName;
		public string Version => mPluginInfo.Version;
		public TInterface Instance
		{
			get
			{
				var instance = GetCurrentInstance().UnboxTo<TInterface>();
				
				return instance;
			}
		}

		public IKeyValueStore InitializationParameters { get; } = new MemoryKeyValueStore();
		public IKeyValueStore Configuration => GetCurrentInstance().CastTo<IPlugin>()?.Configuration;

		public bool IsInitialized => mInstance != null;

		public Result Initialize() 
		{
			if (mInstance != null)
			{
				Shutdown();
			}

			if (string.IsNullOrWhiteSpace(mPluginInfo.AssemblyLocation))
			{
				throw new FileLoadException("The plugin doesn't contain a valid assembly location.");
			}

			if (!File.Exists(mPluginInfo.AssemblyLocation))
			{
				throw new FileNotFoundException("The assembly specified in the plugin info can't be found.", mPluginInfo.AssemblyLocation);
			}

			using (var factory = GetPluginFactory())
			{
				var createInstanceResponse = factory.CreateInstance<TInterface>(mPluginInfo, mContext);
				if (createInstanceResponse.HasError)
				{
					return createInstanceResponse;
				}

				if (createInstanceResponse.Data == null)
				{
					return Result.CreateError("Unspecified error while creating plugin instance.");
				}

				var args = new PluginInitializationArgs
				{
					HostVersion = mHostApplication?.Metadata.ProductVersion,
					HostLog = mLog
				};

				foreach (var key in InitializationParameters.GetKeys())
				{
					args.InitializationArgs.Write(key, InitializationParameters.Read(key));
				}

				try
				{
					mInstance = createInstanceResponse.Data;
					((IPlugin)mInstance).Initialize(args);
				}
				catch (Exception exception)
				{
					WriteFault(exception);
				}
			}

			return Result.Success;
		}
		public void Shutdown()
		{
			if (mInstance == null)
			{
				return;
			}

			try
			{
				((IPlugin)mInstance).Shutdown(new PluginShutdownArgs());
			}
			catch (Exception exception)
			{
				WriteFault(exception);
			}

			mInstance = null;
		}

		protected object GetCurrentInstance() => mInstance;

		protected abstract IPluginFactory GetPluginFactory();

		protected sealed override void DisposeOverride()
		{
			Shutdown();
			CleanupOverride();
		}
		protected virtual void CleanupOverride() { }

		private void WriteFault(Exception exception)
		{
			using (var writer = mContext.DiagnosticsStorage.Open($"fault_{DateTime.Today:yy-MM-dd}_{DateTime.Now:hh.mm.ss}.txt".AsKey()).AsText().Write())
			{
				writer.Write($"The following information have been recorded:\r\n{exception.Message}\r\n{exception.StackTrace}");
			}

			mLog?.Write(exception);
		}

		void IPlugin.Initialize(PluginInitializationArgs args)
		{
			Initialize();
		}
		void IPlugin.Shutdown(PluginShutdownArgs args)
		{
			Shutdown();
		}
	}
}
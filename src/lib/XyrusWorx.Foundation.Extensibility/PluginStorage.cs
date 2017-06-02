using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using XyrusWorx.Collections;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;

namespace XyrusWorx.Extensibility
{
	[PublicAPI]
	public abstract class PluginStorage
	{
		private readonly FileSystemStore mStorage;

		protected PluginStorage([NotNull] string directoryName)
		{
			if (directoryName == null)
			{
				throw new ArgumentNullException(nameof(directoryName));
			}

			mStorage = new FileSystemStore(directoryName.AsKey(), isReadOnly: true);
		}
		protected PluginStorage([NotNull] FileSystemStore storage)
		{
			if (storage == null)
			{
				throw new ArgumentNullException(nameof(storage));
			}

			mStorage = storage;
		}

		[NotNull] public IDictionary<Guid, PluginInfo> Discover<TPlugin>(ILogWriter log = null) where TPlugin : class, IPlugin
		{
			return Discover(typeof(TPlugin), log);
		}
		[NotNull] public IDictionary<Guid, PluginInfo> Discover([NotNull] Type interfaceType, ILogWriter log = null)
		{
			if (interfaceType == null)
			{
				throw new ArgumentNullException(nameof(interfaceType));
			}

			var result = new Dictionary<Guid, PluginInfo>();

			foreach (var childStorageKey in mStorage.GetChildStoreKeys())
			{
				var childStorage = mStorage.GetChildStore(childStorageKey);

				using (new Scope(OpenFactoryScope, CloseFactoryScope).Enter())
				using (var factory = GetPluginFactory())
				{
					log?.WriteDebug("Using plugin factory: {0}", factory.GetType().FullName);

					var loadResult = factory.FindPlugin(childStorage.CastTo<FileSystemStore>().AssertNotNull(), interfaceType);
					if (loadResult.HasError)
					{
						log?.WriteWarning(loadResult.ErrorDescription ?? string.Empty);
						continue;
					}

					log?.WriteDebug("Discovery successful: {0} / {1}", loadResult.Data.Id, loadResult.Data.TypeFullName);
					result.AddOrUpdate(loadResult.Data.Id, loadResult.Data);
				}
			}

			return result;
		}

		[NotNull] public Result<PluginInfo> Load<TPlugin>(StringKey pluginAssemblyStorageKey, ILogWriter log = null) where TPlugin : class, IPlugin
		{
			return Load(pluginAssemblyStorageKey, typeof(TPlugin), log);
		}
		[NotNull] public Result<PluginInfo> Load(StringKey pluginAssemblyStorageKey, [NotNull] Type interfaceType, ILogWriter log = null)
		{
			if (interfaceType == null)
			{
				throw new ArgumentNullException(nameof(interfaceType));
			}

			if (!mStorage.HasChildStore(pluginAssemblyStorageKey))
			{
				return Result.CreateError<Result<PluginInfo>>(new DirectoryNotFoundException());
			}

			using (new Scope(OpenFactoryScope, CloseFactoryScope).Enter())
			using (var factory = GetPluginFactory())
			{
				log?.WriteDebug("Using plugin factory: {0}", factory.GetType().FullName);

				var storage = (FileSystemStore)mStorage.GetChildStore(pluginAssemblyStorageKey);
				var loadResult = factory.FindPlugin(storage, interfaceType);

				if (loadResult.HasError)
				{
					log?.WriteDebug($"Error: {loadResult.ErrorDescription}");
				}
				else
				{
					log?.WriteDebug("Discovery successful: {0} / {1}", loadResult.Data.Id, loadResult.Data.TypeFullName);
				}

				return loadResult;
			}
		}

		protected abstract void OpenFactoryScope();
		protected abstract void CloseFactoryScope();

		protected abstract IPluginFactory GetPluginFactory();
	}
}
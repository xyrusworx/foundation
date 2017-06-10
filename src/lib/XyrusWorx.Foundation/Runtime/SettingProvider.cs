using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using XyrusWorx.Collections;
using XyrusWorx.IO;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public abstract class SettingProvider
	{
		private readonly IBlobStore mStore;
		private readonly Dictionary<StringKey, IKeyValueStore<string>> mSettingGroups;

		protected SettingProvider([NotNull] IBlobStore store)
		{
			if (store == null)
			{
				throw new ArgumentNullException(nameof(store));
			}

			mStore = store;
			mSettingGroups = new Dictionary<StringKey, IKeyValueStore<string>>();
		}

		public string ReadSetting(StringKey settingKey, string defaultValue = null) => ReadSetting(DefaultSettingGroupKey, settingKey, defaultValue);
		public string ReadSetting(StringKey settingGroupKey, StringKey settingKey, string defaultValue = null)
		{
			var settingGroup = LoadSettingGroup(settingGroupKey);
			var settingValue = settingGroup.Read(settingKey);

			return settingValue.NormalizeNull() ?? defaultValue;
		}

		public T ReadSetting<T>(StringKey settingKey, T? defaultValue = null) where T : struct => ReadSetting(DefaultSettingGroupKey, settingKey, defaultValue);
		public T ReadSetting<T>(StringKey settingGroupKey, StringKey settingKey, T? defaultValue = null) where T : struct
		{
			var stringValue = ReadSetting(settingGroupKey, settingKey);

			if (string.IsNullOrEmpty(stringValue))
			{
				return defaultValue ?? default(T);
			}

			if (!stringValue.TryDeserialize(typeof(T), out var typedValue, CultureInfo.InvariantCulture))
			{
				return defaultValue ?? default(T);
			}

			return (T)typedValue;
		}

		[NotNull]
		protected abstract IKeyValueStore<string> CreateKeyValueStore([NotNull] IBlobStore blobStore, StringKey baseName);

		protected const string DefaultSettingGroupKey = "Common";
		protected IKeyValueStore<string> LoadSettingGroup(StringKey settingGroupKey)
		{
			if (mSettingGroups.ContainsKey(settingGroupKey.Normalize()))
			{
				return mSettingGroups[settingGroupKey.Normalize()];
			}

			var settingGroupBlobStore = mStore
				.GetChildStore("Settings", false);

			var settingGroup = CreateKeyValueStore(settingGroupBlobStore, settingGroupKey);

			mSettingGroups.AddOrUpdate(settingGroupKey.Normalize(), settingGroup);

			return settingGroup;
		}
	}
}
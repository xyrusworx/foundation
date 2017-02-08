using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public class ObjectView : KeyValueStore<object>
	{
		private readonly object mInstance;
		private readonly Dictionary<StringKey, PropertyInfo> mProperties;
		private readonly HashSet<StringKey> mNames;

		public ObjectView(object obj)
		{
			mInstance = obj;

			mNames = new HashSet<StringKey>();
			mProperties = new Dictionary<StringKey, PropertyInfo>();

			foreach (var property in GetProperties(obj))
			{
				mNames.Add(property.Name);
				mProperties.AddOrUpdate(property.Name.AsKey().Normalize(), property);
			}
		}

		public override bool Exists(StringKey key)
		{
			return mProperties.ContainsKey(key.Normalize());
		}

		protected override object GetValue(StringKey key)
		{
			if (mInstance == null)
			{
				return null;
			}

			return mProperties.GetValueByKeyOrDefault(key.Normalize())?.GetValue(mInstance);
		}
		protected override void SetValue(StringKey key, object value)
		{
			throw new NotSupportedException();
		}
		protected override IEnumerable<StringKey> Enumerate()
		{
			if (mInstance == null)
			{
				yield break;
			}

			foreach (var name in mNames)
			{
				yield return name;
			}
		}

		private IEnumerable<PropertyInfo> GetProperties(object o)
		{
			if (o == null)
			{
				yield break;
			}

			foreach (var property in o.GetType().GetTypeInfo().DeclaredProperties)
			{
				yield return property;
			}
		}
	}
}
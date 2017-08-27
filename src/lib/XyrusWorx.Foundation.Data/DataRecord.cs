using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.Collections;
using XyrusWorx.IO;

namespace XyrusWorx.Data
{
	[PublicAPI]
	public class DataRecord : DynamicObject, IKeyValueStore
	{
		private readonly object mLock = new object();
		private readonly Dictionary<StringKey, int> mColumnIndices;
		private readonly List<string> mColumnNames;
		private readonly List<object> mColumnValues;
		private readonly List<bool> mIsNull;

		private DataRecord()
		{
			mColumnIndices = new Dictionary<StringKey, int>();
			mColumnNames = new List<string>();
			mColumnValues = new List<object>();
			mIsNull = new List<bool>();
		}

		public DataRecord([NotNull] object obj) : this()
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			var properties = obj
				.GetType()
				.GetTypeInfo()
				.DeclaredProperties
				.Where(x => x.CanRead && x.GetMethod.IsPublic)
				.ToArray();

			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];
				var value = property.GetValue(obj);

				mColumnIndices.AddOrUpdate(property.Name.AsKey().Normalize(), i);
				mColumnNames.Add(property.Name);
				mColumnValues.Add(value);
				mIsNull.Add(value == null);
			}
		}
		public DataRecord([NotNull] IDataRecord nativeRecord) : this()
		{
			if (nativeRecord == null)
			{
				throw new ArgumentNullException(nameof(nativeRecord));
			}

			for (var i = 0; i < nativeRecord.FieldCount; i++)
			{
				mColumnIndices.AddOrUpdate(nativeRecord.GetName(i).AsKey().Normalize(), i);
				mColumnNames.Add(nativeRecord.GetName(i));
				mColumnValues.Add(nativeRecord.GetValue(i));
				mIsNull.Add(nativeRecord.IsDBNull(i));
			}
		}
		public DataRecord([NotNull] IDictionary<string, object> dictionary) : this()
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException(nameof(dictionary));
			}

			var keyArray = dictionary.Keys.ToArray();

			for (var i = 0; i < keyArray.Length; i++)
			{
				mColumnIndices.AddOrUpdate(keyArray[i].AsKey().Normalize(), i);
				mColumnNames.Add(keyArray[i]);
				mColumnValues.Add(dictionary[keyArray[i]]);
				mIsNull.Add(dictionary[keyArray[i]] == null);
			}
		}

		public IReadOnlyList<string> Columns => mColumnNames;
		public IReadOnlyList<object> Values => mColumnValues;

		public int RowIndex { get; set; }
		public bool ThrowOnTypeMismatch { get; set; } = true;

		public bool IsDbNull(int columnIndex) => mIsNull[GetColumnHandle(columnIndex)];
		public bool IsDbNull(string columnName) => mIsNull[GetColumnHandle(columnName)];

		public object GetValue(int columnIndex)
		{
			var handle = GetColumnHandle(columnIndex);
			if (mIsNull[handle])
			{
				return null;
			}

			return mColumnValues[handle];
		}
		public object GetValue(string columnName)
		{
			var handle = GetColumnHandle(columnName);
			if (mIsNull[handle])
			{
				return null;
			}

			return mColumnValues[handle];
		}

		public T? GetValue<T>(int columnIndex) where T: struct
		{
			var i = GetColumnHandle(columnIndex);
			if (IsDbNull(i))
			{
				return null;
			}

			var v = GetValue(i);
			if (v is T)
			{
				return (T) v;
			}

			if (ThrowOnTypeMismatch)
			{
				throw new InvalidCastException($"The column \"{mColumnNames[i]}\" contains a value of type \"{v?.GetType()??typeof(object)}\" but a value of type \"{typeof(T)}\" was expected.");
			}

			return null;
		}
		public T? GetValue<T>(string columnName) where T: struct
		{
			return GetValue<T>(GetColumnHandle(columnName));
		}

		public string GetString(int columnIndex)
		{
			var i = GetColumnHandle(columnIndex);
			if (IsDbNull(i))
			{
				return null;
			}

			return GetValue(i)?.ToString();
		}
		public string GetString(string columnName)
		{
			return GetString(GetColumnHandle(columnName));
		}

		public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!mColumnIndices.ContainsKey(new StringKey(binder.Name).Normalize()))
			{
				result = null;
				return false;
			}

			result = GetValue(binder.Name);
			return true;
		}
		public sealed override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (indexes.Length != 1)
			{
				result = null;
				return false;
			}

			var integerIndex = indexes[0]?.ToString().TryDeserialize<int>(int.TryParse);
			var stringIndex = indexes[0]?.ToString();

			if (integerIndex.HasValue)
			{
				var i = integerIndex.Value;
				if (i < 0 || i >= mColumnIndices.Count)
				{
					result = null;
					return false;
				}

				result = GetValue(i);
				return true;
			}

			if (string.IsNullOrWhiteSpace(stringIndex))
			{
				result = null;
				return false;
			}

			if (!mColumnIndices.ContainsKey(new StringKey(stringIndex).Normalize()))
			{
				result = null;
				return false;
			}

			result = GetValue(stringIndex);
			return true;
		}

		private int GetColumnHandle(string columnName)
		{
			if (string.IsNullOrWhiteSpace(columnName))
			{
				throw new ArgumentNullException(nameof(columnName));
			}

			if (!mColumnIndices.ContainsKey(columnName.AsKey().Normalize()))
			{
				throw new KeyNotFoundException($"The column \"{columnName}\" was not found in the resulting schema of the query.");
			}

			return mColumnIndices[columnName.AsKey().Normalize()];
		}
		private int GetColumnHandle(int columnIndex)
		{
			if (columnIndex < 0 || columnIndex >= mColumnIndices.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}

			return columnIndex;
		}

		IEnumerable<StringKey> IKeyValueStore.GetKeys() => mColumnNames.Select(x => x.AsKey()).ToArray();

		bool IKeyValueStore.Exists(StringKey key) => mColumnIndices.ContainsKey(key.Normalize());
		object IKeyValueStore.Read(StringKey key) => GetValue(key.RawData);

		void IKeyValueStore.Write(StringKey key, object value)
		{
			throw new NotSupportedException();
		}
		void IKeyValueStore.SetDefault(StringKey key, object defaultValue)
		{
			throw new NotSupportedException();
		}
	}
}
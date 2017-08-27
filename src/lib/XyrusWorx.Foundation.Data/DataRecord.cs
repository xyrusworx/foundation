using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Data
{

	[PublicAPI]
	public class DataRecord : DynamicObject
	{
		private readonly object mLock = new object();
		private readonly Dictionary<StringKey, int> mColumnIndices;
		private readonly List<string> mColumnNames;
		private readonly List<object> mColumnValues;
		private readonly List<bool> mIsNull;
		private IFormatProvider mCulture = CultureInfo.CurrentCulture;

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
				.Where(x => x.CanRead && x.GetMethod.IsPublic && x.GetMethod.GetParameters().Length == 0)
				.ToArray();

			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];
				var value = property.GetValue(obj);

				mColumnIndices.AddOrUpdate(new StringKey(property.Name).Normalize(), i);
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
				mColumnIndices.AddOrUpdate(new StringKey(nativeRecord.GetName(i)).Normalize(), i);
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
				mColumnIndices.AddOrUpdate(new StringKey(keyArray[i]).Normalize(), i);
				mColumnNames.Add(keyArray[i]);
				mColumnValues.Add(dictionary[keyArray[i]]);
				mIsNull.Add(dictionary[keyArray[i]] == null);
			}
		}

		public IReadOnlyList<string> Columns => mColumnNames;
		public IReadOnlyList<object> Values => mColumnValues;

		public int RowIndex { get; internal set; }
		
		public TypeMismatchBehavior TypeMismatchBehavior { get; set; }
		public FieldNotFoundBehavior FieldNotFoundBehavior { get; set; }

		[NotNull]
		public IFormatProvider Culture
		{
			get => mCulture;
			set => mCulture = value ?? throw new ArgumentNullException(nameof(value));
		}

		public bool IsDbNull(int columnIndex)
		{
			var i = GetColumnHandle(columnIndex);
			if (i < 0)
			{
				return true;
			}
			
			return mIsNull[i];
		}
		public bool IsDbNull(string columnName)
		{
			var i = GetColumnHandle(columnName);
			if (i < 0)
			{
				return true;
			}
			
			return mIsNull[i];
		}

		public object GetValue(int columnIndex)
		{
			var handle = GetColumnHandle(columnIndex);
			if (handle < 0)
			{
				return null;
			}
			
			if (mIsNull[handle])
			{
				return null;
			}

			return mColumnValues[handle];
		}
		public object GetValue(string columnName)
		{
			var handle = GetColumnHandle(columnName);
			if (handle < 0)
			{
				return null;
			}
			
			if (mIsNull[handle])
			{
				return null;
			}

			return mColumnValues[handle];
		}

		public T? GetValue<T>(int columnIndex) where T : struct
		{
			var i = GetColumnHandle(columnIndex);
			if (i < 0)
			{
				return null;
			}
			
			if (IsDbNull(i))
			{
				return null;
			}

			var v = GetValue(i);
			if (v is T)
			{
				return (T)v;
			}

			switch (TypeMismatchBehavior)
			{
				case TypeMismatchBehavior.Convert:
					if ((v?.ToString()).TryDeserialize(typeof(T), out var rv, Culture))
					{
						return (T)rv;
					}
					return null;
				case TypeMismatchBehavior.ResultNull:
					return null;
				default:
					throw new InvalidCastException($"The field \"{mColumnNames[i]}\" contains a value of type \"{v?.GetType() ?? typeof(object)}\" but a value of type \"{typeof(T)}\" was required.");
			}
		}
		public T? GetValue<T>(string columnName) where T : struct
		{
			var i = GetColumnHandle(columnName);
			if (i < 0)
			{
				return null;
			}
			
			return GetValue<T>(i);
		}

		public string GetString(int columnIndex)
		{
			var i = GetColumnHandle(columnIndex);
			if (i < 0)
			{
				return null;
			}
			
			return !IsDbNull(i) 
				? GetValue(i)?.ToString() : 
				null;

		}
		public string GetString(string columnName)
		{
			var i = GetColumnHandle(columnName);
			if (i < 0)
			{
				return null;
			}
			
			return GetString(i);
		}

		public override IEnumerable<string> GetDynamicMemberNames() => mColumnNames.AsEnumerable();

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
			if (string.IsNullOrWhiteSpace(columnName) || !mColumnIndices.ContainsKey(new StringKey(columnName).Normalize()))
			{
				switch (FieldNotFoundBehavior)
				{
					case FieldNotFoundBehavior.ResultNull:
						return -1;
					default:
						throw new KeyNotFoundException($"The field \"{columnName}\" could not be located.");
				}
			}

			return mColumnIndices[new StringKey(columnName).Normalize()];
		}
		private int GetColumnHandle(int columnIndex)
		{
			if (columnIndex < 0 || columnIndex >= mColumnIndices.Count)
			{
				switch (FieldNotFoundBehavior)
				{
					case FieldNotFoundBehavior.ResultNull:
						return -1;
					default:
						throw new KeyNotFoundException($"The field at position {columnIndex + 1} could not be located.");
				}
			}

			return columnIndex;
		}
	}
}
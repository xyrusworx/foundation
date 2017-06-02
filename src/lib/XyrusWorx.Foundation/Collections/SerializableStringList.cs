using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.Collections
{
	[PublicAPI]
	public class SerializableStringList : IList<string>
	{
		private IList<string> mItems;

		public SerializableStringList()
		{
			mItems = new List<string>();
		}
		public SerializableStringList(string data) : this()
		{
			FromString(data);
		}
		public SerializableStringList(IEnumerable<string> items) : this()
		{
			mItems.Reset(items ?? new List<string>());
		}

		public char SeparatorChar { get; set; } = ',';
		public char EscapeChar { get; set; } = '\\';
		public char QuoteChar { get; set; } = '"';

		public void FromString(string data)
		{
			var isEscaped = false;
			var isQuoted = false;

			var sb = new StringBuilder();
			var str = data ?? string.Empty;

			Clear();

			// ReSharper disable once ForCanBeConvertedToForeach
			for (var i = 0; i < str.Length; i++)
			{
				if (str[i] == EscapeChar)
				{
					isEscaped = !isEscaped;
					if (!isEscaped)
					{
						sb.Append(EscapeChar);
					}
				}
				else if (str[i] == QuoteChar)
				{
					if (!isEscaped)
					{
						isQuoted = !isQuoted;
					}
					else
					{
						sb.Append(QuoteChar);
					}
				}
				else if (str[i] == SeparatorChar)
				{
					if (isQuoted || isEscaped)
					{
						sb.Append(SeparatorChar);
					}
					else
					{
						Add(sb.ToString());
						sb.Clear();
					}
				}
				else
				{
					sb.Append(str[i]);
				}
			}

			if (sb.Length > 0)
			{
				Add(sb.ToString());
			}
		}
		public override string ToString()
		{
			var separatorString = new string(SeparatorChar, 1);

			return string.Join(separatorString,
				from item in this
				select item.Replace(separatorString, $"\\{separatorString}"));
		}

		public void CopyTo(string[] array, int arrayIndex) => mItems.CopyTo(array, arrayIndex);
		public IEnumerator<string> GetEnumerator() => mItems.GetEnumerator();
		
		public bool Contains(string item) => mItems.Contains(item);
		public int IndexOf(string item) => mItems.IndexOf(item);

		public void Add(string item) => mItems.Add(item);
		public void Insert(int index, string item) => mItems.Insert(index, item);

		public void Clear() => mItems.Clear();
		public bool Remove(string item) => mItems.Remove(item);
		public void RemoveAt(int index) => mItems.RemoveAt(index);

		public int Count => mItems.Count;
		public string this[int index]
		{
			get { return mItems[index]; }
			set { mItems[index] = value; }
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		bool ICollection<string>.IsReadOnly => mItems.IsReadOnly;
	}
}
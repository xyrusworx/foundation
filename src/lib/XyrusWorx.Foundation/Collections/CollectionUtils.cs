using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace XyrusWorx.Collections
{
	[PublicAPI]
	public static class CollectionUtils
	{
		[NotNull]
		public static IList<T> AsList<T>([CanBeNull] this IEnumerable<T> instance)
		{
			if (instance == null)
			{
				return new List<T>();
			}

			var list = instance as IList<T>;
			if (list != null)
			{
				return list;
			}

			return instance.ToList();
		}

		[NotNull]
		public static T[] AsArray<T>([CanBeNull] this IEnumerable<T> instance)
		{
			if (instance == null)
			{
				return new T[0];
			}

			var array = instance as T[];
			if (array != null)
			{
				return array;
			}

			return instance.ToArray();
		}

		[NotNull]
		public static IDictionary<TKey, IList<TValue>> GroupToDictionary<TKey, TValue>(this IEnumerable<TValue> instance, [NotNull] Expression<Func<TValue, TKey>> keyGetter, IEqualityComparer<TKey> comparer = null)
		{
			if (keyGetter == null) 
			{
				throw new ArgumentNullException(nameof(keyGetter));
			}

			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			var result = new Dictionary<TKey, IList<TValue>>();
			var keyGetterFunc = keyGetter.Compile();

			comparer = comparer ?? new ExpressionComparer<TKey>(x => x);

			foreach (var item in instance)
			{
				var key = keyGetterFunc(item);
				Add(result, key, item, comparer);
			}

			return result;
		}

		[CanBeNull]
		public static TValue GetValueByKeyOrDefault<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> instance, TKey key, TValue defaultValue = default(TValue))
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			TValue value;
			if (!instance.TryGetValue(key, out value))
			{
				value = defaultValue;
			}
			return value;
		}

		public static void Foreach<T>([NotNull] this IEnumerable<T> instance, [NotNull] Action<T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance)); 
			}

			foreach (var item in instance)
			{
				action(item);
			}
		}

		public static void For<T>([NotNull] this IList<T> instance, [NotNull] Action<T> action, int minIndex = 0, int maxIndex = -1, int threads = 1)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (maxIndex < 0)
			{
				maxIndex = instance.Count + maxIndex;
			}

			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			for (var i = minIndex; i <= maxIndex; i++)
			{
				action(instance[i]);
			}
		}

		[NotNull]
		public static IEnumerable<T> Prepend<T>([NotNull] this IEnumerable<T> instance, [NotNull] params T[] items)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			foreach(var item in items)
			{
				yield return item;
			}

			foreach (var item in instance)
			{
				yield return item;
			}
		}
		
		[NotNull]
		public static IEnumerable<T> Append<T>([NotNull] this IEnumerable<T> instance, [NotNull] params T[] items)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			foreach (var item in instance)
			{
				yield return item;
			}
			
			foreach(var item in items)
			{
				yield return item;
			}
		}

		public static bool AddOrUpdate<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> instance, TKey key, TValue value)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			var containsKey = instance.ContainsKey(key);

			if (containsKey)
			{
				instance[key] = value;
			}
			else
			{
				instance.Add(key, value);
			}

			return !containsKey;
		}

		public static bool AddIfMissing<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> instance, TKey key, TValue value)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			if (instance.ContainsKey(key))
			{
				return false;
			}

			instance.Add(key, value);
			return true;
		}

		public static void AddRange<T>([NotNull] this IList<T> instance, [CanBeNull] IEnumerable<T> items)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));

			items?.Foreach(instance.Add);
		}

		public static void Reset<T>([NotNull] this IList<T> instance, [CanBeNull] IEnumerable<T> items)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			
			instance.Clear();
			items?.Foreach(instance.Add);
		}

		public static void RemoveRange<T>([NotNull] this IList<T> instance, [CanBeNull] IEnumerable<T> items)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));

			items?.Foreach(x => instance.Remove(x));
		}

		private static void Add<TKey, TValue>([NotNull] this IDictionary<TKey, IList<TValue>> instance, TKey key, TValue value, IEqualityComparer<TKey> comparer)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			var predicate = new Func<TKey, bool>(x =>
			{
				if (comparer.GetHashCode(x) == comparer.GetHashCode(key))
				{
					return true;
				}

				if (comparer.Equals(x, key))
				{
					return true;
				}

				return false;
			});

			var hasKey = instance.Keys.Any(predicate);
			if (hasKey)
			{
				instance[instance.Keys.First(predicate)].Add(value);
			}
			else
			{
				instance.Add(key, new List<TValue> { value });
			}
		}
	}
}
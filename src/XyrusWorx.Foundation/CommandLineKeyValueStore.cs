using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using XyrusWorx.Collections;
using XyrusWorx.IO;

namespace XyrusWorx
{
	[PublicAPI]
	public class CommandLineKeyValueStore : KeyValueStore<string>
	{
		private readonly string[] mArgs;

		private HashSet<StringKey> mKnownFlags;
		private Dictionary<StringKey, StringKey> mAliases;
		private IDictionary<StringKey, List<string>> mArguments;

		private const string mAliasPrefix = @"-";
		private const string mParameterPrefix = @"--";

		public CommandLineKeyValueStore()
		{
			mAliases = new Dictionary<StringKey, StringKey>();
			mKnownFlags = new HashSet<StringKey>();
		}
		public CommandLineKeyValueStore(string[] args) : this()
		{
			mArgs = args;
		}

		public void RegisterAlias(StringKey parameter, StringKey alias)
		{
			mAliases.AddOrUpdate(alias, parameter);
			mArguments = null;
		}
		public void RegisterFlag(StringKey parameter)
		{
			mKnownFlags.Add(parameter);
		}

		[NotNull]
		public IEnumerable<string> ReadTail() => ReadMany(new StringKey());

		[NotNull]
		public IEnumerable<string> ReadMany(StringKey key)
		{
			if (Exists(key))
			{
				return mArguments[key].ToArray();
			}

			return new string[0];
		}

		[NotNull]
		public IEnumerable<StringKey> Flags
		{
			get
			{
				if (mArguments == null)
				{
					mArguments = Parse();
				}

				return mArguments.Where(y => !y.Value.Any() || y.Value.All(x => x == null)).Where(x => !x.Key.IsEmpty).Select(x => x.Key);
			}
		}

		[NotNull]
		public IEnumerable<StringKey> Arguments
		{
			get
			{
				if (mArguments == null)
				{
					mArguments = Parse();
				}

				return mArguments.Where(y => y.Value.Any() && y.Value.All(x => x != null)).Where(x => !x.Key.IsEmpty).Select(x => x.Key);
			}
		}

		[NotNull]
		public IEnumerable<string> Values
		{
			get
			{
				if (mArguments == null)
				{
					mArguments = Parse();
				}

				return mArguments.GetValueByKeyOrDefault(new StringKey())?.AsEnumerable() ?? new string[0];
			}
		}

		public override bool IsReadOnly => true;
		public override bool Exists(StringKey key)
		{
			if (mArguments == null)
			{
				mArguments = Parse();
			}

			return mArguments.ContainsKey(key);
		}

		protected override string GetValue(StringKey key)
		{
			if (mArguments == null)
			{
				mArguments = Parse();
			}

			return mArguments[key].FirstOrDefault();
		}
		protected override void SetValue(StringKey key, string value)
		{
			throw new NotSupportedException();
		}
		protected override IEnumerable<StringKey> Enumerate()
		{
			if (mArguments == null)
			{
				mArguments = Parse();
			}

			return mArguments.Keys.Where(x => !x.IsEmpty);
		}

		private IDictionary<StringKey, List<string>> Parse()
		{
#if (NO_NATIVE_BOOTSTRAPPER)
			var args = new string[0]; // UAP never has command line arguments!
#else
			var args = mArgs ?? Environment.GetCommandLineArgs().Skip(1).ToArray();
#endif

			var result = new Dictionary<StringKey, List<string>>();
			var remainder = new List<string>();
			var argsCopy = args.ToList();

			var lastArg = argsCopy.LastOrDefault();

			while (!string.IsNullOrWhiteSpace(lastArg))
			{
				if (argsCopy.Count > 1)
				{
					var secondLastArg = argsCopy[argsCopy.Count - 2];
					var secondLastParameter =
						(secondLastArg.StartsWith(mParameterPrefix) ? secondLastArg.Substring(mParameterPrefix.Length) : null) ??
						(secondLastArg.StartsWith(mAliasPrefix) && !secondLastArg.StartsWith(mParameterPrefix) ? secondLastArg.Substring(mAliasPrefix.Length) : null);

					var isFlag = !string.IsNullOrWhiteSpace(secondLastParameter) && mKnownFlags.Contains(secondLastParameter);
					if (isFlag || !secondLastArg.StartsWith(mAliasPrefix) && !secondLastArg.StartsWith(mParameterPrefix))
					{
						remainder.Add(lastArg);

						if (isFlag)
						{
							argsCopy.RemoveAt(argsCopy.Count - 1);
							break;
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					remainder.Add(lastArg);
				}

				argsCopy.RemoveAt(argsCopy.Count - 1);
				lastArg = argsCopy.LastOrDefault();
			}

			remainder.Reverse();

			for (var i = 0; i < argsCopy.Count; i++)
			{
				var current = argsCopy[i] ?? string.Empty;
				var next = i < argsCopy.Count - 1
					? argsCopy[i + 1] ?? string.Empty
					: string.Empty;

				var parameter = current.StartsWith(mParameterPrefix) ? current.Substring(mParameterPrefix.Length) : null;
				var alias = current.StartsWith(mAliasPrefix) && !current.StartsWith(mParameterPrefix) ? current.Substring(mAliasPrefix.Length) : null;

				StringKey key;
				string value;

				if (!string.IsNullOrWhiteSpace(alias) && mAliases.ContainsKey(alias.AsKey().Normalize()))
				{
					key = mAliases[alias.AsKey().Normalize()];
				}
				else if (!string.IsNullOrWhiteSpace(parameter))
				{
					key = parameter.AsKey().Normalize();
				}
				else
				{
					continue;
				}

				if (!string.IsNullOrWhiteSpace(next))
				{
					if (next.StartsWith(mParameterPrefix) || next.StartsWith(mAliasPrefix))
					{
						value = null;
					}
					else
					{
						value = mKnownFlags.Contains(parameter ?? alias) ? null : next;
					}
				}
				else
				{
					value = null;
				}

				var list = result.GetValueByKeyOrDefault(key);
				if (list == null)
				{
					result.Add(key, list = new List<string>());
				}

				if (list.Count > 0 && list.All(string.IsNullOrWhiteSpace))
				{
					list.Clear();
				}

				list.Add(value);
			}

			var looseTokens = new List<string>();

			foreach (var item in remainder)
			{
				if (!item.StartsWith(mAliasPrefix) && !item.StartsWith(mParameterPrefix))
				{
					looseTokens.Add(item);
				}
				else
				{
					var parameter = item.StartsWith(mParameterPrefix) ? item.Substring(mParameterPrefix.Length) : null;
					var alias = item.StartsWith(mAliasPrefix) && !item.StartsWith(mParameterPrefix) ? item.Substring(mAliasPrefix.Length) : null;

					var flag = parameter ?? alias;
					if (!string.IsNullOrWhiteSpace(flag))
					{
						result.Add(flag, new List<string>{null});
					}
				}
			}

			result.Add(new StringKey(), looseTokens);

			return result;
		}
	}
}
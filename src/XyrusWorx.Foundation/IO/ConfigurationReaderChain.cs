using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public sealed class ConfigurationReaderChain : ConfigurationReader
	{
		private readonly List<ConfigurationReader> mReaders;

		public ConfigurationReaderChain(params ConfigurationReader[] readers)
		{
			mReaders = new List<ConfigurationReader>(readers ?? new ConfigurationReader[0]);
		}

		public IList<ConfigurationReader> Readers => mReaders;

		protected override object GetValue(StringKeySequence sequence)
		{
			foreach (var reader in mReaders)
			{
				if (reader.Exists(sequence))
				{
					return reader.Read(sequence);
				}
			}

			return null;
		}
		protected override IEnumerable<StringKeySequence> EnumerateKeySequences()
		{
			return mReaders.SelectMany(x => (x as IHierarchicKeyValueStore).GetKeys()).Distinct();
		}
	}
}
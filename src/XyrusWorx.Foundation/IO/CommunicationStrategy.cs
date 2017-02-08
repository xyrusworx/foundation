using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public abstract class CommunicationStrategy
	{
		private static Dictionary<StringKey, CommunicationStrategy> mStrategies = new Dictionary<StringKey, CommunicationStrategy>();

		static CommunicationStrategy()
		{
			RegisterCommunicationStrategy(Default);
		}
		internal CommunicationStrategy() { }

		[NotNull]
		public abstract string ContentType { get; }

		[NotNull] public abstract Task<object> ReadAsync([NotNull] Stream stream, [NotNull] Encoding encoding, [NotNull] Type type);
		[NotNull] public abstract Task WriteAsync([NotNull] Stream stream, [NotNull] Encoding encoding, [CanBeNull] object obj);

		[NotNull]
		public static JsonCommunicationStrategy Default { get; } = new JsonCommunicationStrategy();

		public static void RegisterCommunicationStrategy([NotNull] CommunicationStrategy strategy)
		{
			if (strategy == null) throw new ArgumentNullException(nameof(strategy));

			mStrategies.AddOrUpdate(new StringKey(strategy.ContentType).Normalize(), strategy);
		}

		[CanBeNull]
		public static CommunicationStrategy GetCommunicationStrategy([NotNull] string mimeType)
		{
			if (mimeType.NormalizeNull() == null) throw new ArgumentNullException(nameof(mimeType));
			return mStrategies.GetValueByKeyOrDefault(new StringKey(mimeType).Normalize());
		}
	}
}
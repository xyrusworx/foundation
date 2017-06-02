using System;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public class DelegateLogWriter : LogWriter
	{
		private readonly Action<LogMessage> mMessageCallback;

		public DelegateLogWriter([NotNull] Action<LogMessage> messageCallback)
		{
			if (messageCallback == null)
			{
				throw new ArgumentNullException(nameof(messageCallback));
			}

			mMessageCallback = messageCallback;
		}

		protected override void DispatchOverride(LogMessage[] messages)
		{
			messages.Foreach(x => mMessageCallback?.Invoke(x));
		}
	}
}
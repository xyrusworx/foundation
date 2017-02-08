using System.Collections.Generic;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public sealed class QueueLogWriter : LogWriter
	{
		private readonly Queue<LogMessage> mLogMessages = new Queue<LogMessage>();
		private readonly object mLock = new object();

		protected override void DispatchOverride(LogMessage[] messages)
		{
			lock (mLock)
			{
				messages.Foreach(x => mLogMessages.Enqueue(x));
			}
		}

		public bool HasMessages
		{
			get
			{
				lock (mLock)
				{
					return mLogMessages.Count > 0;
				}
			}
		}

		[NotNull]
		public LogMessage[] DequeueAll()
		{
			var result = new List<LogMessage>();

			lock (mLock)
			{
				while (mLogMessages.Count > 0)
				{
					result.Add(mLogMessages.Dequeue());
				}
			}

			return result.ToArray();
		}
	}
}
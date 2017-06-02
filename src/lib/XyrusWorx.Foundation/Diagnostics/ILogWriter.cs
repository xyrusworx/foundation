using System;
using JetBrains.Annotations;
using XyrusWorx.Threading;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public interface ILogWriter
	{
		[NotNull]
		IScope MessageScope { get; }
		LogVerbosity Verbosity { get; set; }

		void Flush();
		void Write(string message, LogMessageClass messageClass = LogMessageClass.Information);

		bool IsFlushDelayed { get; set; }
		TimeSpan FlushInterval { get; set; }

		[NotNull]
		LinkedMessageDispatcherCollection<LogMessage> LinkedDispatchers { get; }

		[CanBeNull]
		LogFilter Filter { get; set; }
	}
}
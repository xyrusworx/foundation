using System;
using JetBrains.Annotations;
using XyrusWorx.Threading;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public abstract class LogWriter : MessageDispatcher<LogMessage>, ILogWriter
	{
		private LogVerbosity mVerbosity;

		public LogFilter Filter { get; set; }

		public LogVerbosity Verbosity
		{
			get
			{
				var parentLog = Parent as ILogWriter;
				if (parentLog != null)
				{
					return parentLog.Verbosity;
				}

				return mVerbosity;
			}
			set
			{
				mVerbosity = value;
			}
		}

		public void Write(string message, LogMessageClass messageClass = LogMessageClass.Information)
		{
			Dispatch(new LogMessage {Text = message, Class = messageClass, Scope = MessageScope.State, Timestamp = DateTime.Now});
		}

		protected sealed override bool FilterOverride(ref LogMessage message)
		{
			if (Parent == null && (int)message.Class < (int)Verbosity)
			{
				return false;
			}

			if (Filter != null)
			{
				LogMessage outMessage;
				Filter(message, out outMessage);

				if (outMessage == null)
				{
					return false;
				}

				message = outMessage;
			}

			return true;
		}
	}
}

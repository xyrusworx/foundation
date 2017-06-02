using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public class LogMessage
	{
		private static readonly int mMaxClassColumnLength;
		private const int mMaxScopeColumnLength = 10;
		private const string mColumnSeparator = " | ";
		private const string mNullPlaceholder = "<null>";

		static LogMessage()
		{
			mMaxClassColumnLength = Enum.GetValues(typeof(LogMessageClass)).OfType<LogMessageClass>().Max(x => ClassToString(x).Length);
		}

		public DateTime Timestamp { get; set; } = DateTime.Now;
		public LogMessageClass Class { get; set; } = LogMessageClass.Information;

		public string Text { get; set; }
		public object Scope { get; set; }

		public override string ToString() => ToString(150);
		public string ToString(int lineLength)
		{
			const char space = ' ';

			var clstr = ClassToString(Class);
			var state = Scope?.ToString() ?? mNullPlaceholder;
			var origin = !string.IsNullOrEmpty(state)
				? state.Substring(0, Math.Min(state.Length, mMaxScopeColumnLength)).PadRight(mMaxScopeColumnLength, space)
				: @" <null> ";

			var padString = new string(space, mMaxScopeColumnLength + mMaxClassColumnLength + mColumnSeparator.Length);
			var wrappedText = Text.WordWrap(lineLength - 1, padString + mColumnSeparator, string.Empty) ?? string.Empty;
			var lineBuilder = new StringBuilder();

			lineBuilder.Append(origin);
			lineBuilder.Append(mColumnSeparator);

			lineBuilder.Append(clstr.PadRight(mMaxClassColumnLength, space));
			lineBuilder.Append(mColumnSeparator);

			lineBuilder.Append(wrappedText);

			return lineBuilder.ToString();
		}

		private static string ClassToString(LogMessageClass @class)
		{
			switch (@class)
			{
				case LogMessageClass.Debug:
					return "DEBUG";
				case LogMessageClass.Verbose:
					return "VERBOSE";
				case LogMessageClass.Information:
					return "NOTICE";
				case LogMessageClass.Warning:
					return "WARNING";
				case LogMessageClass.Error:
					return "ERROR";
			}

			return null;
		}
	}
}
using JetBrains.Annotations;
using System;
using System.Text;
using XyrusWorx.Runtime;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public class ConsoleWriter : LogWriter
	{
		private static readonly object mDispatchLock = new object();
		private readonly int mLineLength;

		public ConsoleWriter()
		{
			int lineLength;

			try
			{
				lineLength = Console.BufferWidth;
			}
			catch
			{
				lineLength = 120;
			}

			mLineLength = lineLength;
		}
		public int SuggestedMaxLineLength => mLineLength;

		protected sealed override void DispatchOverride(LogMessage[] messages)
		{
			lock (mDispatchLock)
			{
				var stringBuilder = new StringBuilder();
				var lastClass = (LogMessageClass?)null;

				foreach (var message in messages)
				{
					if (message.Class != lastClass)
					{
						if (stringBuilder.Length > 0 && lastClass.HasValue)
						{
							WriteBulk(stringBuilder.ToString(), lastClass.Value);
						}

						stringBuilder.Clear();
						lastClass = message.Class;
					}

					var lineBuilder = new StringBuilder();

					Format(lineBuilder, message);
					stringBuilder.AppendLine(lineBuilder.ToString());
				}

				if (stringBuilder.Length > 0 && lastClass.HasValue)
				{
					WriteBulk(stringBuilder.ToString(), lastClass.Value);
				}
			}
		}
		protected virtual void Format([NotNull] StringBuilder line, [NotNull] LogMessage message)
		{
			line.Append(message.ToString(mLineLength));
		}

		protected virtual ConsoleColor? GetForeground(LogMessageClass messageClass)
		{
			switch (messageClass)
			{
				case LogMessageClass.Debug:
					return ConsoleColor.DarkGray;
				case LogMessageClass.Warning:
					return ConsoleColor.Yellow;
				case LogMessageClass.Error:
					return ConsoleColor.White;
			}

			return null;
		}
		protected virtual ConsoleColor? GetBackground(LogMessageClass messageClass)
		{
			switch (messageClass)
			{
				case LogMessageClass.Warning:
					return ConsoleColor.DarkMagenta;
				case LogMessageClass.Error:
					return ConsoleColor.DarkRed;
			}

			return null;
		}

		private void WriteBulk(string bulk, LogMessageClass c)
		{
			using (new ConsoleColorScope(GetForeground(c), GetBackground(c)).Enter())
			{
				var outputStream = c == LogMessageClass.Error
					? Console.Error
					: Console.Out;

				outputStream.Write(bulk);
			}
		}
	}
}
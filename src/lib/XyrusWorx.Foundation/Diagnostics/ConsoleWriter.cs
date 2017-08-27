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

		public bool IncludeScope { get; set; }

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
			if (IncludeScope && !string.IsNullOrWhiteSpace(message.Scope?.ToString()) && !string.IsNullOrWhiteSpace(message.Text))
			{
				line.Append($"{message.Scope}: ");
			}

			line.Append(message.Text);
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
					return ConsoleColor.Red;
			}

			return null;
		}
		protected virtual ConsoleColor? GetBackground(LogMessageClass messageClass) => null;

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
using JetBrains.Annotations;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public class LightConsoleWriter : ConsoleWriter
	{
		public bool IncludeScope { get; set; }

		protected override void Format(StringBuilder line, LogMessage message)
		{
			if (IncludeScope && !string.IsNullOrWhiteSpace(message.Scope?.ToString()) && !string.IsNullOrWhiteSpace(message.Text))
			{
				line.Append($"{message.Scope}: ");
			}

			line.Append(message.Text);
		}

		protected override ConsoleColor? GetForeground(LogMessageClass messageClass)
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
		protected override ConsoleColor? GetBackground(LogMessageClass messageClass) => null;
	}
}
using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public delegate void LogFilter(LogMessage message, out LogMessage outMessage);
}
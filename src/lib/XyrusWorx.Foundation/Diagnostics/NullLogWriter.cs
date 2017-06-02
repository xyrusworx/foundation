using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public sealed class NullLogWriter : LogWriter
	{
		protected override void DispatchOverride(LogMessage[] messages)
		{
		}
	}
}
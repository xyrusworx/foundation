using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public class ErrorDetails
	{
		public int HResult { get; set; }

		[CanBeNull]
		public string StackTrace { get; set; }
	}
}
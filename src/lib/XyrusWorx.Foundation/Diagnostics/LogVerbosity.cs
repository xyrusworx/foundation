using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public enum LogVerbosity
	{
		Debug = -2,
		Verbose = -1,
		Normal = 0,
		WarningsAndErrors = 1,
		ErrorsOnly = 2
	}
}
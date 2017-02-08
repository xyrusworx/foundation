using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public enum LogMessageClass
	{
		Debug = -2,
		Verbose = -1,
		Information = 0,
		Warning = 1,
		Error = 2
	}
}
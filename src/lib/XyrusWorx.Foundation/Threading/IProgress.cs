using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public interface IProgress
	{
		string DisplayName { get; }
		double Progress { get; }

		bool IsInitializing { get; }
		bool IsIdle { get; }
		bool IsAborted { get; }
		bool IsCompleted { get; }
	}
}
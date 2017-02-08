using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public interface IWaitHandler
	{
		void Wait([NotNull] Func<bool> condition, CancellationToken token = default(CancellationToken));
	}
}
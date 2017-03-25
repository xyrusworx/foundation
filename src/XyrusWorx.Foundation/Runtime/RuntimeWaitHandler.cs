using System;
using System.Threading;
using XyrusWorx.Threading;

namespace XyrusWorx.Runtime
{
	class RuntimeWaitHandler : IWaitHandler
	{
		public void Wait(Func<bool> condition, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (condition == null)
			{
				throw new ArgumentNullException(nameof(condition));
			}

			while (!condition())
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				ProcessMessages();
			}
		}

		protected virtual void ProcessMessages()
		{
			// skip
		}
	}
}
using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class OperationWaitHandler : IWaitHandler
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
			Thread.Sleep(100);
		}
	}
}
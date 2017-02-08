using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class DelayAction
	{
		private readonly object mInitLock = new object();

		private readonly Action mAction;
		private readonly TimeSpan mDelay;

		private CancellationTokenSource mCancel;

		public DelayAction([NotNull] Action action, TimeSpan delay)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			mAction = action;
			mDelay = delay;
		}

		public void QueueInvocation()
		{
			var operation = new RelayOperation(new Action(ExecuteActionAfterDelay));

			operation.DispatchMode = OperationDispatchMode.ThreadPoolUserWorkItem;
			operation.Run();
		}
		public void CancelInvocation()
		{
			lock (mInitLock)
			{
				mCancel?.Cancel();
			}
		}

		private void ExecuteActionAfterDelay()
		{
			CancelInvocation();

			CancellationToken cancelToken;

			lock (mInitLock)
			{
				mCancel = new CancellationTokenSource();
				cancelToken = mCancel.Token;
			}

			var handler = new OperationWaitHandler();

			handler.Delay(mDelay, cancelToken);

			try
			{
				if (!cancelToken.IsCancellationRequested)
				{
					mAction();
				}
			}
			finally
			{
				lock (mInitLock)
				{
					mCancel = null;
				}
			}
		}
	}
}
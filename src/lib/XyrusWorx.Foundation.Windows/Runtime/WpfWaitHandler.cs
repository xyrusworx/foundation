using System.Threading;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Threading;

namespace XyrusWorx.Windows.Runtime 
{
	[PublicAPI]
	public class WpfWaitHandler : OperationWaitHandler
	{
		private readonly Dispatcher mDispatcher;

		public WpfWaitHandler([CanBeNull] Dispatcher dispatcher)
		{
			mDispatcher = dispatcher;
		}
	
		protected override void ProcessMessages()
		{
			if (mDispatcher == null)
			{
				Thread.Sleep(100);
			}
			else
			{
				mDispatcher.Invoke(() => { }, DispatcherPriority.Background);
			}
		}
	}
}
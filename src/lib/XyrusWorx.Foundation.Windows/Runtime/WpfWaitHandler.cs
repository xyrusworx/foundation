using System.Threading;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Threading;

namespace XyrusWorx.Windows.Runtime 
{
	[PublicAPI]
	public class WpfWaitHandler : OperationWaitHandler
	{
		protected override void ProcessMessages()
		{
			var dispatcher = System.Windows.Application.Current?.Dispatcher;
			if (dispatcher == null)
			{
				Thread.Sleep(100);
			}
			else
			{
				dispatcher.Invoke(() => { }, DispatcherPriority.Background);
			}
		}
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.Threading;

namespace XyrusWorx
{
	[PublicAPI]
	public static class DelegateUtils
	{
		public static void ExecuteDelayed([NotNull] this Action action, TimeSpan delay)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var operation = new RelayOperation(() =>
			{
				Thread.Sleep(delay);
				action();
			});

			operation.DispatchMode = OperationDispatchMode.ThreadPoolUserWorkItem;
			operation.Run();
		}
		public static async Task ExecuteDelayedAsync([NotNull] this Action action, TimeSpan delay)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			await Task.Run(() =>
			{
				Thread.Sleep(delay);
				action();
			});
		}
	}
}
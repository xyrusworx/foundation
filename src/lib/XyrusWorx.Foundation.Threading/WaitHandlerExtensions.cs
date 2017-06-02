using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public static class WaitHandlerExtensions
	{
		public static void Delay([NotNull] this IWaitHandler waitHandler, TimeSpan delay, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (waitHandler == null)
			{
				throw new ArgumentNullException(nameof(waitHandler));
			}

			var clock = new Clock();
			var scope = new Scope().Enter();

			clock.TickInterval = delay;
			clock.TickAction = () =>
			{
				scope.Leave();
				clock.IsEnabled = false;
			};

			clock.IsEnabled = true;
			waitHandler.Wait(() => !scope.IsInScope, cancellationToken);
		}
		public static void Delay([NotNull] this IWaitHandler waitHandler, int milliseconds, CancellationToken cancellationToken = default(CancellationToken))
		{
			Delay(waitHandler, TimeSpan.FromMilliseconds(milliseconds), cancellationToken);
		}
	}
}
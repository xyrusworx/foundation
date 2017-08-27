using System;
using System.Windows.Threading;
using JetBrains.Annotations;
using Application = System.Windows.Application;

namespace XyrusWorx.Windows
{
	[PublicAPI]
	public static class Execution
	{
		public static T ExecuteOnUiThread<T>([NotNull] this Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			var dispatcher = Application.Current?.Dispatcher;
			if (dispatcher == null)
			{
				return func();
			}

			return dispatcher.Invoke(func, priority);
		}
		
		public static void ExecuteOnUiThread([NotNull] this Action action, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			var dispatcher = Application.Current?.Dispatcher;
			if (dispatcher == null)
			{
				action();
				return;
			}

			dispatcher.Invoke(action, priority);
		}
	}

}

using System;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Runtime;
using XyrusWorx.Windows.Runtime;
using XyrusWorx.Windows.ViewModels;
using Application = System.Windows.Application;

namespace XyrusWorx.Windows
{
	[PublicAPI]
	public static class Execution
	{
		[NotNull]
		public static IApplicationHost Start([NotNull] Type controllerType, [NotNull] Dispatcher dispatcher, IServiceLocator serviceLocator = null)
		{
			if (controllerType == null)
			{
				throw new ArgumentNullException(nameof(controllerType));
			}
			
			if (dispatcher == null)
			{
				throw new ArgumentNullException(nameof(dispatcher));
			}

			var controllerInstance = (serviceLocator ?? ServiceLocator.Default).CreateInstance(controllerType).CastTo<WpfApplication>();
			if (controllerInstance == null)
			{
				throw new ArgumentException($"The type \"{controllerType}\" can't be cast into an {nameof(WpfApplication)}.");
			}
			
			var runtime = new ConstructedHost(dispatcher, controllerInstance);

			controllerInstance.Host = runtime;
			
			return runtime;
		}
		
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

		class ConstructedHost : IApplicationHost
		{
			private readonly Dispatcher mDispatcher;
			private readonly WpfApplication mController;

			public ConstructedHost([NotNull] Dispatcher dispatcher, [NotNull] WpfApplication controller)
			{
				if (dispatcher == null)
				{
					throw new ArgumentNullException(nameof(dispatcher));
				}
				if (controller == null)
				{
					throw new ArgumentNullException(nameof(controller));
				}

				mDispatcher = dispatcher;
				mController = controller;
			}
			
			public XyrusWorx.Runtime.Application Application => mController;
			public ViewModel ViewModel { get; set; }
			public FrameworkElement View { get; set; }

			public Dispatcher GetDispatcher()
			{
				return mDispatcher;
			}
			public void Shutdown(int exitCode = 0)
			{
				Environment.Exit(exitCode);
			}
		}
	}
}

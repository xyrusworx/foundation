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
		public static IApplicationRuntime Start([NotNull] Type controllerType, [NotNull] Dispatcher dispatcher, IServiceLocator serviceLocator = null)
		{
			if (controllerType == null)
			{
				throw new ArgumentNullException(nameof(controllerType));
			}
			
			if (dispatcher == null)
			{
				throw new ArgumentNullException(nameof(dispatcher));
			}

			var controllerInstance = (serviceLocator ?? ServiceLocator.Default).CreateInstance(controllerType).CastTo<ApplicationController>();
			if (controllerInstance == null)
			{
				throw new ArgumentException($"The type \"{controllerType}\" can't be cast into an {nameof(ApplicationController)}.");
			}
			
			var runtime = new ConstructedRuntime(dispatcher, controllerInstance);

			controllerInstance.Runtime = runtime;
			
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

		class ConstructedRuntime : IApplicationRuntime
		{
			private readonly Dispatcher mDispatcher;
			private readonly ApplicationController mController;

			public ConstructedRuntime([NotNull] Dispatcher dispatcher, [NotNull] ApplicationController controller)
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
			
			public ApplicationController Controller => mController;
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

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Runtime;
using XyrusWorx.Windows.Runtime;
using XyrusWorx.Windows.ViewModels;

namespace XyrusWorx.Windows 
{
	[PublicAPI]
	public class WindowsApplicationHost : IWindowsApplicationHost
	{
		private readonly WpfApplication mApplication;

		public WindowsApplicationHost([NotNull] Dispatcher dispatcher) : this(new WpfApplication(dispatcher))
		{
		}
		public WindowsApplicationHost([NotNull] WpfApplication application)
		{
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			mApplication = application;
			mApplication.Host = this;
		}
			
		public XyrusWorx.Runtime.Application Application
		{
			get => mApplication;
		}
		public Dispatcher Dispatcher
		{
			get => mApplication.Dispatcher;
		}

		public ViewModel ViewModel { get; set; }
		public FrameworkElement View { get; set; }

		public void Execute(Action action, TaskPriority priority = TaskPriority.Normal) => mApplication.Dispatcher.Invoke(action, (DispatcherPriority)(int)priority);
		public T Execute<T>(Func<T> func, TaskPriority priority = TaskPriority.Normal) => mApplication.Dispatcher.Invoke(func, (DispatcherPriority)(int)priority);
		public async Task ExecuteAsync(Action action, TaskPriority priority = TaskPriority.Normal) => await mApplication.Dispatcher.InvokeAsync(action, (DispatcherPriority)(int)priority);
		public async Task<T> ExecuteAsync<T>(Func<T> func, TaskPriority priority = TaskPriority.Normal) => await mApplication.Dispatcher.InvokeAsync(func, (DispatcherPriority)(int)priority);

		public void Shutdown(int exitCode = 0)
		{
			Environment.Exit(exitCode);
		}
	}
}
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
		private readonly Dispatcher mDispatcher;
		private readonly WpfApplication mApplication;

		internal WindowsApplicationHost([NotNull] Dispatcher dispatcher, [NotNull] WpfApplication application)
		{
			if (dispatcher == null)
			{
				throw new ArgumentNullException(nameof(dispatcher));
			}
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			mDispatcher = dispatcher;
			mApplication = application;
		}
			
		public XyrusWorx.Runtime.Application Application => mApplication;
		
		public ViewModel ViewModel { get; set; }
		public FrameworkElement View { get; set; }

		public void Execute(Action action, TaskPriority priority = TaskPriority.Normal) => mDispatcher.Invoke(action, (DispatcherPriority)(int)priority);
		public T Execute<T>(Func<T> func, TaskPriority priority = TaskPriority.Normal) => mDispatcher.Invoke(func, (DispatcherPriority)(int)priority);
		public async Task ExecuteAsync(Action action, TaskPriority priority = TaskPriority.Normal) => await mDispatcher.InvokeAsync(action, (DispatcherPriority)(int)priority);
		public async Task<T> ExecuteAsync<T>(Func<T> func, TaskPriority priority = TaskPriority.Normal) => await mDispatcher.InvokeAsync(func, (DispatcherPriority)(int)priority);

		public void Shutdown(int exitCode = 0)
		{
			Environment.Exit(exitCode);
		}
	}
}
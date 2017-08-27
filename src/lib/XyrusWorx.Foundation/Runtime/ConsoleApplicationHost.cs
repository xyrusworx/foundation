using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public class ConsoleApplicationHost : IApplicationHost
	{
		private ConsoleApplication mApplication;

		public ConsoleApplicationHost([NotNull] ConsoleApplication application)
		{
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}
			
			mApplication = application;
			mApplication.Run();
		}
		
		public static void Run([NotNull] Type applicationType, IServiceLocator serviceLocator = null)
		{
			if (applicationType == null)
			{
				throw new ArgumentNullException(nameof(applicationType));
			}

			var sl = serviceLocator ?? ServiceLocator.Default;
			var instance = sl.CreateInstance(applicationType);

			if (!(instance is ConsoleApplication application))
			{
				throw new ArgumentException($"{applicationType} is not implicitly convertible to {typeof(ConsoleApplication)}.");
			}
			
			sl.Register<IApplicationHost>(new ConsoleApplicationHost(application));
			application.Run();
		}
		public static void Run([NotNull] ConsoleApplication instance, IServiceLocator serviceLocator = null)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			var sl = serviceLocator ?? ServiceLocator.Default;
			
			sl.Register<IApplicationHost>(new ConsoleApplicationHost(instance));
			instance.Run();
		}

		public Application Application
		{
			get => mApplication;
		}

		public void Execute(Action action, TaskPriority priority = TaskPriority.Normal) => action();
		public T Execute<T>(Func<T> func, TaskPriority priority = TaskPriority.Normal) => func();
		public async Task ExecuteAsync(Action action, TaskPriority priority = TaskPriority.Normal) => await Task.Run(action);
		public async Task<T> ExecuteAsync<T>(Func<T> func, TaskPriority priority = TaskPriority.Normal) => await Task.Run(func);

		public void Shutdown(int exitCode = 0)
		{
			mApplication.Cancel();
			Environment.Exit(exitCode);
		}
	}
}
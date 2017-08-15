using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Runtime;
using XyrusWorx.Threading;
using XyrusWorx.Windows.ViewModels;
using Application = System.Windows.Application;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public class ApplicationDefinition : Application
	{
		private readonly Scope mRunningScope;

		private Type mControllerType;
		private Type mViewModelType;
		private Type mViewType;

		private IOperation mApplication;
		private ViewModel mViewModel;

		public ApplicationDefinition()
		{
			mRunningScope = new Scope();
			ShutdownMode = ShutdownMode.OnMainWindowClose;
		}

		public Type ControllerType
		{
			get { return mControllerType; }
			set
			{
				if (mRunningScope.IsInScope)
				{
					throw new InvalidOperationException($"Setting \"{nameof(ControllerType)}\" is not possible after the application has started.");
				}

				if (value != null && !typeof(IOperation).IsAssignableFrom(value))
				{
					throw new InvalidOperationException($"\"{nameof(ControllerType)}\" requires a type, which is implicitly convertible to \"{typeof(IOperation)}\"");
				}

				mControllerType = value;
			}
		}
		public Type ViewModelType
		{
			get { return mViewModelType; }
			set
			{
				if (mRunningScope.IsInScope)
				{
					throw new InvalidOperationException($"Setting \"{nameof(ViewModelType)}\" is not possible after the application has started.");
				}

				if (value != null && !typeof(ViewModel).IsAssignableFrom(value))
				{
					throw new InvalidOperationException($"\"{nameof(ViewModelType)}\" requires a type, which is implicitly convertible to \"{typeof(ViewModel)}\"");
				}

				mViewModelType = value;
			}
		}
		public Type ViewType
		{
			get { return mViewType; }
			set
			{
				if (mRunningScope.IsInScope)
				{
					throw new InvalidOperationException($"Setting \"{nameof(ViewType)}\" is not possible after the application has started.");
				}

				if (value != null && !typeof(Window).IsAssignableFrom(value))
				{
					throw new InvalidOperationException($"\"{nameof(ViewType)}\" requires a type, which is implicitly convertible to \"{typeof(Window)}\"");
				}

				mViewType = value;
			}
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			Operation.GlobalThreadException += OnGlobalThreadException;
			Dispatcher.UnhandledException += OnUnhandledException;

			mApplication = ControllerType != null ? (IOperation)Activator.CreateInstance(ControllerType) : null;
			mViewModel = ViewModelType != null ? (ViewModel)Activator.CreateInstance(ViewModelType) : null;
			MainWindow = ViewType != null ? (Window) Activator.CreateInstance(ViewType) : null;
			
			if (MainWindow != null)
			{
				MainWindow.DataContext = mViewModel;
			}
			else
			{
				Shutdown(0);
				return;
			}

			var specificApplication = mApplication.CastTo<ApplicationController>();
			if (specificApplication != null)
			{
				specificApplication.Definition = this;

				ServiceLocator.Default.Register(this);
			}

			ViewModel.GlobalPropertyChanged += OnGlobalPropertyChanged;

			var relay = new RelayOperation(() => mApplication?.Run());
			relay.DispatchMode = OperationDispatchMode.BackgroundThread;

			mRunningScope.Enter();
			relay.Run();
			MainWindow?.Show();
		}
		protected override void OnExit(ExitEventArgs e)
		{
			mApplication?.Cancel();
			mRunningScope.Leave();

			// ReSharper disable once ConstantConditionalAccessQualifier
			if (mApplication?.ExecutionResult?.HasError ?? false)
			{
				e.ApplicationExitCode = -1;
			}

			base.OnExit(e);

			mApplication = null;
			mViewModel = null;
			MainWindow = null;
		}

		internal ViewModel ViewModel => mViewModel;

		private void OnGlobalThreadException(object sender, OperationUnhandledExceptionEventArgs e)
		{
			e.Handled = mApplication?.CastTo<ApplicationController>()?.GlobalExceptionHandler(e.Exception) ?? false;
		}
		private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			e.Handled = mApplication?.CastTo<ApplicationController>()?.GlobalExceptionHandler(e.Exception) ?? false;
		}
		private void OnGlobalPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Dispatcher.BeginInvoke(new Action(CommandManager.InvalidateRequerySuggested));
		}
	}
}

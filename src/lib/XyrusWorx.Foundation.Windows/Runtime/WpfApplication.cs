using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Runtime;
using XyrusWorx.Windows.ViewModels;
using Application = XyrusWorx.Runtime.Application;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
	[SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
	public abstract class WpfApplication : Application, IDialogService, IExceptionHandlerService
	{
		protected WpfApplication() : this(System.Windows.Application.Current?.Dispatcher){}
		protected WpfApplication(Dispatcher dispatcher)
		{
			WaitHandler = new WpfWaitHandler(dispatcher);

			ServiceLocator.Default.Register<IMessageBox, WindowsMessageBox>();
			ServiceLocator.Default.Register<IExceptionHandlerService>(this);
		}

		[NotNull]
		public IWindowsApplicationHost Host { get; internal set; }

		[NotNull]
		public IMessageBox Dialog => new WindowsMessageBox(Host);

		[ContractAnnotation("=> halt")]
		public void Shutdown(int exitCode)
		{
			try
			{
				Host.Execute(() => Host.Shutdown(1));
			}
			catch
			{
				// ignore
			}

			Environment.Exit(1);
		}

		public FrameworkElement GetView() => Host?.View;
		public ViewModel GetViewModel() => Host?.ViewModel;

		public T GetView<T>() where T : FrameworkElement => GetView().CastTo<T>();
		public T GetViewModel<T>() where T: class => GetViewModel().CastTo<T>();

		public virtual bool HandleException(Exception exception) => false;

		protected sealed override IResult InitializeApplication()
		{
			OnInitialize();
			return Result.Success;
		}
		protected sealed override IResult Execute(CancellationToken cancellationToken)
		{
			WaitHandler.Wait(() => cancellationToken.IsCancellationRequested);

			return Result.Success;
		}
		protected sealed override void Cleanup(bool wasCancelled)
		{
			OnTerminate();
		}

		protected virtual void OnInitialize() { }
		protected virtual void OnTerminate() { }

		internal bool GlobalExceptionHandler([NotNull] Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			return HandleException(exception);
		}
		IMessageBox IDialogService.CreateDialog() => Dialog;
	}
}
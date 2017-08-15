using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using JetBrains.Annotations;
using XyrusWorx.MVVM;
using XyrusWorx.Runtime;
using Application = XyrusWorx.Runtime.Application;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
	[SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
	public abstract class ApplicationController : Application, IDialogService, IExceptionHandlerService
	{
		protected ApplicationController()
		{
			WaitHandler = new WpfWaitHandler();

			ServiceLocator.Default.Register<IMessageBox, WindowsMessageBox>();
			ServiceLocator.Default.Register<IExceptionHandlerService>(this);
		}

		[NotNull]
		public ApplicationDefinition Definition { get; internal set; }

		[NotNull]
		public IMessageBox Dialog => new WindowsMessageBox(Definition);

		[ContractAnnotation("=> halt")]
		public void Shutdown(int exitCode)
		{
			try
			{
				Definition.Dispatcher.Invoke(() => Definition.Shutdown(1));
			}
			catch
			{
				// ignore
			}

			Environment.Exit(1);
		}

		[NotNull] public Window GetView() => Definition?.MainWindow;
		[NotNull] public ViewModel GetViewModel() => Definition?.ViewModel;

		[NotNull] public T GetView<T>() where T : Window => GetView().CastTo<T>().AssertNotNull();
		[NotNull] public T GetViewModel<T>() where T: class => GetViewModel().CastTo<T>().AssertNotNull();

		public virtual bool HandleException(Exception exception) => false;

		protected sealed override IResult InitializeApplication()
		{
			ServiceLocator.Default.Register(Definition.Dispatcher);
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
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
	public class WpfApplication : Application, IDialogService, IExceptionHandlerService
	{
		internal protected WpfApplication([NotNull] Dispatcher dispatcher)
		{
			if (dispatcher == null)
			{
				throw new ArgumentNullException(nameof(dispatcher));
			}

			Dispatcher = dispatcher;
			WaitHandler = new WpfWaitHandler(dispatcher);

			ServiceLocator.Default.Register<IMessageBox, WindowsMessageBox>();
			ServiceLocator.Default.Register<IExceptionHandlerService>(this);
		}

		[NotNull]
		public Dispatcher Dispatcher { get; }
		
		[NotNull]
		public IWindowsApplicationHost Host { get; internal protected set; }

		[NotNull]
		public IMessageBox Dialog => new WindowsMessageBox(Host);

		[ContractAnnotation("=> halt")]
		public void Shutdown(int exitCode)
		{
			try
			{
				OnShutdown();
				Host.Execute(() => Host.Shutdown(exitCode));
			}
			catch
			{
				// ignore
			}

			Environment.Exit(exitCode);
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
			OnShutdown();
		}

		protected virtual void OnInitialize() { }
		protected virtual void OnShutdown() { }
		
		internal bool GlobalExceptionHandler([NotNull] Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			return HandleException(exception);
		}
		IMessageBox IDialogService.CreateDialog() => Dialog;

		[NotNull]
		public static WindowsApplicationHost Bootstrap<TView, TViewModel>([CanBeNull] Assembly locatorAssembly = null)
			where TView : FrameworkElement, new()
			where TViewModel : ViewModel 
			=> WpfApplication<TView, TViewModel>.Bootstrap<WpfApplication<TView, TViewModel>>(locatorAssembly: locatorAssembly ?? Assembly.GetCallingAssembly());
	}

	[PublicAPI]
	public class WpfApplication<TMainView, TMainViewModel> : WpfApplication
		where TMainView: FrameworkElement, new()
		where TMainViewModel: ViewModel
	{
		private TMainViewModel mViewModel;
		private TMainView mView;

		public WpfApplication() : this(Dispatcher.CurrentDispatcher)
		{
		}
		public WpfApplication(Dispatcher dispatcher) : base(dispatcher)
		{
			InitializeComponent();
		}

		protected virtual void OnConfigureWindow([NotNull] Window window) { }
		protected virtual Task OnInitialize([NotNull] TMainViewModel viewModel) => Task.CompletedTask;
		protected virtual Task OnShutdown([NotNull] TMainViewModel viewModel) => Task.CompletedTask;
		
		protected sealed override void OnInitialize()
		{
			var mainWindow = new Window();

			mainWindow.Title = Metadata.ProductName;
			mainWindow.Content = mView;
			mainWindow.DataContext = mViewModel;
			
			OnConfigureWindow(mainWindow);

			if (mViewModel != null)
			{
				OnInitialize(mViewModel).Begin();
			}

			mainWindow.Closed += (o, e) => Shutdown(0);
			mainWindow.Show();
		}
		protected sealed override void OnShutdown()
		{
			if (mViewModel != null)
			{
				OnShutdown(mViewModel).ExecuteSynchronous();
			}
			
			Dispatcher.InvokeShutdown();
		}

		internal protected void InitializeComponent([CanBeNull] IServiceLocator serviceLocator = null, [CanBeNull] Assembly locatorAssembly = null)
		{
			serviceLocator = serviceLocator ?? ServiceLocator.Default;

			if (locatorAssembly != null)
			{
				serviceLocator.AutoRegister(locatorAssembly, new[]{typeof(ViewModel)});
			}

			var viewModelResult = serviceLocator.TryResolve<TMainViewModel>();
			if (viewModelResult.HasError || viewModelResult.Data == null)
			{
				mViewModel = serviceLocator.CreateInstance<TMainViewModel>();
			}
			else
			{
				mViewModel = viewModelResult.Data;
			}
			
			mView = new TMainView();
		}
		
		[NotNull]
		internal protected static WindowsApplicationHost Bootstrap<TApplication>([CanBeNull] Assembly locatorAssembly = null) 
			where TApplication: WpfApplication<TMainView, TMainViewModel>, new()
		{
			var application = new TApplication();
			var applicationHost = new WindowsApplicationHost(application);

			ServiceLocator.Default.Register<Application>(application);
			ServiceLocator.Default.Register<IApplicationHost>(applicationHost);
			
			application.InitializeComponent(locatorAssembly: locatorAssembly ?? Assembly.GetCallingAssembly());

			applicationHost.ViewModel = application.mViewModel;
			applicationHost.View = application.mView;
			applicationHost.Application.Run();
			
			Dispatcher.Run();

			return applicationHost;
		}
	}
}
using JetBrains.Annotations;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;
using XyrusWorx.Threading;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public abstract class Application : IOperation
	{
		private static Application mCurrent;

		private IResult mResult;
		private IWaitHandler mWaitHandler;
		private CommandLineProcessor mCommandLine;
		private CancellationTokenSource mCancel;

		private bool mIsCompleted;
		private bool mIsInitializing;
		private bool mWasCancelled;
		private bool mIsRunning;

		protected Application()
		{
			if (mCurrent != null)
			{
				throw new InvalidOperationException("Only one application per domain is allowed.");
			}

#if (NO_NATIVE_BOOTSTRAPPER)
			Assembly assembly = null;
#else
			var assembly = Assembly.GetEntryAssembly() ?? GetType().GetTypeInfo().Assembly;
#endif

			Log = new NullLogWriter();
			CommandLine = new CommandLineKeyValueStore();
			Metadata = new AssemblyMetadata(assembly);
			Context = new ApplicationExecutionContext();

			mCommandLine = new CommandLineProcessor(GetType());

			var productKey = Metadata.FileName.NormalizeNull().TryTransform(Path.GetFileNameWithoutExtension);

#if (NO_NATIVE_BOOTSTRAPPER)
			var assemblyDir = Directory.GetCurrentDirectory();
#else
			var assemblyDir = (assembly.Location.TryTransform(Path.GetDirectoryName) ?? Directory.GetCurrentDirectory()).AsKey();
#endif

			var userDir = Context.GetUserDataDirectoryName("Local", Metadata.CompanyName.NormalizeNull() ?? "XW", productKey ?? ".Default");
			var machineDir = Context.GetMachineDataDirectoryName(Metadata.CompanyName.NormalizeNull() ?? "XW", productKey ?? ".Default");

			WorkingDirectory = new FileSystemStore(assemblyDir);
			UserDataDirectory = new FileSystemStore(userDir);
			MachineDataDirectory = new FileSystemStore(machineDir);

			mCurrent = this;
			mResult = Result.Success;
			mWaitHandler = new RuntimeWaitHandler();
		}

		public IWaitHandler WaitHandler
		{
			get { return mWaitHandler; }
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				mWaitHandler = value;
			}
		}

		[CanBeNull]
		public static Application Current => mCurrent ?? (mCurrent = new GenericApplication());

		[NotNull]
		public virtual string DisplayName => Metadata.ProductName ?? Metadata.AssemblyName ?? GetType().Name;

		[NotNull] public ApplicationExecutionContext Context { get; }

		[NotNull] public FileSystemStore WorkingDirectory { get; }
		[NotNull] public FileSystemStore UserDataDirectory { get; }
		[NotNull] public FileSystemStore MachineDataDirectory { get; }

		[NotNull] public AssemblyMetadata Metadata { get; }
		[NotNull] public LogWriter Log { get; }
		[NotNull] public CommandLineKeyValueStore CommandLine { get; }

		[NotNull]
		public CommandLineProcessor GetCommandLineProcessor() => mCommandLine;

		public event OperationUnhandledExceptionEventHandler ThreadException;

		public bool IsRunning
		{
			get
			{
				return mIsRunning;
			}
		}
		public bool WasCancelled
		{
			get
			{
				return mWasCancelled;
			}
		}
		public bool IsInitializing
		{
			get
			{
				return mIsInitializing;
			}
		}
		public bool IsCompleted
		{
			get
			{
				return mIsCompleted;
			}
		}

		public IResult ExecutionResult => mResult;

		public void Run()
		{
			Run(CancellationToken.None);
		}
		public void Run(CancellationToken cancellationToken)
		{
			var cancelException = false;

			mIsCompleted = false;
			mWasCancelled = false;
			mIsInitializing = false;
			mResult = Result.Success;
			mIsInitializing = true;

			Cancel();
			
			if (cancellationToken != CancellationToken.None)
			{
				mCancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			}
			else
			{
				mCancel = new CancellationTokenSource();
			}

			try
			{
				mCommandLine.Read(CommandLine, this);
				mResult = InitializeApplication();
				mIsInitializing = false;

				if (!mResult.HasError)
				{
					mIsRunning = true;
					mResult = Execute(mCancel.Token);
				}
			}
			catch (TaskCanceledException)
			{
				cancelException = true;
			}
			catch (OperationCanceledException)
			{
				cancelException = true;
			}
			catch (AggregateException exception)
			{
				mResult = Result.CreateError(exception);

				foreach (var inner in exception.InnerExceptions)
				{
					if (!RaiseExceptionEvent(inner))
					{
						throw;
					}
				}
			}
			catch (Exception exception)
			{
				mResult = Result.CreateError(exception);

				if (!RaiseExceptionEvent(exception))
				{
					throw;
				}
			}
			finally
			{
				try
				{
					mIsRunning = false;
					mIsInitializing = false;
					mIsCompleted = true;
					mWasCancelled = mCancel.IsCancellationRequested || cancelException;
					Cleanup(mWasCancelled);
				}
				finally
				{
					if (mWasCancelled)
					{
						mResult = Result.CreateError(new OperationCanceledException());
					}
				}

			}
		}
		public void Cancel()
		{
			if (!IsRunning || mCancel == null)
			{
				return;
			}

			CancellingOverride();

			mCancel?.Cancel();
			WaitHandler.Wait(() => !IsRunning);

			mCancel?.Dispose();
			mCancel = null;

			CancelOverride();
		}

		void IOperation.Wait()
		{
			WaitHandler.Wait(() => IsRunning || IsCompleted);
			WaitHandler.Wait(() => !IsRunning);
		}

		protected virtual IResult InitializeApplication() => Result.Success;
		protected abstract IResult Execute(CancellationToken cancellationToken);

		protected virtual void Cleanup(bool wasCancelled) { }
		protected virtual void CancellingOverride() { }
		protected virtual void CancelOverride() { }

		private bool RaiseExceptionEvent(Exception exception)
		{
			var args = new OperationUnhandledExceptionEventArgs(exception);
			ThreadException?.Invoke(this, args);

			if (!args.Handled)
			{
				args = new OperationUnhandledExceptionEventArgs(exception);

				if (!args.Handled)
				{
					return false;
				}
			}

			return true;
		}

		double IProgress.Progress => IsCompleted ? 1 : 0;

		bool IProgress.IsIdle => !IsRunning;
		bool IProgress.IsAborted => IsCompleted && (WasCancelled || ExecutionResult.HasError);
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public abstract class Operation : Resource, IOperation, INotifyProgressChanged
	{
		private readonly object mProgressLock = new object();
		private readonly object mOpeningLock = new object();
		private readonly object mClosingLock = new object();
		private double mProgress;

		private CancellationTokenSource mCancel;
		private OperationDispatchMode mDispatchMode;
		private Scope mScope;
		private IResult mResult;
		private IWaitHandler mWaitHandler;
		private bool mIsCompleted;
		private bool mIsInitializing;
		private bool mWasCancelled;

		protected Operation()
		{
			Scope = new ReadonlyScope(mScope = new Scope());
			mWaitHandler = new OperationWaitHandler();
		}

		public virtual string DisplayName => "Operation";

		public IReadonlyScope Scope { get; }
		public IWaitHandler WaitHandler
		{
			get { return mWaitHandler; }
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));
				mWaitHandler = value;
			}
		}

		public virtual OperationDispatchMode DispatchMode
		{
			get { return mDispatchMode; }
			set
			{
				if (mScope.IsInScope)
				{
					throw new InvalidOperationException("The dispatch mode can not be changed while the operation is running.");
				}

				mDispatchMode = value;
			}
		}

		public static event OperationUnhandledExceptionEventHandler GlobalThreadException;
		public event OperationUnhandledExceptionEventHandler ThreadException;

		public event EventHandler Started;
		public event EventHandler Ended;

		public void WaitForStarted()
		{
			WaitHandler.Wait(() => IsRunning || IsCompleted);
		}
		public void WaitForEnded()
		{
			WaitHandler.Wait(() => !IsRunning);
		}

		public void Run()
		{
			RunThread(new CancellationTokenSource());
		}
		public void Run(CancellationToken cancellationToken)
		{
			RunThread(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
		}
		public void Cancel()
		{
			if (!IsRunning || mCancel == null)
			{
				return;
			}

			CancellingOverride();

			mCancel?.Cancel();
			WaitForEnded();

			mCancel?.Dispose();
			mCancel = null;

			CancelOverride();
		}

		void IOperation.Wait()
		{
			if (DispatchMode == OperationDispatchMode.Synchronous)
			{
				return;
			}

			WaitForStarted();
			WaitForEnded();
		}

		public bool IsRunning
		{
			get
			{
				lock (mOpeningLock)
				lock (mClosingLock)
				{
					return Scope.IsInScope;
				}
			}
		}
		public bool WasCancelled
		{
			get
			{
				lock (mOpeningLock)
				lock (mClosingLock)
				{
					return mWasCancelled;
				}
			}
		}
		public bool IsInitializing
		{
			get
			{
				lock (mOpeningLock)
				lock (mClosingLock)
				{
					return mIsInitializing;
				}
			}
		}
		public bool IsCompleted
		{
			get
			{
				lock (mOpeningLock)
				lock (mClosingLock)
				{
					return mIsCompleted;
				}
			}
		}

		public IResult ExecutionResult => mResult;

		bool IProgress.IsIdle => !IsRunning;
		bool IProgress.IsAborted => IsCompleted && (WasCancelled || ExecutionResult.HasError);

		protected virtual IResult Initialize() => Result.Success;

		protected virtual void Cleanup(bool wasCancelled) { }
		protected virtual void CancellingOverride() { }
		protected virtual void CancelOverride() { }

		protected abstract IResult Execute(CancellationToken cancellationToken);

		private void RunThread(CancellationTokenSource cancellation)
		{
			lock (mOpeningLock)
			{
				mIsCompleted = false;
				mWasCancelled = false;
				mIsInitializing = false;
			}

			Cancel();
			mCancel = cancellation;

			switch (mDispatchMode)
			{
				case OperationDispatchMode.Synchronous:
					RunThread(mCancel.Token);
					break;
				case OperationDispatchMode.BackgroundThread:
					new Thread(RunThread) { IsBackground = true }.Start(mCancel.Token);
					WaitForStarted();
					break;
				case OperationDispatchMode.ThreadPoolUserWorkItem:
					ThreadPool.QueueUserWorkItem(RunThread, mCancel.Token);
					WaitForStarted();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		private void RunThread(object state)
		{
			var cancelException = false;
			lock (mOpeningLock)
			{
				mWasCancelled = false;
				mIsCompleted = false;
				mResult = Result.Success;
				mIsInitializing = true;
				SetProgress(0);
			}


			try
			{
				
				mResult = Initialize();

				lock (mOpeningLock)
				{
					mIsInitializing = false;
				}

				if (!mResult.HasError)
				{
					lock (mOpeningLock)
					{
						mIsInitializing = false;

						if (!mScope.IsInScope)
						{
							mScope.Enter();
						}
					}
					
					Started?.Invoke(this, new EventArgs());
					mResult = Execute((CancellationToken)state);
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
				lock (mClosingLock)
				{
					try
					{
						mIsInitializing = false;
						mIsCompleted = true;
						mWasCancelled = ((CancellationToken)state).IsCancellationRequested || cancelException;
						Cleanup(mWasCancelled);
					}
					finally
					{
						if (mWasCancelled)
						{
							mResult = Result.CreateError(new OperationCanceledException());
						}

						mScope.Leave();
						Ended?.Invoke(this, new EventArgs());
						SetProgress(mWasCancelled ? Progress : 1);
					}
				}
				
			}
		}
		private bool RaiseExceptionEvent(Exception exception)
		{
			var args = new OperationUnhandledExceptionEventArgs(exception);
			ThreadException?.Invoke(this, args);

			if (!args.Handled)
			{
				args = new OperationUnhandledExceptionEventArgs(exception);
				GlobalThreadException?.Invoke(this, args);

				if (!args.Handled)
				{
					return false;
				}
			}

			return true;
		}

		public double Progress
		{
			get
			{
				lock (mProgressLock)
				{
					return mProgress;
				}
			}
		}
		public event EventHandler ProgressChanged;

		protected void SetProgress(double progress)
		{
			if (progress < 0) progress = 0;
			if (progress > 1) progress = 1;

			lock (mProgressLock)
			{
				mProgress = Math.Max(mProgress, progress);
			}

			try
			{
				ProgressChanged?.Invoke(this, new EventArgs());
			}
			catch (Exception exception)
			{
				RaiseExceptionEvent(exception);
			}
		}
	}
}
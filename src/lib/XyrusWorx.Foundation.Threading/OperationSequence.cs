using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.Collections;
using XyrusWorx.Diagnostics;
using XyrusWorx.Structures;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class OperationSequence : Operation, IOperationSequence
	{
		private readonly object mLock = new object();
		private readonly ObjectDependencyGraph<IOperation> mGraph;

		private List<IOperation> mRunningOperations;
		private Dictionary<IOperation, ProgressInfo> mProgressInfos;
		private OperationSequenceSchedulingMode mSchedulingMode;

		private SequenceErrorBehavior mErrorBehavior;

		public OperationSequence()
		{
			mRunningOperations = new List<IOperation>();
			mProgressInfos = new Dictionary<IOperation, ProgressInfo>();
			mGraph = new ObjectDependencyGraph<IOperation>();
		}

		public void Append(IOperation operation)
		{
			if (operation == null)
			{
				throw new ArgumentNullException(nameof(operation));
			}

			if (IsRunning)
			{
				throw new InvalidOperationException("Adding an operation is not supported while the sequence is running.");
			}

			mGraph.Register(operation);
		}

		public OperationSequenceSchedulingMode SchedulingMode
		{
			get { return mSchedulingMode; }
			set
			{
				if (Scope.IsInScope)
				{
					throw new InvalidOperationException("The scheduling mode can not be changed while the sequence is running.");
				}

				mSchedulingMode = value;
			}
		}
		public SequenceErrorBehavior ErrorBehavior
		{
			get { return mErrorBehavior; }
			set
			{
				if (Scope.IsInScope)
				{
					throw new InvalidOperationException("The error behavior can not be changed while the sequence is running.");
				}

				mErrorBehavior = value;
			}
		}

		[CanBeNull]
		public ILogWriter Log { get; set; }

		[NotNull]
		public IEnumerable<IProgress> GetDetailProgress() => mProgressInfos.Values.Select(x => (IProgress)x);
		public ObjectDependencyGraphNode<IOperation> Operation(IOperation operation) => mGraph.Element(operation);

		public IEnumerator<IOperation> GetEnumerator()
		{
			IEnumerable<IOperation> arr;

			lock (mLock)
			{
				arr = mGraph.GetKnownElements();
			}

			return arr.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected virtual void StartOperation(IOperation operation) { }
		protected virtual void FinishOperation(IOperation operation, IResult result) { }

		protected sealed override void CancellingOverride()
		{
			lock (mLock)
			{
				mRunningOperations?.ForEach(x => x.Cancel());
			}
		}
		protected sealed override void CancelOverride()
		{
			lock (mLock)
			{
				mRunningOperations?.Clear();
			}
		}

		protected sealed override IResult Execute(CancellationToken cancellationToken)
		{
			var result = new MultiResult();

			mProgressInfos.Clear();

			foreach (var operation in mGraph.GetKnownElements())
			{
				mProgressInfos.Add(operation, new ProgressInfo
				{
					DisplayName = operation.DisplayName,
					IsInitializing = true,
					IsCompleted = false,
					IsAborted = false,
					IsIdle = false,
					Progress = 0
				});
			}

			SetProgress(0);

			var partitions = mGraph.GetPartitionsByDependencyDepth().ToArray();

			foreach (var partition in partitions)
			{
				MultiResult partitionResult;

				switch (SchedulingMode)
				{
					case OperationSequenceSchedulingMode.Sequential:
						partitionResult = ExecuteSequential(partition, cancellationToken);
						break;
					case OperationSequenceSchedulingMode.Parallel:
						partitionResult = ExecuteParallel(partition, cancellationToken);
						break;
					default:
						return Result.Success;
				}

				result.Results.AddRange(partitionResult.Results);
			}

			lock (mLock)
			{
				mRunningOperations?.Clear();
			}

			return result;
		}

		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		private IResult Execute(IOperation operation, Func<double, double> scaleProgress, CancellationToken cancellationToken)
		{
			IResult result = Result.Success;
			INotifyProgressChanged notify = null;

			var progressHandler = new EventHandler((o, e) =>
			{
				var co = (IOperation) o;

				mProgressInfos[operation] = new ProgressInfo
				{
					DisplayName = co.DisplayName,
					IsIdle = co.IsIdle,
					IsAborted = co.IsAborted,
					IsCompleted = co.IsCompleted,
					IsInitializing = co.IsInitializing,
					Progress = co.Progress
				};

				SetProgress(scaleProgress(operation.Progress));
			});
			try
			{
				lock (mLock)
				{
					mRunningOperations.Add(operation);
				}

				notify = operation as INotifyProgressChanged;

				if (notify != null)
				{
					notify.ProgressChanged += progressHandler;
				}

				StartOperation(operation);

				operation.Run();
				operation.Wait();

				var actionResponse = operation.ExecutionResult;
				if (actionResponse.HasError)
				{
					return actionResponse;
				}
			}
			catch (TaskCanceledException)
			{
				return Result.CreateError(new OperationCanceledException());
			}
			catch (OperationCanceledException)
			{
				return Result.CreateError(new OperationCanceledException());
			}
			catch (Exception exception)
			{
				return Result.CreateError(exception);
			}
			finally
			{
				lock (mLock)
				{
					if (operation != null)
					{
						mRunningOperations.Remove(operation);
						result = operation.ExecutionResult;
					}
				}

				if (notify != null)
				{
					notify.ProgressChanged -= progressHandler;
				}

				FinishOperation(operation, result);
			}

			return result;
		}

		private MultiResult ExecuteParallel(IEnumerable<IOperation> operations, CancellationToken cancellationToken)
		{
			var tasks = new List<Task>();
			var result = new MultiResult();

			var operationsDone = 0;
			var operationArray = operations.ToArray();
			var operationCount = operationArray.Length;

			foreach (var operation in operationArray)
			{
				// ReSharper disable once MethodSupportsCancellation
				tasks.Add(Task.Run(() =>
				{
					if (cancellationToken.IsCancellationRequested)
					{
						mProgressInfos[operation] = new ProgressInfo
						{
							DisplayName = operation.DisplayName,
							IsInitializing = false,
							IsCompleted = true,
							IsAborted = true,
							IsIdle = true,
							Progress = operation.Progress
						};

						return;
					}

					// ReSharper disable once AccessToModifiedClosure
					var operationResult = Execute(operation, c => operationsDone / (double)operationCount, cancellationToken);
					result.Results.Add(operationResult);

					if (operationResult.HasError && !operation.WasCancelled && ErrorBehavior > SequenceErrorBehavior.Ignore)
					{
						Log?.WriteError(operationResult.ErrorDescription);
					}

					mProgressInfos[operation] = new ProgressInfo
					{
						DisplayName = operation.DisplayName,
						IsInitializing = false,
						IsCompleted = true,
						IsAborted = operation.IsAborted,
						IsIdle = true,
						Progress = operation.Progress
					};

					operationsDone++;

					if (operation.WasCancelled || operationResult.HasError && ErrorBehavior == SequenceErrorBehavior.Abort)
					{
						foreach (var remainingOperation in operationArray.Where(x => x.WasCancelled || x.IsRunning))
						{
							remainingOperation.Cancel();
						}
					}
				}));
			}

			Task.WaitAll(tasks.ToArray(), cancellationToken);

			if (cancellationToken.IsCancellationRequested )
			{
				foreach (var operation in operationArray.Where(x => x.WasCancelled || x.IsRunning))
				{
					mProgressInfos[operation] = new ProgressInfo
					{
						DisplayName = operation.DisplayName,
						IsInitializing = false,
						IsCompleted = true,
						IsAborted = true,
						IsIdle = true,
						Progress = operation.Progress
					};
				}
			}

			return result;
		}
		private MultiResult ExecuteSequential(IEnumerable<IOperation> operations, CancellationToken cancellationToken)
		{
			var result = new MultiResult();

			var operationsDone = 0;
			var operationArray = operations.ToArray();
			var operationCount = operationArray.Length;

			foreach (var operation in operationArray)
			{
				SetProgress(operationsDone / (double)operationCount);

				// ReSharper disable once AccessToModifiedClosure
				var operationResult = Execute(operation, f => f/operationCount + (double) operationsDone/operationCount, cancellationToken);
				result.Results.Add(operationResult);

				if (operationResult.HasError && ErrorBehavior > SequenceErrorBehavior.Ignore)
				{
					Log?.WriteError(operationResult.ErrorDescription);
				}

				mProgressInfos[operation] = new ProgressInfo
				{
					DisplayName = operation.DisplayName,
					IsInitializing = false,
					IsCompleted = true,
					IsAborted = operation.IsAborted,
					IsIdle = true,
					Progress = operation.Progress
				};

				SetProgress((operationsDone + operation.Progress) / operationCount);
				operationsDone++;

				if (operation.WasCancelled || (ErrorBehavior == SequenceErrorBehavior.Abort && operationResult.HasError))
				{
					for (var i = operationsDone; i < operationCount; i++)
					{
						mProgressInfos[operationArray[i]] = new ProgressInfo
						{
							DisplayName = operationArray[i].DisplayName,
							IsInitializing = false,
							IsCompleted = true,
							IsAborted = true,
							IsIdle = true,
							Progress = operationArray[i].Progress
						};
					}

					break;
				}
			}

			return result;
		}

		IEnumerable<IProgress> IDetailProgress.Details => GetDetailProgress();
		struct ProgressInfo : IProgress
		{
			public string DisplayName { get; set; }
			public double Progress { get; set; }

			public bool IsInitializing { get; set; }
			public bool IsIdle { get; set; }
			public bool IsAborted { get; set; }
			public bool IsCompleted { get; set; }
		}
	}
}
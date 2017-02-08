using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public interface IOperation : IProgress
	{
		void Run();
		void Run(CancellationToken cancellationToken);

		void Cancel();
		void Wait();

		bool IsRunning { get; }
		bool WasCancelled { get; }

		[NotNull] IResult ExecutionResult { get; }
		[NotNull] IWaitHandler WaitHandler { get; set; }
		[NotNull] IReadonlyScope Scope { get; }
	}
}
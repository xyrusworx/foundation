using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class RelayOperation : Operation
	{
		private readonly Func<CancellationToken, IResult> mDelegate;
		private readonly string mDisplayName;

		public RelayOperation([NotNull] Action action, string displayName = null)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			mDisplayName = displayName;
			mDelegate = token =>
			{
				action();
				return Result.Success;
			};
		}
		public RelayOperation([NotNull] Action<CancellationToken> action, string displayName = null)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			mDisplayName = displayName;
			mDelegate = token =>
			{
				action(token);
				return Result.Success;
			};
		}
		public RelayOperation([NotNull] Func<IResult> func, string displayName = null)
		{
			if (func == null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			mDisplayName = displayName;
			mDelegate = token => func();
		}
		public RelayOperation([NotNull] Func<CancellationToken, IResult> func, string displayName = null)
		{
			if (func == null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			mDisplayName = displayName;
			mDelegate = func;
		}

		public Func<IResult> InitializationCallback { get; set; }
		public Action CleanupCallback { get; set; }

		public override string DisplayName => mDisplayName;

		protected override IResult Initialize()
		{
			return InitializationCallback?.Invoke() ?? Result.Success;
		}
		protected override IResult Execute(CancellationToken cancellationToken)
		{
			return mDelegate(cancellationToken);
		}
		protected override void Cleanup(bool wasCancelled)
		{
			CleanupCallback?.Invoke();
		}
	}
}
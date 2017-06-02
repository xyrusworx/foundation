using System;
using System.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Data
{
	[PublicAPI]
	public class DataTransaction : Resource
	{
		private Action mDisposeAction;
		private IDbTransaction mTransaction;

		internal DataTransaction([NotNull] IDbConnection connection, [NotNull] Action disposeAction, DataTransactionIsolation isolation)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));
			if (disposeAction == null) throw new ArgumentNullException(nameof(disposeAction));

			mTransaction = connection.BeginTransaction((IsolationLevel)(int)isolation);
			mDisposeAction = disposeAction;
		}

		public void Commit()
		{
			mTransaction.Commit();
		}
		public void Rollback()
		{
			mTransaction.Rollback();
		}

		internal IDbTransaction GetTransaction() => mTransaction;

		protected sealed override void DisposeOverride()
		{
			mDisposeAction?.Invoke();
			mDisposeAction = null;

			mTransaction?.Dispose();
			mTransaction = null;
		}
	}
}
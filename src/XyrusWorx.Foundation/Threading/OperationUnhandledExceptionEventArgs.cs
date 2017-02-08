using System;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public class OperationUnhandledExceptionEventArgs : EventArgs
	{
		public OperationUnhandledExceptionEventArgs(Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			Exception = exception;
		}

		public bool Handled { get; set; }
		public Exception Exception { get; }
	}
}
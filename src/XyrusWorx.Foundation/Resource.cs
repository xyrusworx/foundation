using JetBrains.Annotations;
using System;

namespace XyrusWorx
{
	[PublicAPI]
	public class Resource : IDisposable
	{
		private bool mIsDisposed;

		~Resource()
		{
			Dispose(false);
		}

		protected virtual void DisposeOverride() { }
		protected virtual void FinalizeOverride() { }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected bool IsDisposed => mIsDisposed;

		private void Dispose(bool disposing)
		{
			if (!mIsDisposed)
			{
				if (disposing)
				{
					try
					{
						DisposeOverride();
					}
					catch
					{
						// ignore
					}
				}

				try
				{
					FinalizeOverride();
				}
				catch
				{
					// ignore
				}

				mIsDisposed = true;
			}
		}
	}
}
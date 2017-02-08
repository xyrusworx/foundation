using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Data
{
	[PublicAPI]
	public class DataReader : Resource, IBulkReader
	{
		private IDataReader mReader;

		public DataReader([NotNull] IDataReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}

			mReader = reader;
		}
		public bool ThrowOnTypeMismatch { get; set; } = true;

		public IEnumerable<DataRecord> ReadAll()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(DataReader));
			}

			var counter = 0;

			while (mReader.Read())
			{
				yield return new DataRecord(mReader) { RowIndex = counter++, ThrowOnTypeMismatch = ThrowOnTypeMismatch };
			}
		}
		public int ReadAll(Action<DataRecord> callback, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(DataReader));
			}

			var counter = 0;

			while (mReader.Read())
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				callback(new DataRecord(mReader) { RowIndex = counter, ThrowOnTypeMismatch = ThrowOnTypeMismatch });
				counter++;
			}

			return counter;
		}

		protected sealed override void DisposeOverride()
		{
			mReader?.Dispose();
			mReader = null;
		}
	}
}
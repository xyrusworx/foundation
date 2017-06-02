using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Data
{
	[PublicAPI]
	public class ArrayReader : Resource, IBulkReader
	{
		private IEnumerable mData;

		public ArrayReader([NotNull] IEnumerable enumeration)
		{
			if (enumeration == null)
			{
				throw new ArgumentNullException(nameof(enumeration));
			}

			mData = enumeration;
		}
		public bool ThrowOnTypeMismatch { get; set; } = true;
		
		public IEnumerable<DataRecord> ReadAll()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(DataReader));
			}

			var counter = 0;

			foreach (var element in mData)
			{
				yield return new DataRecord(element) {RowIndex = counter, ThrowOnTypeMismatch = ThrowOnTypeMismatch};
				counter++;
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

			foreach (var element in mData)
			{
				callback(new DataRecord(element) { RowIndex = counter, ThrowOnTypeMismatch = ThrowOnTypeMismatch });
				counter++;
			}

			return counter;
		}

		protected sealed override void DisposeOverride()
		{
			mData = null;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Data
{
	[PublicAPI]
	public interface IBulkReader
	{
		TypeMismatchBehavior TypeMismatchBehavior { get; set; }
		FieldNotFoundBehavior FieldNotFoundBehavior { get; set; }

		[NotNull]
		IEnumerable<DataRecord> ReadAll();
		int ReadAll([NotNull] Action<DataRecord> callback, CancellationToken cancellationToken = default(CancellationToken));
	}
}
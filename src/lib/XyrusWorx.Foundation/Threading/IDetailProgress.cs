using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public interface IDetailProgress : IProgress
	{
		[NotNull]
		IEnumerable<IProgress> Details { get; }
	}
}
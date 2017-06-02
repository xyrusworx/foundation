using System.Collections;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public interface IReadonlyScope
	{
		ScopeEnterTrigger EnterTrigger { get; set; }
		ScopeLeaveTrigger LeaveTrigger { get; set; }

		[CanBeNull]
		object State { get; }
		bool IsInScope { get; }

		[NotNull]
		IEnumerable Stack { get; }
	}
}
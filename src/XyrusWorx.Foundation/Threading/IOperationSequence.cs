using System.Collections.Generic;
using JetBrains.Annotations;
using XyrusWorx.Structures;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public interface IOperationSequence : IOperation, IEnumerable<IOperation>, IDetailProgress
	{
		void Append([NotNull] IOperation operation);

		[NotNull]
		ObjectDependencyGraphNode<IOperation> Operation([NotNull] IOperation operation);
	}
}
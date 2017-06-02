using System;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public interface IScope : IReadonlyScope, IDisposable
	{
		IScope Enter(object state = null);
		IScope Leave();
	}
}
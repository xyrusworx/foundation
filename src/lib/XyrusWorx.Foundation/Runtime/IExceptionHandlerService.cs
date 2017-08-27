using System;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public interface IExceptionHandlerService
	{
		bool HandleException([NotNull] Exception exception);
	}
}
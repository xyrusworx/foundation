using System;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface IExceptionHandlerService
	{
		bool HandleException([NotNull] Exception exception);
	}
}
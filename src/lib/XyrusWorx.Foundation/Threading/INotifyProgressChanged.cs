using System;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public interface INotifyProgressChanged
	{
		event EventHandler ProgressChanged;
	}
}
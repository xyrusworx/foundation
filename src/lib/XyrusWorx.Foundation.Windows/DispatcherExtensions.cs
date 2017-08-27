using System;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Windows.Runtime;

namespace XyrusWorx.Windows 
{
	[PublicAPI]
	public static class DispatcherExtensions
	{
		public static IWindowsApplicationHost Start([NotNull] this Dispatcher dispatcher, [NotNull] WpfApplication application)
		{
			if (dispatcher == null)
			{
				throw new ArgumentNullException(nameof(dispatcher));
			}
			
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			return new WindowsApplicationHost(dispatcher, application);
		}
	}
}
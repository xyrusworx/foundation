using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using XyrusWorx.Windows.ViewModels;

namespace XyrusWorx.Windows.Runtime 
{
	[PublicAPI]
	public interface IApplicationHost 
	{
		[CanBeNull]
		ViewModel ViewModel { get; }
		
		[CanBeNull]
		FrameworkElement View { get; }
		
		[NotNull]
		XyrusWorx.Runtime.Application Application { get; }
		
		[CanBeNull]
		Dispatcher GetDispatcher();

		[ContractAnnotation("=> halt")]
		void Shutdown(int exitCode = 0);
	}
}
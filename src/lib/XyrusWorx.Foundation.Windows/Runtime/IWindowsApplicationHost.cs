using System.Windows;
using JetBrains.Annotations;
using XyrusWorx.Runtime;
using XyrusWorx.Windows.ViewModels;

namespace XyrusWorx.Windows.Runtime 
{
	[PublicAPI]
	public interface IWindowsApplicationHost : IApplicationHost
	{
		[CanBeNull]
		ViewModel ViewModel { get; }
		
		[CanBeNull]
		FrameworkElement View { get; }
	}
}
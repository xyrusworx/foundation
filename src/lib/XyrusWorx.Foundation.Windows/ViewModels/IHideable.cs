using JetBrains.Annotations;

namespace XyrusWorx.Windows.ViewModels
{
	[PublicAPI]
	public interface IHideable
	{
		bool IsVisible { get; set; }
	}
}
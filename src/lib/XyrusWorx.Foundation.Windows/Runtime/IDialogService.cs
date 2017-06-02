using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface IDialogService
	{
		[NotNull]
		IMessageBox CreateDialog();
	}
}
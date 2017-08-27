using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public interface IDialogService
	{
		[NotNull]
		IMessageBox CreateDialog();
	}
}
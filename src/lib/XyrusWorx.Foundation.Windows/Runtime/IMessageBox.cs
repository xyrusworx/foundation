using System.Windows;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface IMessageBox
	{
		[NotNull]
		IMessageBox Title(string title);

		[NotNull]
		IMessageBox Message(string message);

		[NotNull]
		IMessageBox Notice();

		[NotNull]
		IMessageBox Notice(string message);

		[NotNull]
		IMessageBox Warning();

		[NotNull]
		IMessageBox Warning(string message);

		[NotNull]
		IMessageBox Error();

		[NotNull]
		IMessageBox Error(string message);

		[NotNull]
		IMessageBox Owner([NotNull] Window window);

		void Display();
		bool Ask();
		bool? AskOrCancel();

		[NotNull]
		IAsyncMessageBox Async { get; }
	}
}
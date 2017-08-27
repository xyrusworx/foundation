using System;
using System.Windows;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface IOpenFolderDialog
	{
		[NotNull]
		IOpenFolderDialog Prompt(string prompt);

		[NotNull]
		IOpenFolderDialog InitialSelection(string path);

		[NotNull]
		IOpenFolderDialog RootFolder(Environment.SpecialFolder specialFolder);

		[NotNull]
		IOpenFolderDialog Owner(Window window);

		[NotNull]
		IAsyncOpenFolderDialog Async { get; }

		[NotNull]
		Result<string> Ask();
	}
}
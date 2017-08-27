using System;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
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
		IOpenFolderDialog Owner(object view);

		[NotNull]
		IAsyncOpenFolderDialog Async { get; }

		[NotNull]
		Result<string> Ask();
	}
}
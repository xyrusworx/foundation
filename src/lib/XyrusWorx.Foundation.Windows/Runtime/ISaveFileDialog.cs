using System.Windows;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface ISaveFileDialog
	{
		[NotNull]
		ISaveFileDialog Title(string title);

		[NotNull]
		ISaveFileDialog InitialDirectory(string path);

		[NotNull]
		ISaveFileDialog Format([NotNull] string pattern, string displayName);

		[NotNull]
		ISaveFileDialog Owner(Window window);

		[NotNull]
		IAsyncSaveFileDialog Async { get; }

		[NotNull]
		Result<string> Ask();
	}
}
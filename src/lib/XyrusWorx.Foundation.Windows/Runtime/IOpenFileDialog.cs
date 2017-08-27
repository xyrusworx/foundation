using System.Windows;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface IOpenFileDialog
	{
		[NotNull]
		IOpenFileDialog Title(string title);

		[NotNull]
		IOpenFileDialog InitialDirectory(string path);

		[NotNull]
		IOpenFileDialog Format([NotNull] string pattern, string displayName);

		[NotNull]
		IOpenFileDialog Owner(Window window);

		[NotNull]
		IAsyncOpenFileDialog Async { get; }

		[NotNull]
		Result<string> Ask();
	}
}
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
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
		IOpenFileDialog Owner(object view);

		[NotNull]
		IAsyncOpenFileDialog Async { get; }

		[NotNull]
		Result<string> Ask();
	}
}
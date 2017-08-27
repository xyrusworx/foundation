using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface IAsyncOpenFileDialog
	{
		[NotNull]
		Task<Result<string>> Ask();
	}
}
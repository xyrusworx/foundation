using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public interface IAsyncSaveFileDialog
	{
		[NotNull]
		Task<Result<string>> Ask();
	}
}
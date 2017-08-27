using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public interface IAsyncOpenFolderDialog
	{
		[NotNull]
		Task<Result<string>> Ask();
	}
}
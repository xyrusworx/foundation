using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public interface IAsyncMessageBox
	{
		Task Display();
		Task<bool> Ask();
		Task<bool?> AskOrCancel();
	}
}
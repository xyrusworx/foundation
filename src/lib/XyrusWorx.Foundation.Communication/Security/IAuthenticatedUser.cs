using JetBrains.Annotations;
using XyrusWorx.Communication.Provider;

namespace XyrusWorx.Communication.Security
{
	[PublicAPI]
	public interface IAuthenticatedUser
	{
		string Name { get; }
		WebService Context { get; }
	}
}
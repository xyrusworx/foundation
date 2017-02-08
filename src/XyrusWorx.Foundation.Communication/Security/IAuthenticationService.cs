using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using XyrusWorx.Communication.Provider;

namespace XyrusWorx.Communication.Security
{
	[PublicAPI]
	public interface IAuthenticationService
	{
		Task<IAuthenticatedUser> GetUserFromRequestAsync([NotNull] WebService context, [NotNull] HttpRequest request, [NotNull] ClaimsPrincipal user);
	}
}
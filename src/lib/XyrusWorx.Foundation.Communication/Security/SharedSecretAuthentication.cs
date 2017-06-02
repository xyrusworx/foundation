using System;
using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using XyrusWorx.Communication.Provider;

namespace XyrusWorx.Communication.Security
{
	[PublicAPI]
	public class SharedSecretAuthentication : IAuthenticationService
	{
		private readonly string mSecret;
		private readonly string mHeaderName;

		public SharedSecretAuthentication([NotNull] string secret, [NotNull] string headerName = "X-Secret")
		{
			if (secret == null) throw new ArgumentNullException(nameof(secret));
			if (headerName == null) throw new ArgumentNullException(nameof(headerName));

			mSecret = secret;
			mHeaderName = headerName;
		}

		public Task<IAuthenticatedUser> GetUserFromRequestAsync(WebService context, HttpRequest request, ClaimsPrincipal user)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			if (!request.Headers.ContainsKey(mHeaderName))
			{
				return Task.FromResult<IAuthenticatedUser>(null);
			}

			var headerValue = string.Join(" ", request.Headers[mHeaderName].ToArray());
			if (string.Equals(headerValue, mSecret))
			{
				return Task.FromResult<IAuthenticatedUser>(new AnonymousUser(context));
			}

			return Task.FromResult<IAuthenticatedUser>(null);
		}
	}
}
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using XyrusWorx.Communication.Provider;

namespace XyrusWorx.Communication.Security
{
	[PublicAPI]
	public class BasicHttpAuthentication : IAuthenticationService
	{
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

			const string scheme = "BASIC";
			var authorizationHeader = request.Headers["Authorization"].ToString();

			if (string.IsNullOrEmpty(authorizationHeader))
			{
				return Task.FromResult<IAuthenticatedUser>(null);
			}

			if (!authorizationHeader.StartsWith(scheme + ' ', StringComparison.OrdinalIgnoreCase))
			{
				return Task.FromResult<IAuthenticatedUser>(new AnonymousUser(context));
			}

			var encodedCredentials = authorizationHeader.Substring(scheme.Length).Trim();

			if (string.IsNullOrEmpty(encodedCredentials))
			{
				return Task.FromResult<IAuthenticatedUser>(null);
			}

			string decodedCredentials;

			try
			{
				decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
			}
			catch
			{
				return Task.FromResult<IAuthenticatedUser>(null);
			}

			var delimiterIndex = decodedCredentials.IndexOf(':');
			if (delimiterIndex == -1)
			{
				return Task.FromResult<IAuthenticatedUser>(null);
			}

			var username = decodedCredentials.Substring(0, delimiterIndex);
			var password = decodedCredentials.Substring(delimiterIndex + 1);

			return Task.FromResult<IAuthenticatedUser>(new BasicUser(context, username, password));
		}
	}
}
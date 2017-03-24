using System;
using System.Net;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Client.Security
{
	[PublicAPI]
	public class SharedSecretClientAuthentication : ServiceClientAuthentication
	{
		private readonly string mSecret;
		private readonly string mHeaderName;

		public SharedSecretClientAuthentication([NotNull] string secret, [NotNull] string headerName = "X-Secret")
		{
			if (secret == null) throw new ArgumentNullException(nameof(secret));
			if (headerName == null) throw new ArgumentNullException(nameof(headerName));

			mSecret = secret;
			mHeaderName = headerName;
		}

		protected override void WriteRequest(WebRequest request)
		{
			request.Headers[mHeaderName] = mSecret;
		}
	}
}
using System;
using System.Net;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.Foundation.Communication.Client.Security
{
	[PublicAPI]
	public class BasicHttpClientAuthentication : ServiceClientAuthentication
	{
		private readonly NetworkCredential mCredentials;

		public BasicHttpClientAuthentication([NotNull] NetworkCredential credentials)
		{
			if (credentials == null)
			{
				throw new ArgumentNullException(nameof(credentials));
			}

			mCredentials = credentials;
		}

		protected override void WriteRequest(WebRequest request)
		{
			var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(mCredentials.UserName + ":" + mCredentials.Password));

			request.Headers["Authorization"] = "Basic " + encoded;
			request.Credentials = mCredentials;
		}
	}
}
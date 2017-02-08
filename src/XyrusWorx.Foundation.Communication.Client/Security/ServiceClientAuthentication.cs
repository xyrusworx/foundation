using System;
using System.Net;
using JetBrains.Annotations;

namespace XyrusWorx.Foundation.Communication.Client.Security
{
	[PublicAPI]
	public abstract class ServiceClientAuthentication
	{
		internal void Configure([NotNull] WebRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			WriteRequest(request);
		}

		protected abstract void WriteRequest([NotNull] WebRequest request);
	}
}
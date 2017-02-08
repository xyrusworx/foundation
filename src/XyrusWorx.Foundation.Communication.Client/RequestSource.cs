using System;
using JetBrains.Annotations;

namespace XyrusWorx.Foundation.Communication.Client
{
	[PublicAPI]
	public class RequestSource : Resource
	{
		private readonly WebServiceClient mClient;

		public RequestSource(WebServiceClient client)
		{
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			mClient = client;
		}

		[NotNull] public RequestBuilder Get([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Get);
		[NotNull] public RequestBuilder Post([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Post);
		[NotNull] public RequestBuilder Put([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Put);
		[NotNull] public RequestBuilder Delete([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Delete);
		[NotNull] public RequestBuilder Request([NotNull] string requestPath, RequestVerb verb) => Request(requestPath).Verb(verb);

		[NotNull]
		protected virtual RequestBuilder ConfigureRequest([NotNull] RequestBuilder request)
		{
			return request;
		}

		private RequestBuilder Request([NotNull] string requestPath)
		{
			if (requestPath.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(requestPath));
			}

			var builder =  new RequestBuilder(mClient, $"/api/{requestPath.TrimStart('/')}");

			// ReSharper disable once ConstantNullCoalescingCondition
			return ConfigureRequest(builder) ?? builder;
		}
	}
}
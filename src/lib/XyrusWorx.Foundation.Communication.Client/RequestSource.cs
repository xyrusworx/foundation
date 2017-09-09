using System;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Client
{
	[PublicAPI]
	public class RequestSource : Resource
	{
		private readonly WebServiceClient mClient;
		private readonly string mNode;

		public RequestSource(WebServiceClient client, string node = "api")
		{
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			mClient = client;
			mNode = node;
		}

		[NotNull] public RequestBuilder Get([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Get);
		[NotNull] public RequestBuilder Post([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Post);
		[NotNull] public RequestBuilder Put([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Put);
		[NotNull] public RequestBuilder Delete([NotNull] string requestPath) => Request(requestPath).Verb(RequestVerb.Delete);
		[NotNull] public RequestBuilder Request([NotNull] string requestPath, RequestVerb verb) => Request(requestPath).Verb(verb);

		[NotNull]
		protected virtual RequestBuilder ConfigureRequest([NotNull] RequestBuilder request) => request;

		private RequestBuilder Request([NotNull] string requestPath)
		{
			if (requestPath.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(requestPath));
			}

			var path = new[]{mNode?.Trim('/'), requestPath.TrimStart('/')}.Concat("/");
			var builder =  new RequestBuilder(mClient, $"/{path}");

			// ReSharper disable once ConstantNullCoalescingCondition
			return ConfigureRequest(builder) ?? builder;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.Foundation.Communication.Client.Security;
using XyrusWorx.IO;

namespace XyrusWorx.Foundation.Communication.Client
{
	[PublicAPI]
	public class RequestBuilder
	{
		private readonly WebServiceClient mClient;
		private readonly string mRequestPath;
		private readonly IKeyValueStore<string> mHeaders;
		private readonly IKeyValueStore<object> mParameters;
		private readonly List<Tuple<Func<IWebResult, bool>, Action<WebServiceClientResponse>>> mCallbacks;
		private readonly List<ServiceClientAuthentication> mAuthentications;
		private RequestVerb mVerb;
		private object mBody;

		internal RequestBuilder([NotNull] WebServiceClient client, [NotNull] string requestPath)
		{
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			if (requestPath.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(requestPath));
			}

			mHeaders = new MemoryKeyValueStore<string>();
			mParameters = new MemoryKeyValueStore();
			mAuthentications = new List<ServiceClientAuthentication>();
			mCallbacks = new List<Tuple<Func<IWebResult, bool>, Action<WebServiceClientResponse>>>();
			mClient = client;
			mRequestPath = requestPath;
			mVerb = RequestVerb.Get;
		}

		[NotNull]
		public RequestBuilder Reset()
		{
			mCallbacks.Clear();
			mParameters.Clear();
			mAuthentications.Clear();
			mHeaders.Clear();
			mVerb = RequestVerb.Get;
			mBody = null;

			return this;
		}

		[NotNull]
		public RequestBuilder Verb(RequestVerb verb)
		{
			mVerb = verb;
			return this;
		}

		[NotNull]
		public RequestBuilder Header(StringKey key, [CanBeNull] string value)
		{
			if (value == null)
			{
				mHeaders.Remove(key);
				return this;
			}

			mHeaders.Write(key, value);
			return this;
		}

		[NotNull]
		public RequestBuilder Parameter<T>(StringKey key, [CanBeNull] T value)
		{
			if (value == null)
			{
				mParameters.Remove(key);
				return this;
			}

			mParameters.Write(key, value);
			return this;
		}

		[NotNull]
		public RequestBuilder Body<T>([CanBeNull] T body)
		{
			mBody = body;
			return this;
		}

		[NotNull]
		public RequestBuilder Callback([NotNull] Action<WebServiceClientResponse> what)
		{
			if (what == null)
			{
				throw new ArgumentNullException(nameof(what));
			}

			mCallbacks.Add(new Tuple<Func<IWebResult, bool>, Action<WebServiceClientResponse>>(n => true, what));
			return this;
		}

		[NotNull]
		public RequestBuilder Callback([NotNull] Func<IWebResult, bool> when, [NotNull] Action<WebServiceClientResponse> what)
		{
			if (when == null)
			{
				throw new ArgumentNullException(nameof(when));
			}

			if (what == null)
			{
				throw new ArgumentNullException(nameof(what));
			}

			mCallbacks.Add(new Tuple<Func<IWebResult, bool>, Action<WebServiceClientResponse>>(when, what));
			return this;
		}

		[NotNull]
		public RequestBuilder Authentication([NotNull] ServiceClientAuthentication authentication)
		{
			if (authentication == null)
			{
				throw new ArgumentNullException(nameof(authentication));
			}

			mAuthentications.Add(authentication);
			return this;
		}

		[NotNull]
		public WebServiceClientResponse Send(CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = mClient.CreateRequest(mVerb, mRequestPath, mParameters);

			SetAuthentication(request);
			SetHeaders(request);

			try
			{
				if (HasBody())
				{
					request.WriteBody(mBody);
				}

				var result = request.Invoke(cancellationToken);

				RunCallbacks(result);

				return result;
			}
			catch (Exception exception)
			{
				return WebServiceClientResponse.FromError(exception);
			}
		}

		[NotNull]
		public async Task<WebServiceClientResponse> SendAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = mClient.CreateRequest(mVerb, mRequestPath, mParameters);

			SetAuthentication(request);
			SetHeaders(request);

			try
			{
				if (HasBody())
				{
					await request.WriteBodyAsync(mBody);
				}

				var result = await request.InvokeAsync(cancellationToken);

				RunCallbacks(result);

				return result;
			}
			catch (Exception exception)
			{
				return WebServiceClientResponse.FromError(exception);
			}
		}

		public string RequestPath => mRequestPath;
		public RequestVerb RequestVerb => mVerb;

		private bool HasBody()
		{
			return mBody != null && mVerb != RequestVerb.Get;
		}
		private void SetAuthentication(WebServiceClientRequest request)
		{
			if (!mAuthentications.Any())
			{
				request.Authenticate();
			}
			else
			{
				request.Authenticate(mAuthentications.ToArray());
			}
		}
		private void SetHeaders(WebServiceClientRequest request)
		{
			foreach (var headerKey in mHeaders.Keys)
			{
				request.WriteHeader(headerKey, mHeaders[headerKey]);
			}
		}
		private void RunCallbacks(WebServiceClientResponse response)
		{
			foreach (var callback in mCallbacks)
			{
				var condition = callback.Item1(response);
				if (condition)
				{
					callback.Item2(response);
				}
			}
		}
	}
}
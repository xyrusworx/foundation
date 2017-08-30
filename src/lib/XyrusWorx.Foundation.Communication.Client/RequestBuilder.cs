using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.Communication.Client.Security;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Client
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
		private readonly List<Func<RequestInterceptorContext, Task>> mInterceptors;
		private RequestVerb mVerb;
		private bool mUseBodyString;
		private object mBody;
		private string mBodyString;

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
			mInterceptors = new List<Func<RequestInterceptorContext, Task>>();
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
			mUseBodyString = false;
			return this;
		}
		
		[NotNull]
		public RequestBuilder Body([CanBeNull] string body)
		{
			mBodyString = body;
			mUseBodyString = true;
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
		public RequestBuilder Interceptor([NotNull] Func<RequestInterceptorContext, Task> callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}
			
			mInterceptors.Add(callback);
			return this;
		} 

		[NotNull]
		public WebServiceClientResponse Send(CancellationToken cancellationToken = default(CancellationToken))
		{
			var ic = new RequestInterceptorContext(this);
			foreach (var interceptor in mInterceptors)
			{
				var task = interceptor(ic);
				try
				{
					task.Wait(cancellationToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
			
			if (ic.InterceptorResult.HasError)
			{
				return WebServiceClientResponse.FromError(new Exception(ic.InterceptorResult.ErrorDescription.NormalizeNull() ?? "An unknown error occured when preparing the request."));
			}
			
			var request = mClient.CreateRequest(mVerb, mRequestPath, mParameters);

			SetAuthentication(request);
			SetHeaders(request);

			try
			{
				if (HasBody())
				{
					if (mUseBodyString)
					{
						request.WriteBodyString(mBodyString);
					}
					else
					{
						request.WriteBody(mBody);
					}
				}
				
				var result = request.Invoke(cancellationToken);
				if (cancellationToken.IsCancellationRequested)
				{
					return result;
				}
				
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
			var ic = new RequestInterceptorContext(this);
			foreach (var interceptor in mInterceptors)
			{
				await interceptor(ic);
			}

			if (ic.InterceptorResult.HasError)
			{
				return WebServiceClientResponse.FromError(new Exception(ic.InterceptorResult.ErrorDescription.NormalizeNull() ?? "An unknown error occured when preparing the request."));
			}
			
			var request = mClient.CreateRequest(mVerb, mRequestPath, mParameters);

			SetAuthentication(request);
			SetHeaders(request);

			try
			{
				if (HasBody())
				{
					if (mUseBodyString)
					{
						await request.WriteBodyStringAsync(mBodyString);
					}
					else
					{
						await request.WriteBodyAsync(mBody);
					}
				}

				var result = await request.InvokeAsync(cancellationToken);
				if (cancellationToken.IsCancellationRequested)
				{
					return result;
				}

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

		internal IKeyValueStore<string> GetHeaders() => mHeaders;
		internal IKeyValueStore<object> GetParameters() => mParameters;

		internal Uri GetRequestUri() => mClient.CreateRequestUri(mVerb, mRequestPath, mParameters);
		
		internal string GetVerb() => mVerb.ToString().ToUpperInvariant();
		internal string GetBodyString()
		{
			if (mUseBodyString)
			{
				return mBodyString;
			}
			
			var comm = mClient.Configuration.CommunicationStrategy ?? new JsonCommunicationStrategy();
			var enc = mClient.Configuration.Encoding ?? Encoding.UTF8;

			using (var stream = new MemoryStream())
			{
				if (mBody != null)
				{
					comm.WriteAsync(stream, enc, mBody);
				}

				stream.Seek(0, SeekOrigin.Begin);
				return enc.GetString(stream.ToArray());
			}

		}

		private bool HasBody()
		{
			if (mUseBodyString)
			{
				return !string.IsNullOrEmpty(mBodyString);
			}
			
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
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.Communication.Client.Security;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Client
{
	[PublicAPI]
	public class WebServiceClientRequest
	{
		private readonly ServiceClientConfiguration mConfiguration;
		private readonly CommunicationStrategy mCommunicationStrategy;
		private readonly HttpWebRequest mRequest;

		internal WebServiceClientRequest([NotNull] ServiceClientConfiguration configuration, [NotNull] CommunicationStrategy communicationStrategy, [NotNull] HttpWebRequest request)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			if (communicationStrategy == null) throw new ArgumentNullException(nameof(communicationStrategy));
			if (request == null) throw new ArgumentNullException(nameof(request));

			mConfiguration = configuration;
			mCommunicationStrategy = communicationStrategy;
			mRequest = request;
		}

		public void Authenticate()
		{
			foreach (var measure in mConfiguration.AuthenticationMeasures)
			{
				measure.Configure(mRequest);
			}
		}
		public void Authenticate(params ServiceClientAuthentication[] authenticationMeasures)
		{
			Authenticate();

			foreach (var measure in authenticationMeasures)
			{
				measure.Configure(mRequest);
			}
		}

		public void WriteHeader([NotNull] string headerName, string data)
		{
			if (headerName.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(headerName));
			}

			if (data.NormalizeNull() == null)
			{
				mRequest.Headers.Remove(headerName);
				return;
			}

			mRequest.Headers[headerName] = data;
		}

		public void WriteBody(object data)
		{
			var task = WriteBodyAsync(data);

			task.Wait();
		}

		[NotNull]
		public async Task WriteBodyAsync(object data)
		{
			using (var stream = await mRequest.GetRequestStreamAsync())
			{
				await mCommunicationStrategy.WriteAsync(stream, mConfiguration.Encoding ?? Encoding.UTF8, data);
			}
		}

		[NotNull]
		public WebServiceClientResponse Invoke()
		{
			return Invoke(CancellationToken.None);
		}

		[NotNull]
		public WebServiceClientResponse Invoke(CancellationToken cancellationToken)
		{
			var task = InvokeAsync(cancellationToken);

			task.Wait(cancellationToken);

			if (cancellationToken.IsCancellationRequested)
			{
				return new WebServiceClientResponse(new OperationCanceledException(), 900);
			}

			return task.Result;
		}

		[NotNull]
		public Task<WebServiceClientResponse> InvokeAsync()
		{
			return InvokeAsync(CancellationToken.None);
		}

		[NotNull]
		public async Task<WebServiceClientResponse> InvokeAsync(CancellationToken cancellationToken)
		{
			try
			{
				var response = await mRequest.GetResponseAsync();
				if (cancellationToken.IsCancellationRequested)
				{
					return new WebServiceClientResponse(new OperationCanceledException(), 900);
				}

				return await FromResponseAsync(response, Result.Success, cancellationToken);
			}
			catch (WebException webException)
			{
				return await FromResponseAsync(webException.Response, Result.CreateError(webException.Message), cancellationToken);
			}
			catch (Exception exception)
			{
				return new WebServiceClientResponse(exception, 901);
			}
		}

		private async Task<WebServiceClientResponse> FromResponseAsync(WebResponse response, IResult invokeResult, CancellationToken cancellationToken)
		{
			byte[] data;

			var responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				data = new byte[0];
			}
			else
			{
				using (responseStream)
				using (var memoryStream = new MemoryStream())
				{
					await responseStream.CopyToAsync(memoryStream, 81920, cancellationToken);

					if (cancellationToken.IsCancellationRequested)
					{
						return new WebServiceClientResponse(new OperationCanceledException(), 900);
					}

					memoryStream.Seek(0, SeekOrigin.Begin);

					data = memoryStream.ToArray();
				}
			}

			var headers = new MemoryKeyValueStore<string>();

			foreach (var headerKey in response.Headers.AllKeys)
			{
				headers[headerKey] = response.Headers[headerKey];
			}

			var http = response as HttpWebResponse;
			var result = new WebServiceClientResponse(mConfiguration, mCommunicationStrategy, data, headers, invokeResult, (int)(http?.StatusCode ?? HttpStatusCode.OK));

			return result;
		}
	}
}
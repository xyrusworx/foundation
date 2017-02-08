using System;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Foundation.Communication.Client
{
	[PublicAPI]
	public class WebServiceClient
	{
		private readonly ServiceClientConfiguration mConfiguration;

		public WebServiceClient([NotNull] ServiceClientConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			mConfiguration = configuration;
		}

		[NotNull]
		public Uri BaseUri => new Uri($"{(mConfiguration.TransportLevelSecurity ? "https" : "http")}://{mConfiguration.Hostname}:{mConfiguration.Port}");

		[NotNull]
		public ServiceClientConfiguration Configuration => mConfiguration;

		[NotNull]
		public WebServiceClientRequest CreateRequest(RequestVerb verb, string relativeUri, IKeyValueStore<object> queryParameters = null)
		{
			var uriString = new Uri(BaseUri, relativeUri ?? string.Empty).ToString();
			if (uriString.Contains("?"))
			{
				throw new ArgumentException($"The relative URI can't contain a query string. Please use \"{nameof(queryParameters)}\" or escape any '?'-characters");
			}

			if (queryParameters != null)
			{
				var separatorChar = '?';

				foreach (var key in queryParameters.Keys)
				{
					var escapedValue = queryParameters[key]?.ToString().NormalizeNull().TryTransform(Uri.EscapeUriString);
					var tokenString = $"{separatorChar}{key}={escapedValue}";

					uriString += tokenString;
					separatorChar = '&';
				}
			}

			var innerRequest = WebRequest.CreateHttp(new Uri(uriString));

			innerRequest.Method = verb.ToString().ToUpper();
			innerRequest.Accept = CommunicationStrategy.ContentType;
			innerRequest.ContentType = $"{CommunicationStrategy.ContentType}; charset={(mConfiguration.Encoding ?? Encoding.UTF8).WebName}";

			var request = new WebServiceClientRequest(mConfiguration, CommunicationStrategy, innerRequest);

			return request;
		}

		private CommunicationStrategy CommunicationStrategy => mConfiguration.CommunicationStrategy ?? CommunicationStrategy.Default;
	}
}

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Client
{
	[PublicAPI]
	public class WebServiceClientResponse : Resource, IWebResult
	{
		private readonly ServiceClientConfiguration mConfiguration;
		private readonly CommunicationStrategy mCommunicationStrategy;

		private readonly byte[] mResponse;
		private readonly IKeyValueStore<string> mHeaders;

		private WebServiceClientResponse() { }

		internal WebServiceClientResponse([NotNull] Exception exception, int statusCode)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			HasError = true;
			ErrorDescription = exception.GetOriginalMessage();
			StatusCode = statusCode;
		}
		internal WebServiceClientResponse(
			[NotNull] ServiceClientConfiguration configuration, 
			[NotNull] CommunicationStrategy communicationStrategy, 
			[NotNull] byte[] data, 
			[NotNull] IKeyValueStore<string> headers, 
			[NotNull] IResult result, int statusCode)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			if (communicationStrategy == null) throw new ArgumentNullException(nameof(communicationStrategy));
			if (data == null) throw new ArgumentNullException(nameof(data));
			if (headers == null) throw new ArgumentNullException(nameof(headers));
			if (result == null) throw new ArgumentNullException(nameof(result));

			mConfiguration = configuration;
			mCommunicationStrategy = communicationStrategy;
			mResponse = data;
			mHeaders = headers;

			HasError = result.HasError;
			ErrorDescription = result.ErrorDescription;
			StatusCode = statusCode;
		}

		[NotNull]
		public static WebServiceClientResponse FromError([NotNull] Exception exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			var instance = new WebServiceClientResponse();

			instance.HasError = true;
			instance.ErrorDescription = exception.GetOriginalMessage();

			return instance;
		}

		public bool HasError { get; private set; }
		public string ErrorDescription { get; private set; }
		public int StatusCode { get; private set; }

		[CanBeNull]
		public string ReadBody()
		{
			var task = ReadBodyAsync();

			task.Wait();

			return task.Result;
		}

		[CanBeNull]
		public T ReadBody<T>()
		{
			var task = ReadBodyAsync<T>();

			task.Wait();

			return task.Result;
		}

		[NotNull]
		public Task<string> ReadBodyAsync()
		{
			if (mResponse == null)
			{
				return Task.FromResult<string>(null);
			}

			var result = GetEncoding().GetString(mResponse);

			return Task.FromResult(result);
		}

		[NotNull]
		public async Task<T> ReadBodyAsync<T>()
		{
			if (mResponse == null)
			{
				return default(T);
			}

			using (var stream = new MemoryStream(mResponse))
			{
				return (T)(await mCommunicationStrategy.ReadAsync(stream, GetEncoding(), typeof(T)));
			}
		}

		private Encoding GetEncoding()
		{
			var inputEncoding = mConfiguration.Encoding ?? Encoding.UTF8;
			var encodingMatch = new Regex(@"^.*?/.*?;\s*charset=(.*?)$", RegexOptions.IgnoreCase).Match(mHeaders["Content-Type"] ?? string.Empty);

			if (encodingMatch.Success)
			{
				var ianaName = encodingMatch.Groups[1].Value;
				try
				{
					inputEncoding = Encoding.GetEncoding(ianaName);
				}
				catch
				{
					// ok...
				}
			}

			return inputEncoding;
		}
	}
}
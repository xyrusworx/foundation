using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Client.Serialization
{
	[PublicAPI]
	public sealed class JsonCommunicationStrategy : CommunicationStrategy
	{
		private readonly JsonSerializer mSerializer;

		internal JsonCommunicationStrategy()
		{
			mSerializer = new JsonSerializer();
			mSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
		}

		public override string ContentType => "application/json";

		public override Task<object> ReadAsync(Stream stream, Encoding encoding, Type type)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));
			if (type == null) throw new ArgumentNullException(nameof(type));

			using (var reader = new StreamReader(stream, encoding))
			{
				return Task.FromResult(mSerializer.Deserialize(new JsonTextReader(reader), type));
			}
		}
		public override Task WriteAsync(Stream stream, Encoding encoding, object obj)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));

			if (obj == null)
			{
				return Task.CompletedTask;
			}

			using (var writer = new StringWriter())
			{
				mSerializer.Serialize(writer, obj);
				writer.Flush();

				var buffer = encoding.GetBytes(writer.ToString());
				return stream.WriteAsync(buffer, 0, buffer.Length);
			}
		}
	}
}
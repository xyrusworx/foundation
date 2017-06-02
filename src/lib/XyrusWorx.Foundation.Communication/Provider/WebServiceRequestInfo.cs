using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public sealed class WebServiceRequestInfo
	{
		private readonly HttpContext mContext;

		internal WebServiceRequestInfo(HttpContext context)
		{
			mContext = context;

			Headers = new MemoryKeyValueStore<string>();

			var requestHeaders = context?.Request?.Headers;
			if (requestHeaders != null)
			{
				foreach (var key in requestHeaders.Keys)
				{
					Headers.Write(new StringKey(key), requestHeaders[key].Concat("; "));
				}
			}
		}

		public IKeyValueStore<string> Headers { get; }

		[CanBeNull]
		public bool? IsHttps => mContext?.Request?.IsHttps;

		[CanBeNull]
		public IPAddress LocalAddress => mContext?.Connection?.LocalIpAddress;

		[CanBeNull]
		public IPAddress RemoteAddress => mContext?.Connection?.RemoteIpAddress;

		[CanBeNull]
		public int? LocalPort => mContext?.Connection?.LocalPort;

		[CanBeNull]
		public int? RemotePort => mContext?.Connection?.RemotePort;

		[CanBeNull]
		public X509Certificate GetClientCertificate()
		{
			return mContext?.Connection?.ClientCertificate;
		}

		[CanBeNull]
		public async Task<X509Certificate> GetClientCertificateAsync()
		{
			var connection = mContext?.Connection;
			if (connection == null)
			{
				return null;
			}

			return await connection.GetClientCertificateAsync();
		}
	}
}
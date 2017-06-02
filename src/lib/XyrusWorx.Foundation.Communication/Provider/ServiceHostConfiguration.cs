using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public class ServiceHostConfiguration
	{
		private readonly X509Certificate2 mCertificate;

		public ServiceHostConfiguration(string hostname, ushort port, X509Certificate2 certificate = null)
		{
			mCertificate = certificate;
			hostname = hostname.NormalizeNull();

			if (hostname == "*")
			{
				hostname = null;
			}

			if (hostname != null && (Regex.IsMatch(hostname, "[*:/|\\\"'?\\s]") || Regex.IsMatch(hostname, "^\\d$")))
			{
				throw new ArgumentException($"Invalid hostname: {hostname}", nameof(hostname));
			}

			Hostname = hostname ?? "*";
			Port = port;
			UseHttps = mCertificate != null;
			mCertificate = certificate;
		}

		[NotNull]
		public string Hostname { get; }
		public ushort Port { get; }

		public bool UseHttps { get; }

		[CanBeNull]
		public X509Certificate2 GetCertificate() => mCertificate;

		[NotNull]
		public ServiceHostProviderConfiguration Configuration { get; } = new ServiceHostProviderConfiguration();

		[CanBeNull]
		public object LogMessageScope { get; set; }
	}
}
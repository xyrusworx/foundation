using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using XyrusWorx.Communication.Client.Security;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Client
{
	[PublicAPI]
	public class ServiceClientConfiguration
	{
		public ServiceClientConfiguration([NotNull] string hostname, ushort port)
		{
			if (hostname.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(hostname));
			}

			if (Regex.IsMatch(hostname, "[:/|\\\"'?\\s]") || Regex.IsMatch(hostname, "^\\d$"))
			{
				throw new ArgumentException($"Invalid hostname: {hostname}", nameof(hostname));
			}

			Hostname = hostname;
			Port = port;
		}

		[NotNull]
		public string Hostname { get; }
		public ushort Port { get; }

		public bool TransportLevelSecurity { get; set; }
		public CommunicationStrategy CommunicationStrategy { get; set; }
		public Encoding Encoding { get; set; }

		[NotNull]
		public List<ServiceClientAuthentication> AuthenticationMeasures { get; } = new List<ServiceClientAuthentication>();
	}
}
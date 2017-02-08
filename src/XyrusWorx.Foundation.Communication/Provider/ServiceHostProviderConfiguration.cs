using System.Text;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public class ServiceHostProviderConfiguration : IProviderConfiguration
	{
		public string RoutePrefix { get; set; }

		public CommunicationStrategy CommunicationStrategy { get; set; }
		public bool IncludeExceptionDetailsInErrorResponses { get; set; }
		public Encoding Encoding { get; set; }
	}
}
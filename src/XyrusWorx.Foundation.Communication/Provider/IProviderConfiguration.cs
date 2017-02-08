using System.Text;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public interface IProviderConfiguration
	{
		[CanBeNull]
		string RoutePrefix { get; }

		[CanBeNull]
		Encoding Encoding { get; }

		CommunicationStrategy CommunicationStrategy { get; }
		bool IncludeExceptionDetailsInErrorResponses { get; set; }
	}
}
using JetBrains.Annotations;

namespace XyrusWorx.Foundation.Communication.Client
{
	[PublicAPI]
	public interface IWebResult : IResult
	{
		int StatusCode { get; }
	}
}
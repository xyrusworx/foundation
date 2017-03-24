using JetBrains.Annotations;

namespace XyrusWorx.Communication.Client
{
	[PublicAPI]
	public interface IWebResult : IResult
	{
		int StatusCode { get; }
	}
}
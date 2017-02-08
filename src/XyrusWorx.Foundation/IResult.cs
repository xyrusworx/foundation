using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public interface IResult
	{
		bool HasError { get; }

		[CanBeNull]
		string ErrorDescription { get; }
	}
}
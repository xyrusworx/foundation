using System.Threading;

namespace XyrusWorx.Runtime
{
	class GenericApplication : Application
	{
		protected override IResult Execute(CancellationToken cancellationToken)
		{
			return Result.Success;
		}
	}
}
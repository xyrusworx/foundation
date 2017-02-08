using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public class MultiResult : IResult
	{
		public bool HasError => Results.Any(x => x.HasError);
		public string ErrorDescription => HasError ? null : "One or more errors have occured.";

		[NotNull]
		public IList<IResult> Results { get; } = new List<IResult>();
	}
}
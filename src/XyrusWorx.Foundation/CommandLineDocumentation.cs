using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public class CommandLineDocumentation
	{
		[NotNull]
		public List<CommandLineTokenDocumentation> AcceptedTokens { get; } = new List<CommandLineTokenDocumentation>();

		public IEnumerable<CommandLineTokenDocumentation> GetSortedTokens() => AcceptedTokens.OrderBy(x => x.Rank);

		public override string ToString()
		{
			var tokStr = GetSortedTokens().OrderBy(x => x.Rank).Select(x => x.ToString()).ToArray();

			return string.Join(" ", tokStr);
		}
	}
}
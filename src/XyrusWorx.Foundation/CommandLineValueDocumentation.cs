using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public class CommandLineValueDocumentation : CommandLineTokenDocumentation
	{
		[NotNull]
		public List<string> AcceptedValues { get; } = new List<string>();

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("{" + $"{string.Join(" | ", AcceptedValues)}" + "}");

			return sb.ToString();
		}

		internal override int Rank => 100;
	}
}
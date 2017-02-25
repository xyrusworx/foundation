using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XyrusWorx
{
	[PublicAPI]
	public class CommandLinePropertyDocumentation : CommandLineNamedTokenDocumentation
	{
		public CommandLinePropertyDocumentation([NotNull] string name) : base(name)
		{
		}

		public bool IsOptional { get; set; }
		public bool AllowMultiple { get; set; }

		[NotNull]
		public List<string> AcceptedValues { get; } = new List<string>();

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (AllowMultiple)
			{
				sb.Append("{");
			}
			else if (IsOptional)
			{
				sb.Append("[");
			}

			sb.Append($"--{Name}");

			if (AcceptedValues.Any())
			{
				sb.Append($" <{string.Join(" | ", AcceptedValues)}>");
			}

			if (AllowMultiple)
			{
				sb.Append("}");
			}
			else if (IsOptional)
			{
				sb.Append("]");
			}

			return sb.ToString();
		}

		internal override int Rank => IsOptional ? 1 : 50;
	}
}
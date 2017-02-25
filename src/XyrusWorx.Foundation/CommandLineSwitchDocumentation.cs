using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public class CommandLineSwitchDocumentation : CommandLineNamedTokenDocumentation
	{
		public CommandLineSwitchDocumentation([NotNull] string name) : base(name)
		{
		}

		public bool AllowMultiple { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (AllowMultiple)
			{
				sb.Append("{");
			}
			else
			{
				sb.Append("[");
			}

			sb.Append($"--{Name}");

			if (AllowMultiple)
			{
				sb.Append("}");
			}
			else
			{
				sb.Append("]");
			}

			return sb.ToString();
		}

		internal override int Rank => 10;
	}
}
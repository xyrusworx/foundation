using System;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Property)]
	public class CommandLineValuesAttribute : CommandLineAttribute
	{
		protected override Result<object> GetValueOverride(CommandLineKeyValueStore parser)
		{
			return new Result<object>(parser.ReadTail().ToArray());
		}
	}
}
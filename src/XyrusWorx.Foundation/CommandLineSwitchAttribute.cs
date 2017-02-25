using System;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Property)]
	public class CommandLineSwitchAttribute : CommandLineAttribute
	{
		public CommandLineSwitchAttribute([NotNull] string name)
		{
			if (name.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			Name = name;
		}

		[NotNull]
		public string Name { get; }

		[CanBeNull]
		public string ShortForm { get; set; }

		protected override Result PrepareOverride(CommandLineKeyValueStore parser)
		{
			parser.RegisterFlag(Name);
			parser.RegisterAlias(Name, ShortForm);

			return Result.Success;
		}
		protected override Result<object> GetValueOverride(CommandLineKeyValueStore parser)
		{
			if (!parser.Exists(Name))
			{
				return Result.CreateError<Result<object>>($"Command line switch \"{Name}\" not set.");
			}

			return new Result<object>(parser.Exists(Name));
		}
		protected override object GetDefaultValue() => false;
	}
}
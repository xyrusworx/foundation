using System;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Property)]
	public class CommandLinePropertyAttribute : CommandLineAttribute
	{
		public CommandLinePropertyAttribute([NotNull] string name)
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

		[CanBeNull]
		public object DefaultValue { get; set; }

		protected override Result PrepareOverride(CommandLineKeyValueStore parser)
		{
			parser.RegisterAlias(Name, ShortForm);

			return Result.Success;
		}
		protected override Result<object> GetValueOverride(CommandLineKeyValueStore parser)
		{
			if (!parser.Exists(Name) || string.IsNullOrWhiteSpace(parser.Read(Name)))
			{
				return Result.CreateError<Result<object>>($"Command line value \"{Name}\" not set.");
			}

			return new Result<object>(parser.ReadMany(Name).ToArray());
		}
		protected override object GetDefaultValue() => DefaultValue;

		public override StringKey GetKey() => Name;
	}
}
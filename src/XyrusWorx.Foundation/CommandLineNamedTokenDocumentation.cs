using System;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public abstract class CommandLineNamedTokenDocumentation : CommandLineTokenDocumentation
	{
		protected CommandLineNamedTokenDocumentation([NotNull] string name)
		{
			if (name.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			Name = name;
		}

		[CanBeNull]
		public string ShortName { get; set; }

		[NotNull]
		public string Name { get; }
	}
}
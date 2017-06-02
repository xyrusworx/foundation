using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public abstract class CommandLineTokenDocumentation
	{
		[CanBeNull]
		public string Description { get; set; }

		[CanBeNull]
		public string ShortDescription { get; set; }

		internal abstract int Rank { get; }
	}
}
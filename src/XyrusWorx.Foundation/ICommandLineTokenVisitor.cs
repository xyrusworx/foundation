using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;

namespace XyrusWorx
{
	[PublicAPI]
	public interface ICommandLineTokenVisitor
	{
		StringKey GetKey();

		[NotNull]
		Result Prepare([NotNull] CommandLineKeyValueStore parser);

		[NotNull]
		Result Visit([NotNull] CommandLineKeyValueStore parser, [NotNull] PropertyInfo property, [NotNull] object modelInstance, [NotNull] ILogWriter log);
	}
}
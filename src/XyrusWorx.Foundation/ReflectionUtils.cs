using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public static class ReflectionUtils
	{
		[NotNull]
		public static IEnumerable<Type> GetLoadableTypes([NotNull] this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return GetLoadableTypeInfos(assembly).Select(x => x.AsType());
		}

		[NotNull]
		public static IEnumerable<TypeInfo> GetLoadableTypeInfos([NotNull] this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			try
			{
				return assembly.DefinedTypes.ToArray();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(x => x != null).Select(x => x.GetTypeInfo()).ToArray();
			}
		}
	}
}
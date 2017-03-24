using JetBrains.Annotations;
using System;
using System.IO;
using System.Reflection;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public class AssemblyMetadata
	{
		private readonly Assembly mAssembly;

		internal AssemblyMetadata([CanBeNull] Assembly assembly)
		{
			mAssembly = assembly;
		}

		[NotNull]
		public string ModuleName => (mAssembly?.Location).TryTransform(Path.GetFileName);

		[CanBeNull] public string AssemblyName => mAssembly?.FullName;

#if (NO_NATIVE_BOOTSTRAPPER)
		[CanBeNull] public string AssemblyLocation => null;
		[CanBeNull] public string AssemblyCodeBase => null;
#else
		[CanBeNull] public string AssemblyLocation => mAssembly?.Location;
		[CanBeNull] public string AssemblyCodeBase => mAssembly?.CodeBase;
#endif

		[CanBeNull] public string ProductName => Read<AssemblyProductAttribute>(x => x.Product);
		[CanBeNull] public string FileName => AssemblyLocation.TryTransform(Path.GetFileName);

		[CanBeNull] public string CompanyName => Read<AssemblyCompanyAttribute>(x => x.Company);
		[CanBeNull] public string LegalCopyright => Read<AssemblyCopyrightAttribute>(x => x.Copyright);
		[CanBeNull] public string LegalTrademarks => Read<AssemblyTrademarkAttribute>(x => x.Trademark);
		[CanBeNull] public string Culture => Read<AssemblyCultureAttribute>(x => x.Culture);

		[CanBeNull] public string FileVersion => Read<AssemblyFileVersionAttribute>(x => x.Version) ?? mAssembly?.GetName().Version?.ToString();
		[CanBeNull] public string ProductVersion => Read<AssemblyVersionAttribute>(x => x.Version) ?? mAssembly?.GetName().Version?.ToString();

		private string Read<T>(Func<T, string> property) where T : Attribute
		{
			var attribute = mAssembly.GetCustomAttribute<T>();
			if (attribute == null)
			{
				return null;
			}

			var value = property(attribute);

			return value;
		}
	}
}
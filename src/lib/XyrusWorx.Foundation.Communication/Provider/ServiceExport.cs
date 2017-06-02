using System;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI, AttributeUsage(AttributeTargets.Method), MeansImplicitUse(ImplicitUseTargetFlags.Members)]
	public class ServiceExport : Attribute
	{
		private readonly string mRoute;
		private readonly WebServiceVerbs mVerbs;

		public ServiceExport(WebServiceVerbs verbs)
		{
			if (verbs.HasFlag(WebServiceVerbs.Get))
			{
				throw new ArgumentException("Exports to the index path are not allowed for verb GET.");
			}

			mRoute = string.Empty;
			mVerbs = verbs;
		}
		public ServiceExport([NotNull] string route, WebServiceVerbs verbs = WebServiceVerbs.All)
		{
			if (route.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(route));
			}

			mRoute = route;
			mVerbs = verbs;
		}

		public string Route => mRoute;
		public WebServiceVerbs Verbs => mVerbs;
	}
}
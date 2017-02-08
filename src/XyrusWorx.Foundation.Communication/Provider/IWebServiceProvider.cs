using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using XyrusWorx.Communication.Security;
using XyrusWorx.Diagnostics;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public interface IWebServiceProvider
	{
		[NotNull]
		ILogWriter Log { get; }

		[NotNull]
		IProviderConfiguration Configuration { get; }

		[NotNull]
		IEnumerable<WebService> Services { get; }

		[NotNull]
		IEnumerable<IAuthenticationService> GetAuthenticationServices(Type filter);

		[NotNull]
		IWebServiceProvider AddService([NotNull] WebService service);

		[NotNull]
		IWebServiceProvider AddAuthentication<T>([NotNull] T authenticationService) where T : IAuthenticationService;

		event EventHandler<RouteInfo> RouteMapped;
	}
}
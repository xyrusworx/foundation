using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Routing;
using XyrusWorx.Collections;
using XyrusWorx.Communication.Security;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public class IntegrativeWebServiceHost : Resource, IWebServiceProvider
	{
		private readonly Dictionary<Type, IAuthenticationService> mAuthenticationServices;
		private readonly Dictionary<StringKey, WebService> mServices;

		private bool mWasActivated;

		public IntegrativeWebServiceHost([NotNull] CommunicationStrategy communicationStrategy, ILogWriter log = null) : this(communicationStrategy, null, log) { }
		public IntegrativeWebServiceHost([NotNull] CommunicationStrategy communicationStrategy, string routePrefix, ILogWriter log = null)
		{
			if (communicationStrategy == null)
			{
				throw new ArgumentNullException(nameof(communicationStrategy));
			}

			mAuthenticationServices = new Dictionary<Type, IAuthenticationService>();
			mServices = new Dictionary<StringKey, WebService>();

			Configuration = new ServiceHostProviderConfiguration
			{
				CommunicationStrategy = communicationStrategy,
				RoutePrefix = routePrefix.NormalizeNull()
			};

			Log = log ?? new NullLogWriter();
		}

		public event EventHandler<RouteInfo> RouteMapped;

		public ILogWriter Log { get; }
		public IProviderConfiguration Configuration { get; }
		public IEnumerable<WebService> Services => mServices.Values;
		public IEnumerable<IAuthenticationService> GetAuthenticationServices(Type filter)
		{
			if (filter == null)
			{
				return mAuthenticationServices.Values;
			}

			var service = mAuthenticationServices.GetValueByKeyOrDefault(filter);
			if (service != null)
			{
				return new[] { service };
			}

			return new IAuthenticationService[0];
		}

		public IWebServiceProvider AddService(WebService service)
		{
			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			if (mWasActivated)
			{
				throw new InvalidOperationException("Services can't be added after the service host has been activated.");
			}

			var key = service.Route.AsKey().Normalize();

			if (mServices.ContainsKey(key))
			{
				throw new InvalidOperationException($"A service with this route (\"{service.Route}\") has already been added to the host.");
			}

			mServices.Add(key, service);
			return this;
		}
		public IWebServiceProvider AddAuthentication<T>(T authenticationService) where T : IAuthenticationService
		{
			if (authenticationService == null)
			{
				throw new ArgumentNullException(nameof(authenticationService));
			}

			mAuthenticationServices.AddOrUpdate(typeof(T), authenticationService);
			return this;
		}

		public void Activate([NotNull] IRouteBuilder routeBuilder)
		{
			if (routeBuilder == null)
			{
				throw new ArgumentNullException(nameof(routeBuilder));
			}

			foreach (var service in mServices.Values)
			{
				var exporter = new WebServiceExporter(service, this);

				exporter.RoutePrefix = Configuration.RoutePrefix;
				exporter.Log = Log;

				foreach (var routeInfo in exporter.Export(routeBuilder))
				{
					RouteMapped?.Invoke(this, routeInfo);
				}
			}

			mWasActivated = true;
		}

		protected override void DisposeOverride()
		{
			mWasActivated = false;
		}
	}
}
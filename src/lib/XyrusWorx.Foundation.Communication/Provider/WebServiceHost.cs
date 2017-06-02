using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using XyrusWorx.Collections;
using XyrusWorx.Communication.Security;
using XyrusWorx.Diagnostics;
using XyrusWorx.Threading;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public class WebServiceHost : Operation, IWebServiceProvider
	{
		private static readonly object mLock = new object();
		private static readonly Scope mStateScope = new Scope();

		private readonly Dictionary<Type, IAuthenticationService> mAuthenticationServices;
		private readonly Dictionary<StringKey, WebService> mServices;

		private IWebHost mHost;

		public WebServiceHost([NotNull] ServiceHostConfiguration configuration, ILogWriter log = null)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			mServices = new Dictionary<StringKey, WebService>();
			mAuthenticationServices = new Dictionary<Type, IAuthenticationService>();

			Configuration = configuration;
			Log = log ?? new NullLogWriter();
		}

		[NotNull]
		public Uri Uri => new Uri($"{(Configuration.UseHttps ? "https" : "http")}://{Configuration.Hostname}:{Configuration.Port}");

		[NotNull]
		public override string DisplayName => $"Service host on \"{Uri}\"";

		[NotNull]
		public ServiceHostConfiguration Configuration { get; }

		public ILogWriter Log { get; }
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

			if (IsRunning)
			{
				throw new InvalidOperationException("Services can't be added while the service host is running.");
			}

			var key = service.Route.AsKey().Normalize();

			if (mServices.ContainsKey(key))
			{
				throw new InvalidOperationException($"A service with this route (\"{service.Route}\") has already been added to the host.");
			}

			mServices.Add(key, service);
			return this;
		}
		public IWebServiceProvider AddAuthentication<T>(T authenticationService) where T: IAuthenticationService
		{
			if (authenticationService == null)
			{
				throw new ArgumentNullException(nameof(authenticationService));
			}

			mAuthenticationServices.AddOrUpdate(typeof(T), authenticationService);
			return this;
		}

		public event EventHandler<RouteInfo> RouteMapped;

		protected sealed override IResult Initialize()
		{
			lock (mLock)
			{
				using (mStateScope.Enter(this))
				{
					var builder = new WebHostBuilder()
						.UseUrls($"{Uri}")
						.UseStartup<WebServiceHostRuntime>()
						.UseKestrel(options =>
						{
							if (Configuration.UseHttps)
							{
								options.UseHttps(Configuration.GetCertificate());
							}
						});

					mHost = builder.Build();
				}
			}

			try
			{
				mHost.Start();
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					Log.Write(e);
				}

				return Result.CreateError($"Failed to bind service to url \"http://{Configuration.Hostname}:{Configuration.Port}\".");
			}
			catch (Exception e)
			{
				Log.Write(e);
				return Result.CreateError($"Failed to bind service to url \"http://{Configuration.Hostname}:{Configuration.Port}\".");
			}

			return Result.Success;
		}
		protected sealed override IResult Execute(CancellationToken cancellationToken)
		{
			Log.WriteInformation($"{DisplayName} listening");

			WaitHandler.Wait(() => cancellationToken.IsCancellationRequested);

			return Result.Success;
		}
		protected sealed override void Cleanup(bool wasCancelled)
		{
			mHost?.Dispose();
			mHost = null;
		}

		internal static WebServiceHost Current => mStateScope.State as WebServiceHost;
		internal void RaiseRouteMappedEvent(RouteInfo routeInfo)
		{
			RouteMapped?.Invoke(this, routeInfo);
		}

		IProviderConfiguration IWebServiceProvider.Configuration => Configuration.Configuration;
	}
}

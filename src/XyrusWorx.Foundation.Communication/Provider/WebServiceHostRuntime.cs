using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XyrusWorx.Communication.Serialization;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Provider
{
	class WebServiceHostRuntime
	{
		private readonly WebServiceHost mProvider;

		public WebServiceHostRuntime()
		{
			mProvider = WebServiceHost.Current.AssertNotNull();
		}

		[UsedImplicitly]
		[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddRouting();

			services.AddSingleton(mProvider);
		}

		[UsedImplicitly]
		public void Configure(IApplicationBuilder applicationBuilder, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
		{
			var routeBuilder = new RouteBuilder(applicationBuilder);

			foreach (var service in mProvider.Services)
			{
				var exporter = new WebServiceExporter(service, mProvider);

				exporter.RoutePrefix = mProvider.Configuration.Configuration.RoutePrefix.NormalizeNull();
				exporter.Log = mProvider.Log;

				foreach (var routeInfo in exporter.Export(routeBuilder))
				{
					mProvider.RaiseRouteMappedEvent(routeInfo);
				}
			}

			loggerFactory.AddProvider(new ServiceLogProvider(mProvider.Log, mProvider.Configuration.LogMessageScope));

			applicationBuilder.UseRouter(routeBuilder.Build());
			applicationBuilder.UseStatusCodePages(HandleFaultStatusCode);

			
		}

		private async Task HandleFaultStatusCode(StatusCodeContext context)
		{
			var statusCode = context.HttpContext.Response.StatusCode;
			var statusCodeName = Enum.GetName(typeof(HttpStatusCode), (HttpStatusCode)statusCode);
			var statusCodeDescription = statusCodeName.TryTransform(x => $"{statusCode} {Regex.Replace(x, "([A-Z])", " $1").Trim()}");

			var resultWriter = mProvider.Configuration.Configuration.CommunicationStrategy ?? new JsonCommunicationStrategy();
			var result = new Result
			{
				HasError = true,
				ErrorDescription = statusCodeDescription
			};

			context.HttpContext.Response.ContentType = resultWriter.ContentType;

			await resultWriter.WriteAsync(context.HttpContext.Response.Body, mProvider.Configuration.Configuration.Encoding ?? Encoding.UTF8, result);
		}
	}
}
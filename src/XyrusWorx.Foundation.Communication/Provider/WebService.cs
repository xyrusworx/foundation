using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using XyrusWorx.Communication.Security;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public abstract class WebService : Resource
	{
		private string mRoute;

		protected WebService([NotNull] string route)
		{
			if (route.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(route));
			}

			mRoute = route;
		}

		protected internal IAuthenticatedUser User { get; internal set; }
		protected internal WebServiceRequestInfo Request { get; internal set; }

		internal IServiceProvider FrameworkServiceProvider { get; set; }
		protected T ResolveFrameworkLevelService<T>() where T : class => FrameworkServiceProvider?.GetService<T>();

		protected virtual WebServiceResult Index()
		{
			return NotAllowed();
		}

		protected WebServiceResult Ok() => new WebServiceResult(204);
		protected WebServiceResult Ok(object data) => new WebServiceResult(200, data);

		protected WebServiceResult BadRequest() => new WebServiceResult(400);
		protected WebServiceResult BadRequest(object data) => new WebServiceResult(400, data);

		protected WebServiceResult NotFound() => new WebServiceResult(404);
		protected WebServiceResult NotFound(object data) => new WebServiceResult(404, data);

		protected WebServiceResult NotAllowed() => new WebServiceResult(403);
		protected WebServiceResult NotAllowed(object data) => new WebServiceResult(403, data);

		protected WebServiceResult StatusCode(int statusCode) => new WebServiceResult(statusCode);
		protected WebServiceResult StatusCode(int statusCode, object data) => new WebServiceResult(statusCode, data);

		protected WebServiceResult Fail(string message)
		{
			var result = Result.CreateError<Result>(message);

			return new WebServiceResult(500) { ErrorDescription = result.ErrorDescription, ErrorDetails = result.ErrorDetails };
		}
		protected WebServiceResult Fail(Exception exception)
		{
			var result = Result.CreateError<Result>(exception);

			return new WebServiceResult(500) { ErrorDescription = result.ErrorDescription, ErrorDetails = result.ErrorDetails };
		}

		public string Route => mRoute;
	}
}
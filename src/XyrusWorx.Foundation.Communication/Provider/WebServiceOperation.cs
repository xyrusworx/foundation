using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using XyrusWorx.Collections;
using XyrusWorx.Communication.Security;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Provider
{
	class WebServiceOperation
	{
		private readonly WebServiceExporter mServiceExporter;
		private readonly MethodBase mMethod;
		private readonly WebServiceVerbs[] mVerbs;
		private readonly WebServiceOperationParameter[] mParameters;

		private readonly Type mAuthenticationProviderType;
		private readonly bool mAuthenticationRequired;
		private readonly string mRoute;

		public WebServiceOperation([NotNull] WebServiceExporter serviceExporter)
		{
			if (serviceExporter == null) throw new ArgumentNullException(nameof(serviceExporter));

			mServiceExporter = serviceExporter;

			var type = mServiceExporter.GetExtensionType();
			var method =
				type.GetMethod("Index", BindingFlags.Instance | BindingFlags.NonPublic) ??
				type.GetMethod("Index", BindingFlags.Instance | BindingFlags.Public);

			if (method.GetParameters().Length == 0)
			{
				mMethod = method;
				mRoute = string.Empty;
				mVerbs = new[] { WebServiceVerbs.Get };
				mParameters = new WebServiceOperationParameter[0];
			}

			var authRequiredAttribute = method.GetCustomAttribute<RequireAuthenticationAttribute>();
			if (authRequiredAttribute != null)
			{
				mAuthenticationRequired = true;
				mAuthenticationProviderType = authRequiredAttribute.ProviderType;
			}
		}
		public WebServiceOperation([NotNull] WebServiceExporter serviceExporter, [NotNull] MethodBase method)
		{
			if (serviceExporter == null) throw new ArgumentNullException(nameof(serviceExporter));
			if (method == null) throw new ArgumentNullException(nameof(method));

			mServiceExporter = serviceExporter;

			var attribute = method.GetCustomAttribute<ServiceExport>();

			if (attribute != null)
			{
				mMethod = method;
				mRoute = attribute.Route;
				mVerbs = Enum
					.GetValues(typeof(WebServiceVerbs))
					.OfType<WebServiceVerbs>()
					.Except(new[] { WebServiceVerbs.All, WebServiceVerbs.None })
					.Where(x => attribute.Verbs.HasFlag(x))
					.ToArray();
				mParameters = method.GetParameters().Select(x => new WebServiceOperationParameter(x)).ToArray();

				var authRequiredAttribute = method.GetCustomAttribute<RequireAuthenticationAttribute>();
				if (authRequiredAttribute != null)
				{
					mAuthenticationRequired = true;
					mAuthenticationProviderType = authRequiredAttribute.ProviderType;
				}

				if (mParameters.Count(x => x.FromBody) > 1)
				{
					throw new ArgumentException($"{mServiceExporter.GetExtensionType().FullName}.{method.Name}: only one parameter can be decorated with \"{typeof(FromBodyAttribute).Name}\".");
				}
			}
		}

		public IEnumerable<RouteInfo> Map([NotNull] IRouteBuilder builder, WebService instance)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			if (mMethod == null || mParameters == null || mVerbs == null)
			{
				return new RouteInfo[0];
			}

			var infos = new List<RouteInfo>();

			foreach (var verb in mVerbs)
			{
				var routeInfo = new RouteInfo(
					instance.GetType(), 
					mMethod,
					mVerbs.Aggregate(WebServiceVerbs.None, (a, b) => a | b), 
					GetRoute(mRoute, instance));

				builder.MapVerb(verb.ToString().ToUpper(), GetRoute(mRoute, instance), HandleRequestAsync);

				infos.Add(routeInfo);
			}

			return infos.ToArray();
		}
		public bool IsExportedMethod => mMethod != null && mVerbs != null && mParameters != null;

		private async Task HandleRequestAsync(HttpContext context)
		{
			var inputEncoding = mServiceExporter.Encoding ?? Encoding.UTF8;
			var outputEncoding = mServiceExporter.Encoding ?? Encoding.UTF8;

			var acceptHeader = context.Request.Headers.GetValueByKeyOrDefault("Accept").ToArray().FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

			var inputStrategy = context.Request.ContentType.NormalizeNull().TryTransform(CommunicationStrategy.GetCommunicationStrategy) ?? mServiceExporter.DefaultCommunicationStrategy;
			var outputStrategy = acceptHeader.NormalizeNull().TryTransform(CommunicationStrategy.GetCommunicationStrategy) ?? mServiceExporter.DefaultCommunicationStrategy;

			context.Response.ContentType = outputStrategy.ContentType + $"; charset={outputEncoding.WebName}";

			var encodingMatch = new Regex(@"^.*?/.*?;\s*charset=(.*?)$", RegexOptions.IgnoreCase).Match(context.Request.ContentType ?? string.Empty);
			if (encodingMatch.Success)
			{
				var ianaName = encodingMatch.Groups[1].Value;
				try
				{
					var providedInputEncoding = Encoding.GetEncoding(ianaName);
					inputEncoding = providedInputEncoding;
				}
				catch
				{
					mServiceExporter.Log?.WriteInformation("Provided input encoding was not understood: {0}", ianaName);
				}
			}

			var instance = mServiceExporter.GetInstance();
			var instanceScope = new Scope(() => { }, () =>
			{
				instance.Dispose();
			});

			using (instanceScope.Enter())
			{
				if (mAuthenticationRequired)
				{
					foreach (var authenticationService in mServiceExporter.Provider.GetAuthenticationServices(mAuthenticationProviderType))
					{
						if (authenticationService != null)
						{
							instance.User = await authenticationService.GetUserFromRequestAsync(instance, context.Request, context.User);
							if (instance.User != null)
							{
								break;
							}
						}
					}
				}

				instance.Request = new WebServiceRequestInfo(context);
				instance.FrameworkServiceProvider = context.RequestServices;

				if (mMethod == null || mParameters == null)
				{
					await outputStrategy.WriteAsync(context.Response.Body, outputEncoding, GetHttpResponse(context.Response.StatusCode = 404));
					return;
				}

				if (mAuthenticationRequired && instance.User == null)
				{
					await outputStrategy.WriteAsync(context.Response.Body, outputEncoding, GetHttpResponse(context.Response.StatusCode = 403));
					return;
				}

				var parameterValues = new object[mParameters.Length];

				for (var i = 0; i < mParameters.Length; i++)
				{
					var fallback = mParameters[i].DefaultValue ?? (mParameters[i].Type.GetTypeInfo().IsValueType ? Activator.CreateInstance(mParameters[i].Type) : null);

					if (mParameters[i].FromBody)
					{
						var inpCt = inputStrategy.ContentType?.Split(';').FirstOrDefault();
						var reqCt = context.Request.ContentType?.Split(';').FirstOrDefault();

						if (!string.Equals(inpCt, reqCt, StringComparison.OrdinalIgnoreCase))
						{
							await outputStrategy.WriteAsync(context.Response.Body, outputEncoding, GetHttpResponse(context.Response.StatusCode = 415));
							return;
						}

						try
						{
							parameterValues[i] = await inputStrategy.ReadAsync(context.Request.Body, inputEncoding, mParameters[i].Type) ?? fallback;
						}
						catch (Exception exception)
						{
							var httpResponse = GetHttpResponse(400);
							var exceptionResponse = Result.CreateError(exception);

							httpResponse.ErrorDetails = exceptionResponse.ErrorDetails;
							httpResponse.ErrorDescription =
								exceptionResponse.ErrorDescription.NormalizeNull().TryTransform(x => $"Error reading request body: {x}") ??
								httpResponse.ErrorDescription;

							await outputStrategy.WriteAsync(context.Response.Body, outputEncoding, httpResponse);
							return;
						}
					}
					else if (mParameters[i].FromQuery)
					{
						StringValues queryParameterValue;

						if (context.Request.Query.TryGetValue(mParameters[i].Name, out queryParameterValue))
						{
							parameterValues[i] = string.Join(" ", queryParameterValue.ToArray()).TryDeserialize(mParameters[i].Type, CultureInfo.InvariantCulture) ?? fallback;
						}
						else
						{
							parameterValues[i] = fallback;
						}
					}
					else
					{
						var routeValue = context.GetRouteValue(mParameters[i].Name);
						if (routeValue == null)
						{
							parameterValues[i] = fallback;
						}
						else if (mParameters[i].Type.GetTypeInfo().IsInstanceOfType(routeValue))
						{
							parameterValues[i] = routeValue;
						}
						else
						{
							parameterValues[i] = routeValue.ToString().TryDeserialize(mParameters[i].Type, CultureInfo.InvariantCulture) ?? fallback;
						}
					}
				}

				try
				{
					var invokeResult = mMethod.Invoke(instance, parameterValues);
					if (invokeResult == null)
					{
						context.Response.StatusCode = 204;
						return;
					}

					if (invokeResult is Task)
					{
						var invokeTask = (Task) invokeResult;
						await invokeTask;

						var resultProperty = invokeResult.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
						if (resultProperty != null)
						{
							invokeResult = resultProperty.GetValue(invokeTask);
						}
						else
						{
							context.Response.StatusCode = 204;
							return;
						}
					}

					var normalizedResult = invokeResult is WebServiceResult ? (WebServiceResult) invokeResult : new WebServiceResult(invokeResult);

					context.Response.ContentType = outputStrategy.ContentType;
					context.Response.StatusCode = normalizedResult.StatusCode;

					if (normalizedResult.HasError)
					{
						var httpResponse = GetHttpResponse(normalizedResult.StatusCode);

						httpResponse.ErrorDetails =
							normalizedResult.ErrorDetails ??
							normalizedResult.Data?.CastTo<Result>()?.ErrorDetails;

						httpResponse.ErrorDescription =
							normalizedResult.ErrorDescription.NormalizeNull() ??
							normalizedResult.Data?.CastTo<IResult>()?.ErrorDescription ??
							httpResponse.ErrorDescription;

						await outputStrategy.WriteAsync(context.Response.Body, outputEncoding, httpResponse);
						return;
					}

					await outputStrategy.WriteAsync(context.Response.Body, outputEncoding, normalizedResult.Data);
				}
				catch (Exception exception)
				{
					var errorResult = Result.CreateError(exception);

					if (!mServiceExporter.Provider.Configuration.IncludeExceptionDetailsInErrorResponses)
					{
						errorResult.ErrorDetails = null;
					}

					context.Response.StatusCode = 500;

					await outputStrategy.WriteAsync(context.Response.Body, outputEncoding, errorResult);
				}
				finally
				{
					instance.Request = null;
					instance.User = null;
					instance.FrameworkServiceProvider = null;
				}
			}
		}

		[Pure] private Result GetHttpResponse(int statusCode)
		{
			var statusCodeName = Enum.GetName(typeof(HttpStatusCode), (HttpStatusCode)statusCode);
			var statusCodeDescription = statusCodeName.TryTransform(x => $"{statusCode} {Regex.Replace(x, "([A-Z])", " $1").Trim()}");

			return new Result
			{
				HasError = statusCode >= 400,
				ErrorDescription = statusCodeDescription
			};
		}
		[Pure] private string GetRoute(string operationRoute, WebService instance)
		{
			return new[] { mServiceExporter.RoutePrefix?.Trim('/'), instance.Route.Trim('/'), operationRoute }.Concat("/").Trim('/');
		}
	}
}
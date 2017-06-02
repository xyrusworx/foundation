using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Routing;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Provider
{
	class WebServiceExporter
	{
		private readonly WebService mImplementation;

		public WebServiceExporter([NotNull] WebService implementation, [NotNull] IWebServiceProvider provider)
		{
			if (implementation == null) throw new ArgumentNullException(nameof(implementation));
			if (provider == null) throw new ArgumentNullException(nameof(provider));

			mImplementation = implementation;
			Provider = provider;
			Encoding = provider.Configuration.Encoding;

			DefaultCommunicationStrategy = provider.Configuration.CommunicationStrategy ?? new JsonCommunicationStrategy();
		}

		[NotNull] public CommunicationStrategy DefaultCommunicationStrategy { get; }
		[NotNull] public IWebServiceProvider Provider { get; }

		[CanBeNull] public ILogWriter Log { get; set; }
		[CanBeNull] public string RoutePrefix { get; set; }
		[CanBeNull] public Encoding Encoding { get; }

		public Type GetExtensionType() => mImplementation.GetType();
		public WebService GetInstance()
		{
			return mImplementation;
		}
		public IEnumerable<RouteInfo> Export([NotNull] IRouteBuilder routeBuilder)
		{
			if (routeBuilder == null)
			{
				throw new ArgumentNullException(nameof(routeBuilder));
			}

			var list = new List<RouteInfo>();

			foreach (var operation in CollectOperations())
			{
				var forOperation = operation.Map(routeBuilder, mImplementation);

				list.AddRange(forOperation);
			}

			return list;
		}

		[Pure]
		private IEnumerable<WebServiceOperation> CollectOperations()
		{
			var type = mImplementation.GetType();
			var indexOperation = new WebServiceOperation(this);

			if (indexOperation.IsExportedMethod)
			{
				yield return indexOperation;
			}

			while (type != null && type != typeof(object))
			{
				var typeInfo = type.GetTypeInfo();

				var publicMethods = typeInfo.GetMethods(BindingFlags.Public | BindingFlags.Instance);
				var nonPublicMethods = typeInfo.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

				foreach (var method in publicMethods.Concat(nonPublicMethods))
				{
					var operation = new WebServiceOperation(this, method);
					if (!operation.IsExportedMethod)
					{
						continue;
					}

					yield return operation;
				}

				type = typeInfo.BaseType;
			}
		}
	}
}
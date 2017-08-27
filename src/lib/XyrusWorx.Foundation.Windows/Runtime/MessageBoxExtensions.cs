using System;
using System.Windows;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public static class MessageBoxExtensions
	{
		[NotNull]
		public static IMessageBox Message([NotNull] this IMessageBox definition, [NotNull] Exception exception)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			return definition.Message(exception.GetOriginalMessage());
		}

		[NotNull]
		public static IMessageBox Message([NotNull] this IMessageBox definition, [CanBeNull] IResult result)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			return definition.Message(result?.ErrorDescription);
		}

		[NotNull]
		public static IMessageBox Notice([NotNull] this IMessageBox definition, [CanBeNull] IResult result)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			return definition.Notice((result?.HasError ?? false) ? result.ErrorDescription : null);
		}

		[NotNull]
		public static IMessageBox Warning([NotNull] this IMessageBox definition, [NotNull] Exception exception)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			return definition.Warning(exception.GetOriginalMessage());
		}

		[NotNull]
		public static IMessageBox Warning([NotNull] this IMessageBox definition, [CanBeNull] IResult result)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			return definition.Warning(result?.ErrorDescription);
		}

		[NotNull]
		public static IMessageBox Error([NotNull] this IMessageBox definition, [NotNull] Exception exception)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			return definition.Error(exception.GetOriginalMessage());
		}

		[NotNull]
		public static IMessageBox Error([NotNull] this IMessageBox definition, [CanBeNull] IResult result)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			return definition.Error((result?.HasError ?? false) ? null : result?.ErrorDescription);
		}

		[NotNull]
		public static IMessageBox Owner([NotNull] this IMessageBox definition, [NotNull] WpfApplication application)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			var mainWindow = application.Host.View as Window;
			if (mainWindow != null)
			{
				return definition.Owner(mainWindow);
			}

			return definition;
		}

		[NotNull]
		public static IMessageBox Owner([NotNull] this IMessageBox definition, [NotNull] IApplicationHost host)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (host == null)
			{
				throw new ArgumentNullException(nameof(host));
			}

			var mainWindow = host.View as Window;
			if (mainWindow != null)
			{
				return definition.Owner(mainWindow);
			}

			return definition;
		}
	}
}
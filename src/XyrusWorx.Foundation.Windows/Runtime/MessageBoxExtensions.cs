﻿using System;
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
		public static IMessageBox Owner([NotNull] this IMessageBox definition, [NotNull] ApplicationController application)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			return definition.Owner(application.Definition.MainWindow);
		}

		[NotNull]
		public static IMessageBox Owner([NotNull] this IMessageBox definition, [NotNull] ApplicationDefinition applicationDefinition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (applicationDefinition == null)
			{
				throw new ArgumentNullException(nameof(applicationDefinition));
			}

			return definition.Owner(applicationDefinition.MainWindow);
		}
	}
}
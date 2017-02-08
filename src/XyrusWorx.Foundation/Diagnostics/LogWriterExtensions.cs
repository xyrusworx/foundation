using System;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public static class LogWriterExtensions
	{
		public static void Write([NotNull] this ILogWriter instance, [NotNull] IResult result)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (result == null) throw new ArgumentNullException(nameof(result));

			var altError = result.HasError
				? "An unknown error has orrcured."
				: "The operation completed successfully.";

			instance.Write(result.ErrorDescription.NormalizeNull() ?? altError, result.HasError ? LogMessageClass.Error : LogMessageClass.Information);
		}
		public static void Write([NotNull] this ILogWriter instance, [NotNull] Exception exception, bool includeStackTrace = false)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			foreach (var err in exception.Unroll().Reverse())
			{
				instance.WriteError(includeStackTrace
					? $"{err.GetType().FullName}: {err}"
					: $"{err.GetType().FullName}: {err.GetOriginalMessage()}");
			}
		}

		public static void WriteDebug([NotNull] this ILogWriter instance, string message, params object[] parameters)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			// ReSharper disable once ConstantNullCoalescingCondition
			instance.Write(parameters == null || parameters.Length == 0 ? message : string.Format(message ?? string.Empty, parameters), LogMessageClass.Debug);
		}
		public static void WriteVerbose([NotNull] this ILogWriter instance, string message, params object[] parameters)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			// ReSharper disable once ConstantNullCoalescingCondition
			instance.Write(parameters == null || parameters.Length == 0 ? message : string.Format(message ?? string.Empty, parameters), LogMessageClass.Verbose);
		}
		public static void WriteInformation([NotNull] this ILogWriter instance, string message, params object[] parameters)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			// ReSharper disable once ConstantNullCoalescingCondition
			instance.Write(parameters == null || parameters.Length == 0 ? message : string.Format(message ?? string.Empty, parameters));
		}
		public static void WriteWarning([NotNull] this ILogWriter instance, string message, params object[] parameters)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			// ReSharper disable once ConstantNullCoalescingCondition
			instance.Write(parameters == null || parameters.Length == 0 ? message : string.Format(message ?? string.Empty, parameters), LogMessageClass.Warning);
		}
		public static void WriteError([NotNull] this ILogWriter instance, string message, params object[] parameters)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			// ReSharper disable once ConstantNullCoalescingCondition
			instance.Write(parameters == null || parameters.Length == 0 ? message : string.Format(message ?? string.Empty, parameters), LogMessageClass.Error);
		}
	}
}
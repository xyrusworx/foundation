using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using XyrusWorx.Diagnostics;

namespace XyrusWorx.Communication.Provider
{
	class ServiceLogProvider : Resource, ILoggerProvider
	{
		private readonly object mMessageScope;
		private readonly ILogWriter mWriter;

		public ServiceLogProvider([NotNull] ILogWriter writer, object messageScope = null)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			mWriter = writer;
			mMessageScope = messageScope;
		}
		public ILogger CreateLogger(string categoryName)
		{
			return new ServiceLogger(this, mMessageScope);
		}

		class ServiceLogger : ILogger
		{
			private readonly ServiceLogProvider mProvider;
			private readonly object mMessageScope;
			private readonly Scope mScope;

			public ServiceLogger([NotNull] ServiceLogProvider provider, object messageScope = null)
			{
				if (provider == null)
				{
					throw new ArgumentNullException(nameof(provider));
				}

				mProvider = provider;
				mMessageScope = messageScope;
				mScope = new Scope();
			}

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				if (logLevel == LogLevel.None)
				{
					return;
				}

				LogMessageClass messageClass;

				switch (logLevel)
				{
					case LogLevel.Trace: messageClass = LogMessageClass.Debug; break;
					case LogLevel.Debug: messageClass = LogMessageClass.Verbose; break;
					case LogLevel.Information: messageClass = LogMessageClass.Information; break;
					case LogLevel.Warning: messageClass = LogMessageClass.Warning; break;
					case LogLevel.Error:
					case LogLevel.Critical:
						messageClass = LogMessageClass.Error; break;
					default:
						return;
				}

				var scope = mMessageScope == null ? new Scope() : mProvider.mWriter.MessageScope;

				using (scope.Enter(mMessageScope))
				{
					if (exception != null)
					{
						mProvider.mWriter.Write(exception);
					}
					else
					{
						// ReSharper disable once ExpressionIsAlwaysNull
						mProvider.mWriter.Write(formatter(state, exception), messageClass);
					}
				}
			}
			public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && (int)logLevel - 2 >= (int)mProvider.mWriter.Verbosity;
			public IDisposable BeginScope<TState>(TState state)
			{
				return mScope.Enter(state);
			}
		}
	}
}
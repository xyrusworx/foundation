using System;
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;

namespace XyrusWorx
{
	[PublicAPI]
	public class Result : IResult
	{
		public bool HasError { get; set; }
		public string ErrorDescription { get; set; }

		[CanBeNull]
		public ErrorDetails ErrorDetails { get; set; }

		[NotNull]
		public static Result CreateError([CanBeNull] Exception exception)
		{
			return CreateError(typeof(Result), exception?.Message, exception?.HResult, exception?.StackTrace);
		}

		[NotNull]
		public static T CreateError<T>([CanBeNull] Exception exception) where T : Result, new()
		{
			return (T)CreateError(typeof(T), exception?.GetOriginalMessage(), exception?.HResult, exception?.StackTrace);
		}

		[NotNull]
		public static Result CreateError([NotNull] Type responseType, [CanBeNull] Exception exception)
		{
			return CreateError(responseType, exception?.Message, exception?.HResult, exception?.StackTrace);
		}

		[NotNull]
		public static Result CreateError([CanBeNull] string errorDescription, int? hResult = null, string stackTrace = null)
		{
			return CreateError(typeof (Result), errorDescription, hResult, stackTrace);
		}

		[NotNull]
		public static Result CreateError([NotNull] Type responseType, [CanBeNull] string errorDescription, int? hResult = null, string stackTrace = null)
		{
			if (responseType == null)
			{
				throw new ArgumentNullException(nameof(responseType));
			}

			var response = (Result)Activator.CreateInstance(responseType);

			response.HasError = true;
			response.ErrorDescription = errorDescription;
			response.ErrorDetails = new ErrorDetails
			{
				HResult = hResult.GetValueOrDefault(),
				StackTrace = stackTrace
			};

			return response;
		}

		[NotNull]
		public static T CreateError<T>([CanBeNull] string errorDescription, int? hResult = null, string stackTrace = null) where T : Result, new()
		{
			return (T)CreateError(typeof(T), errorDescription, hResult, stackTrace);
		}

		[NotNull]
		public Result Specialize([NotNull] Type targetType)
		{
			if (targetType == null)
			{
				throw new ArgumentNullException(nameof(targetType));
			}

			var targetResponse = (Result)Activator.CreateInstance(targetType);

			targetResponse.HasError = HasError;
			targetResponse.ErrorDescription = ErrorDescription;

			if (HasError)
			{
				targetResponse.ErrorDetails = ErrorDetails;
			}

			return targetResponse;
		}

		[Pure][NotNull]
		public T Specialize<T>() where T : Result, new() => (T)Specialize(typeof(T));
		
		[Pure][NotNull]
		public Result<T> With<T>(T data = default(T)) => new Result<T>(data);

		[NotNull]
		public static Result Success { get; } = new Result {ErrorDescription = "The operation completed successfully."};

		public void ThrowIfError()
		{
			if (HasError)
			{
				throw new Exception(ErrorDescription);
			}
		}
	}

	[PublicAPI]
	public class Result<T> : Result
	{
		public Result() { }
		public Result(T data)
		{
			Data = data;
		}
		public T Data { get; set; }
		
		[Pure]
		public T GetOrThrow()
		{
			if (!HasError)
			{
				return Data;
			}

			throw new Exception(ErrorDescription);
		}

		[Pure, NotNull]
		public static implicit operator Result<T>(T data) => new Result<T>(data);
	}
}

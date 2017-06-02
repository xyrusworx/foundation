using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public static class CommonUtils
	{
		private const string mNotAMemberExpression = "The provided expression (\"{1}\") does not contain a member of type {0}";

		public static T UnboxTo<T>([CanBeNull] this object instance) 
		{
			return instance is T ? (T)instance : default(T);
		}

		public static T AssertNotNull<T>([CanBeNull] this T instance) where T: class
		{
			Debug.Assert(instance != null);

			if (instance == null)
			{
				throw new NullReferenceException();
			}

			return instance;
		}

		[CanBeNull]
		public static T CastTo<T>([CanBeNull] this object instance) where T : class
		{
			return instance as T;
		}

		[NotNull]
		public static MemberInfo GetMember<T>([NotNull] this Type type, Expression<Action<T>> memberExpression)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var member = memberExpression.Body as MemberExpression;
			if (member == null)
			{
				throw new ArgumentException(string.Format(mNotAMemberExpression, type, memberExpression));
			}

			var info = member.Member;
			if (info == null)
			{
				throw new ArgumentException(string.Format(mNotAMemberExpression, type, memberExpression));
			}

			return info;
		}

		[NotNull]
		public static MemberInfo GetMember<T>([NotNull] this Type type, Expression<Func<T, object>> memberExpression)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var member = memberExpression.Body as MemberExpression;
			if (memberExpression.Body.NodeType == ExpressionType.Convert)
			{
				var unaryExpression = memberExpression.Body as UnaryExpression;
				if (unaryExpression == null)
				{
					throw new ArgumentException(string.Format(mNotAMemberExpression, type, memberExpression));
				}
				member = unaryExpression.Operand as MemberExpression;
			}

			if (member == null)
			{
				throw new ArgumentException(string.Format(mNotAMemberExpression, type, memberExpression));
			}

			var info = member.Member;
			if (info == null)
			{
				throw new ArgumentException(string.Format(mNotAMemberExpression, type, memberExpression));
			}

			return info;
		}

		public static bool HasFlag(this int value, int flag)
		{
			return (value & flag) == flag;
		}

		[NotNull]
		public static IEnumerable<Exception> Unroll([NotNull] this Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			var list = new List<Exception> { exception };

			if (exception is AggregateException)
			{
				var aggregate = exception.CastTo<AggregateException>().AssertNotNull();
				foreach (var detail in aggregate.InnerExceptions.SelectMany(Unroll))
				{
					list.Insert(0, detail);
				}
			}

			while (exception.InnerException != null)
			{
				list.Insert(0, exception.InnerException);
				exception = exception.InnerException;
			}

			return list;
		}

		[NotNull]
		public static string FormatHexDump([NotNull] this byte[] byteArray, int rowWidth = 16, bool includeColumnHeaders = false, bool includeRowHeaders = false)
		{
			// line width: 
			//    rowWidth * 4 + 2    ( + "| ")
			//    rowWidth * 4 + 13   ( + "XXXXXXXX | ", + "| ")

			if (byteArray == null)
			{
				throw new ArgumentNullException(nameof(byteArray));
			}

			var leftStringBuilder = new StringBuilder();
			var rightStringBuilder = new StringBuilder();
			var compositionStringBuilder = new StringBuilder();

			var column = 0;

			if (includeColumnHeaders)
			{
				if (includeRowHeaders)
				{
					compositionStringBuilder.Append(new string(' ', 8));
					compositionStringBuilder.Append(" | ");
				}

				for (var i = 0; i < rowWidth; i++)
				{
					compositionStringBuilder.AppendFormat("{0:x2} ", i);
				}

				compositionStringBuilder.Append("| ");
				compositionStringBuilder.Append(Environment.NewLine);

				if (includeRowHeaders)
				{
					compositionStringBuilder.Append(new string('-', 9));
					compositionStringBuilder.Append("+");
					compositionStringBuilder.Append("-");
				}
				
				compositionStringBuilder.Append(new string('-', 3 * rowWidth));
				compositionStringBuilder.Append("+");
				compositionStringBuilder.Append("-");
				compositionStringBuilder.Append(new string('-', rowWidth));
				compositionStringBuilder.Append(Environment.NewLine);
			}

			if (includeRowHeaders)
			{
				compositionStringBuilder.AppendFormat("{0:x8} | ", 0);
			}

			int bytePosition;
			for (bytePosition = 0; bytePosition < byteArray.Length; bytePosition++)
			{
				if (column >= rowWidth)
				{
					compositionStringBuilder.AppendFormat("{0}| {1}{2}", leftStringBuilder.ToString().PadRight(3 * rowWidth), rightStringBuilder, Environment.NewLine);
					leftStringBuilder.Clear();
					rightStringBuilder.Clear();

					if (includeRowHeaders)
					{
						compositionStringBuilder.AppendFormat("{0:x8} | ", bytePosition);
					}

					column = 0;
				}

				leftStringBuilder.AppendFormat("{0:x2} ", byteArray[bytePosition]);
				rightStringBuilder.Append(byteArray[bytePosition] < 0x20 ? '.' : (char) byteArray[bytePosition]);

				column++;
			}

			if (column > 0)
			{
				compositionStringBuilder.AppendFormat("{0}| {1}{2}", leftStringBuilder.ToString().PadRight(3 * rowWidth), rightStringBuilder, Environment.NewLine);
			}

			return compositionStringBuilder.ToString();
		}

		[NotNull]
		public static string GetOriginalMessage([NotNull] this Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			if (exception is AggregateException)
			{
				var aggregate = exception.CastTo<AggregateException>().AssertNotNull();
				if (aggregate.InnerExceptions.Count == 1)
				{
					return GetOriginalMessage(aggregate.InnerExceptions[0]);
				}

				if (aggregate.InnerExceptions.Count > 1)
				{
					var messages = aggregate.InnerExceptions.Select(GetOriginalMessage);
					var compositeMessage = messages.Concat(Environment.NewLine);

					return compositeMessage;
				}
			}

			if (exception is TargetInvocationException && exception.InnerException != null)
			{
				return GetOriginalMessage(exception.InnerException);
			}

			return exception.Message.NormalizeNull() ?? "An unknown error occured";
		}

		public static T GetOrThrow<T>(this Result<T> result)
		{
			if (result == null)
			{
				return default(T);
			}

			if (result.HasError)
			{
				throw new Exception(result.ErrorDescription);
			}

			return result.Data;
		}

		public static async Task<T> GetOrThrow<T>([NotNull] this Task<Result<T>> task)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			var result = await task;

			if (result == null)
			{
				return default(T);
			}

			if (result.HasError)
			{
				throw new Exception(result.ErrorDescription);
			}

			return result.Data;
		}

		public static T ExecuteSynchronous<T>([NotNull] this Task<T> task, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			try
			{
				task.Wait(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				return default(T);
			}

			return task.Result;
		}

		public static void ExecuteSynchronous([NotNull] this Task task, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			try
			{
				task.Wait(cancellationToken);
			}
			catch (OperationCanceledException)
			{
			}
		}

		public static async void Begin([NotNull] this Task task)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			await task;
		}
	}
}
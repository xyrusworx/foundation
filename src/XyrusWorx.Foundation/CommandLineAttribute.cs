using JetBrains.Annotations;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using XyrusWorx.Diagnostics;

namespace XyrusWorx
{
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class CommandLineAttribute : Attribute, ICommandLineTokenVisitor
	{
		public Result Prepare(CommandLineKeyValueStore parser)
		{
			if (parser == null)
			{
				throw new ArgumentNullException(nameof(parser));
			}

			return PrepareOverride(parser);
		}
		public Result Visit(CommandLineKeyValueStore parser, PropertyInfo property, object modelInstance, ILogWriter log)
		{
			if (parser == null)
			{
				throw new ArgumentNullException(nameof(parser));
			}

			if (property == null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			if (modelInstance == null)
			{
				throw new ArgumentNullException(nameof(modelInstance));
			}

			var targetType = property.PropertyType.GetTypeInfo();

			var defaultValue = GetDefault(targetType);
			var valueResult = GetValueOverride(parser);

			if (valueResult.HasError)
			{
				property.SetValue(modelInstance, defaultValue);
				log.WriteVerbose(valueResult.ErrorDescription);
				return valueResult;
			}

			var castResult = ChangeType(valueResult.Data, targetType);
			if (castResult.HasError)
			{
				property.SetValue(modelInstance, defaultValue);
				log.WriteWarning(castResult.ErrorDescription);
				return castResult;
			}

			property.SetValue(modelInstance, castResult.Data);
			return Result.Success;
		}

		[NotNull]
		protected virtual Result PrepareOverride([NotNull] CommandLineKeyValueStore parser) => Result.Success;

		[NotNull]
		protected abstract Result<object> GetValueOverride([NotNull] CommandLineKeyValueStore parser);

		[CanBeNull]
		protected virtual object GetDefaultValue() => null;

		private object GetDefault(TypeInfo targetType)
		{
			var overridenDefaultValue = GetDefaultValue();
			if (overridenDefaultValue != null)
			{
				var castResult = ChangeType(overridenDefaultValue, targetType);
				if (!castResult.HasError)
				{
					return castResult.Data;
				}
			}

			if (targetType.IsArray)
			{
				var elementType = targetType.GetElementType().GetTypeInfo();

				return new ArrayList().ToArray(elementType.UnderlyingSystemType);
			}

			return targetType.IsValueType ? Activator.CreateInstance(targetType.UnderlyingSystemType) : null;
		}
		private Result<object> ChangeType(object value, TypeInfo targetType)
		{
			if (targetType.IsArray)
			{
				var elementType = targetType.GetElementType().GetTypeInfo();
				var resultingElements = new ArrayList();

				if (value is IEnumerable)
				{
					foreach (var element in (IEnumerable)value)
					{
						var ctResult = ChangeType(element, elementType);
						if (ctResult.HasError)
						{
							continue;
						}

						resultingElements.Add(ctResult.Data);
					}
				}
				else
				{
					var ctResult = ChangeType(value, elementType);
					if (ctResult.HasError)
					{
						return ctResult;
					}

					resultingElements.Add(ctResult.Data);
				}

				return new Result<object>(resultingElements.ToArray(elementType.UnderlyingSystemType));
			}

			if (value is IEnumerable && !(value is string))
			{
				value = value.CastTo<IEnumerable>()?.OfType<object>().FirstOrDefault();
			}

			if (Equals(targetType, typeof(string).GetTypeInfo()))
			{
				return new Result<object>(value?.ToString());
			}

			if (targetType.IsEnum || targetType.IsPrimitive)
			{
				if (!(value ?? string.Empty).ToString().TryDeserialize(targetType, out var deserialized, CultureInfo.InvariantCulture))
				{
					return Result.CreateError<Result<object>>($"Failed to convert \"{value}\" to target type \"{targetType}\".");
				}

				return new Result<object>(deserialized);
			}

			try
			{
				return new Result<object>(Convert.ChangeType(value, targetType.UnderlyingSystemType, CultureInfo.InvariantCulture));
			}
			catch
			{
				return Result.CreateError<Result<object>>($"Failed to convert \"{value}\" to target type \"{targetType}\".");
			}
		}

		public abstract StringKey GetKey();
	}
}
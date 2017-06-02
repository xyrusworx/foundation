using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Property)]
	public class CommandLineAnnotationAttribute : Attribute
	{
		public CommandLineAnnotationAttribute(string shortDescription = null)
		{
			ShortDescription = shortDescription;
		}

		public string ValueLabel { get; set; }
		public bool IsRequired { get; set; }

		public string Description { get; set; }
		public string ShortDescription { get; set; }

		public void AddToDocumentation([NotNull] CommandLineDocumentation documentation, [NotNull] PropertyInfo property)
		{
			if (documentation == null)
			{
				throw new ArgumentNullException(nameof(documentation));
			}

			if (property == null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			var attributes = new CommandLineAttribute[]
			{
				property.GetCustomAttribute<CommandLinePropertyAttribute>(),
				property.GetCustomAttribute<CommandLineSwitchAttribute>(),
				property.GetCustomAttribute<CommandLineValuesAttribute>()
			};

			var attribute = attributes.FirstOrDefault(x => x != null);
			if (attribute == null)
			{
				throw new ArgumentException("The provided property is not bound to a command line token.", nameof(property));
			}

			if (attribute is CommandLineSwitchAttribute)
			{
				var switchAttribute = (CommandLineSwitchAttribute) attribute;

				documentation.AcceptedTokens.Add(new CommandLineSwitchDocumentation(switchAttribute.Name)
				{
					AllowMultiple = AcceptsMultiple(property),
					Description = Description,
					ShortDescription = ShortDescription,
					ShortName = switchAttribute.ShortForm
				});
			}

			if (attribute is CommandLinePropertyAttribute)
			{
				var propertyAttribute = (CommandLinePropertyAttribute)attribute;
				var doc = new CommandLinePropertyDocumentation(propertyAttribute.Name)
				{
					AllowMultiple = AcceptsMultiple(property),
					IsOptional = !IsRequired,
					Description = Description,
					ShortDescription = ShortDescription,
					ShortName = propertyAttribute.ShortForm
				};

				doc.AcceptedValues.AddRange(AcceptedTokens(property.PropertyType, ValueLabel));
				documentation.AcceptedTokens.Add(doc);
			}

			if (attribute is CommandLineValuesAttribute)
			{
				var doc = new CommandLineValueDocumentation
				{
					Description = Description,
					ShortDescription = ShortDescription
				};

				doc.AcceptedValues.AddRange(AcceptedTokens(property.PropertyType, ValueLabel));
				documentation.AcceptedTokens.Add(doc);
			}
		}

		private IEnumerable<string> AcceptedTokens([NotNull] Type type, string valueName)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			valueName = valueName.NormalizeNull() ?? "value";

			if (type.IsArray)
			{
				foreach (var result in AcceptedTokens(type.GetElementType(), valueName))
				{
					yield return result;
				}

				yield break;
			}

			if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type) && type != typeof(string))
			{
				if (type.GenericTypeArguments.Length == 1)
				{
					var gtarg = type.GenericTypeArguments[0];

					foreach (var result in AcceptedTokens(gtarg, valueName))
					{
						yield return result;
					}

					yield break;
				}

				yield return valueName;
				yield break;
			}

			if (type == typeof(string))
			{
				yield return valueName;
				yield break;
			}

			var typeInfo = type.GetTypeInfo();
			if (typeInfo.IsEnum)
			{
				foreach (var value in Enum.GetNames(type))
				{
					yield return value;
				}

				yield break;
			}

			yield return valueName;
		}

		private bool AcceptsMultiple([NotNull] PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException(nameof(propertyInfo));
			}

			if (propertyInfo.PropertyType.IsArray)
			{ 			
				return true;
			}

			if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.PropertyType != typeof(string))
			{
				return true;
			}

			return false;
		}
	}
}
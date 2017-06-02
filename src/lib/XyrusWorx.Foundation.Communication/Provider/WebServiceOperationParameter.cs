using System;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Provider
{
	class WebServiceOperationParameter
	{
		public WebServiceOperationParameter([NotNull] ParameterInfo parameter)
		{
			if (parameter == null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			Name = parameter.Name;
			Type = parameter.ParameterType;
			FromQuery = parameter.GetCustomAttribute<FromQueryAttribute>() != null;
			FromBody = parameter.GetCustomAttribute<FromBodyAttribute>() != null;
			DefaultValue = parameter.IsOptional ? parameter.DefaultValue : null;

			if (FromBody)
			{
				FromQuery = false;
			}
		}

		[NotNull] public string Name { get; }
		[NotNull] public Type Type { get; }

		public bool FromBody { get; }
		public bool FromQuery { get; }

		[CanBeNull]
		public object DefaultValue { get; }
	}
}
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	//Based on the project from http://web.archive.org/web/20130316081653/http://tranxcoder.wordpress.com/2008/10/12/customizing-lookful-wpf-controls-take-2/

	[PublicAPI]
	public class ArithmeticConverter : IMultiValueConverter
	{
		public ArithmeticOperation Operation { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var numbers = values.Select(x => x?.ToString().TryDeserialize<double>() ?? 0).ToArray();
			var result = 0.0;

			switch (Operation)
			{
				case ArithmeticOperation.Add:
					result = numbers.Aggregate((a, b) => a + b);
					break;
				case ArithmeticOperation.Subtract:
					result = numbers.Aggregate((a, b) => a - b);
					break;
				case ArithmeticOperation.Multiply:
					result = numbers.Aggregate((a, b) => a * b);
					break;
				case ArithmeticOperation.Divide:
					// ReSharper disable PossibleMultipleEnumeration
					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (numbers.Skip(1).Any(x => x == 0))
					{
						result = double.NaN;
					}
					else
					{
						result = numbers.Aggregate((a, b) => a / b);
					}
					// ReSharper restore PossibleMultipleEnumeration
					break;
			}

			if (targetType == typeof(int)) return (int)result;
			if (targetType == typeof(uint)) return (uint)result;
			if (targetType == typeof(short)) return (short)result;
			if (targetType == typeof(ushort)) return (ushort)result;
			if (targetType == typeof(float)) return (float)result;
			if (targetType == typeof(decimal)) return (decimal)result;
			if (targetType == typeof(byte)) return (byte)result;
			if (targetType == typeof(sbyte)) return (sbyte)result;
			if (targetType == typeof(string)) return result.ToString(culture);

			return result;
		}

		object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
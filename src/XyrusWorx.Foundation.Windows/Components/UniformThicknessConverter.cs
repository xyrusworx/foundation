using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class UniformThicknessConverter : IValueConverter
	{
		public ArithmeticOperation ComponentOperation { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var v = value?.ToString().TryDeserialize<double>() ?? 0;
			return new Thickness(v);
		}
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
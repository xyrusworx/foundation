using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class BooleanConverter : IValueConverter
	{
		public object TrueValue { get; set; }
		public object FalseValue { get; set; }
		public object NullValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool))
			{
				return NullValue;
			}

			var v = (bool)value;
			return v ? TrueValue : FalseValue;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
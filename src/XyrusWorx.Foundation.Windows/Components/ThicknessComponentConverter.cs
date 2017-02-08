using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class ThicknessComponentConverter : IValueConverter
	{
		public ArithmeticOperation ComponentOperation { get; set; }

		public bool Left { get; set; }
		public bool Top { get; set; }
		public bool Right { get; set; }
		public bool Bottom { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Thickness))
			{
				return null;
			}

			var data = (Thickness)value;
			var values = new List<double>();

			if (Left) values.Add(data.Left);
			if (Top) values.Add(data.Top);
			if (Right) values.Add(data.Right);
			if (Bottom) values.Add(data.Bottom);

			return new ArithmeticConverter {Operation = ComponentOperation}.Convert(values.OfType<object>().ToArray(), targetType, parameter, culture);
		}
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
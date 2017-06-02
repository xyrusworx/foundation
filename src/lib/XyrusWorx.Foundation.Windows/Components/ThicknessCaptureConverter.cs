using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class ThicknessCaptureConverter : IValueConverter
	{
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
			var output = new Thickness();

			if (Left) output.Left = data.Left;
			if (Top) output.Top = data.Top;
			if (Right) output.Right = data.Right;
			if (Bottom) output.Bottom = data.Bottom;

			return output;
		}
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
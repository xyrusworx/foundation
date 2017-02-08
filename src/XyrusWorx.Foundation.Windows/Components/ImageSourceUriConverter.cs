using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class ImageSourceUriConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var uri = value as string;
			if (uri == null && value is Uri)
			{
				uri = ((Uri)value).ToString();
			}
			else if (uri == null)
			{
				return null;
			}

			return new ImageSourceConverter().ConvertFromString(uri);
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}

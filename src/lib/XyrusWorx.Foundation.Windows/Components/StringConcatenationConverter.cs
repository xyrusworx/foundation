using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class StringConcatenationConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var strings = values
				.Select(x => x?.ToString() ?? string.Empty)
				.Select(x => string.IsNullOrEmpty(x) ? x : (PrependSpace ? ($" {x}") : x));
			return string.Concat(strings);
		}

		object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		public bool PrependSpace { get; set; }
	}
}
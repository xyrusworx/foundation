using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class EmptyEnumerationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(value.CastTo<IEnumerable>()?.OfType<object>().Any() ?? false);
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}

}
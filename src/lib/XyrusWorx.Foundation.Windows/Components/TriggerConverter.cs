using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components 
{
	[PublicAPI]
	public class TriggerConverter : IValueConverter
	{
		public object Value { get; set; }
		public object Result { get; set; }
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, Value) ? Result : value;
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}
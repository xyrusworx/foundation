using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class RoundingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var p = (int)(parameter?.ToString().TryDeserialize<double>() ?? 0);
			var v = value?.ToString().TryDeserialize<double>() ?? 0;

			var result = p < 0 ? v : Math.Round(v, Math.Max(0, p));

			if (targetType == typeof (byte)) return (byte) result;
			if (targetType == typeof (sbyte)) return (sbyte) result;
			if (targetType == typeof (short)) return (short) result;
			if (targetType == typeof (ushort)) return (ushort) result;
			if (targetType == typeof (int)) return (int) result;
			if (targetType == typeof (uint)) return (uint) result;
			if (targetType == typeof (long)) return (long) result;
			if (targetType == typeof (ulong)) return (ulong) result;
			if (targetType == typeof (float)) return (float) result;
			if (targetType == typeof (double)) return result;
			if (targetType == typeof (decimal)) return (decimal)result;
			if (targetType == typeof (string)) return result.ToString("R", culture);

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var result = value?.ToString().TryDeserialize<double>() ?? 0;

			if (targetType == typeof(byte)) return (byte)result;
			if (targetType == typeof(sbyte)) return (sbyte)result;
			if (targetType == typeof(short)) return (short)result;
			if (targetType == typeof(ushort)) return (ushort)result;
			if (targetType == typeof(int)) return (int)result;
			if (targetType == typeof(uint)) return (uint)result;
			if (targetType == typeof(long)) return (long)result;
			if (targetType == typeof(ulong)) return (ulong)result;
			if (targetType == typeof(float)) return (float)result;
			if (targetType == typeof(double)) return result;
			if (targetType == typeof(decimal)) return (decimal)result;
			if (targetType == typeof(string)) return result.ToString(culture);

			return null;
		}
	}
}
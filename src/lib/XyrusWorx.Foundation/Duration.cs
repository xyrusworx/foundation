using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI, DebuggerDisplay("ShortDisplayString")]
	public struct Duration : IEquatable<Duration>, IComparable<Duration>, IComparable
	{
		private readonly double mMinutes;

		public Duration(double minutes)
		{
			if (minutes < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minutes));
			}

			mMinutes = minutes;
		}
		public Duration(TimeSpan timeSpan)
		{
			mMinutes = timeSpan.TotalMinutes;
		}
		public Duration(string durationString)
		{
			var data = Parse(durationString);
			mMinutes = data.mMinutes;
		}

		public double Weeks
		{
			get => Days / 7;
		}

		public double Days
		{
			get => Hours / 24;
		}

		public double Hours
		{
			get => Minutes / 60;
		}

		public double Minutes
		{
			get => mMinutes;
		}

		public double Seconds
		{
			get => Minutes * 60;
		}

		public double Milliseconds
		{
			get => Seconds * 1000;
		}

		public static Duration Zero
		{
			get => new Duration();
		}

		public TimeSpan ToTimeSpan() => TimeSpan.FromMinutes(mMinutes);

		public string LongDisplayString
		{
			get
			{
				var v = TimeSpan.FromMinutes(mMinutes);
				var str = new List<string>();

				if (Math.Abs(v.TotalHours - 1) < double.Epsilon)
				{
					str.Add($"{1} hour");
				}
				else if (v.TotalHours > 1)
				{
					str.Add($"{Math.Floor(v.TotalHours)} hours");
				}

				if (v.Minutes == 1)
				{
					str.Add($"{1} minute");
				}
				else if (v.Minutes > 1)
				{
					str.Add($"{v.Minutes} minutes");
				}

				if (v.Seconds == 1 && v.Milliseconds == 0)
				{
					str.Add($"{1} second");
				}
				else
				{
					str.Add($"{v.Seconds + Math.Round(v.Milliseconds/1000d, 3)} seconds");
				}

				return str.Concat(", ");
			}
		}
		public string ShortDisplayString
		{
			get => SerializeCore(this, CultureInfo.CurrentCulture);
		}

		public static Duration Parse([NotNull] string str) => Parse(str, CultureInfo.CurrentCulture);

		public static Duration Parse([NotNull] string str, [NotNull] IFormatProvider format)
		{
			if (str == null)
			{
				throw new ArgumentNullException(nameof(str));
			}
			
			if (format == null)
			{
				throw new ArgumentNullException(nameof(format));
			}

			var result = ParseCore(str, format);
			if (result.HasError)
			{
				throw new FormatException(result.ErrorDescription);
			}

			return result.Data;
		}

		public static bool TryParse(string str, out Duration value) => TryParse(str, CultureInfo.CurrentCulture, out value);

		public static bool TryParse(string str, [NotNull] IFormatProvider format, out Duration value)
		{
			if (format == null)
			{
				throw new ArgumentNullException(nameof(format));
			}

			var result = ParseCore(str, format);
			if (result.HasError)
			{
				value = default(Duration);
				return false;
			}

			value = result.Data;
			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			
			return obj is Duration && Equals((Duration)obj);
		}
		// ReSharper disable once ImpureMethodCallOnReadonlyValueField
		public bool Equals(Duration other) => mMinutes.Equals(other.mMinutes);

		// ReSharper disable once ImpureMethodCallOnReadonlyValueField
		public override int GetHashCode() => mMinutes.GetHashCode();

		// ReSharper disable once ImpureMethodCallOnReadonlyValueField
		public int CompareTo(Duration other) => mMinutes.CompareTo(other.mMinutes);
		public int CompareTo(object obj)
		{
			if (!(obj is Duration))
			{
				return -1;
			}

			return CompareTo((Duration)obj);
		}

		public static Duration operator +(Duration left, Duration right) => new Duration(left.mMinutes + right.mMinutes);
		public static Duration operator -(Duration left, Duration right) => new Duration(left.mMinutes - right.mMinutes);
		public static Duration operator *(Duration left, Duration right) => new Duration(left.mMinutes * right.mMinutes);
		public static Duration operator /(Duration left, Duration right) => new Duration(left.mMinutes / right.mMinutes);

		public static Duration operator +(Duration left, TimeSpan right) => new Duration(left.mMinutes + right.TotalMinutes);
		public static Duration operator -(Duration left, TimeSpan right) => new Duration(left.mMinutes - right.TotalMinutes);
		public static Duration operator *(Duration left, TimeSpan right) => new Duration(left.mMinutes * right.TotalMinutes);
		public static Duration operator /(Duration left, TimeSpan right) => new Duration(left.mMinutes / right.TotalMinutes);

		public static TimeSpan operator +(TimeSpan left, Duration right) => TimeSpan.FromMinutes(left.TotalMinutes + right.mMinutes);
		public static TimeSpan operator -(TimeSpan left, Duration right) => TimeSpan.FromMinutes(left.TotalMinutes - right.mMinutes);
		public static TimeSpan operator *(TimeSpan left, Duration right) => TimeSpan.FromMinutes(left.TotalMinutes * right.mMinutes);
		public static TimeSpan operator /(TimeSpan left, Duration right) => TimeSpan.FromMinutes(left.TotalMinutes / right.mMinutes);

		public static DateTime operator +(DateTime left, Duration right) => left.Subtract(TimeSpan.FromMinutes(right.mMinutes));
		public static DateTime operator -(DateTime left, Duration right) => left.Subtract(TimeSpan.FromMinutes(right.mMinutes));
		public static DateTime operator *(DateTime left, Duration right) => left.Subtract(TimeSpan.FromMinutes(right.mMinutes));
		public static DateTime operator /(DateTime left, Duration right) => left.Subtract(TimeSpan.FromMinutes(right.mMinutes));

		public static bool operator ==(Duration left, Duration right) => left.Equals(right);
		public static bool operator !=(Duration left, Duration right) => !left.Equals(right);

		public static bool operator <(Duration left, Duration right) => left.CompareTo(right) < 0;
		public static bool operator >(Duration left, Duration right) => left.CompareTo(right) > 0;
		public static bool operator <=(Duration left, Duration right) => left.CompareTo(right) <= 0;
		public static bool operator >=(Duration left, Duration right) => left.CompareTo(right) >= 0;

		public static implicit operator Duration(string durationString) => new Duration(durationString);
		public static implicit operator Duration(TimeSpan timeSpan) => new Duration(timeSpan);
		public static explicit operator TimeSpan(Duration duration) => duration.ToTimeSpan();

		public override string ToString() => ToString(CultureInfo.CurrentCulture);
		public string ToString(IFormatProvider format) => SerializeCore(this, format);

		private static string SerializeCore(Duration value, IFormatProvider format)
		{
			var timeSpan = TimeSpan.FromMinutes(value.mMinutes);
			var sb = new List<string>();

			if (timeSpan.TotalMilliseconds < 1)
			{
				return "0m";
			}

			if (timeSpan.TotalDays > 7)
			{
				var w = (int)Math.Floor(timeSpan.TotalDays) / 7;
				var d = (int)Math.Floor(timeSpan.TotalDays) % 7;

				sb.Add($"{w.ToString(format)}w");

				if (d >= 1)
				{
					sb.Add($"{d.ToString(format)}d");
				}
			}
			else if (timeSpan.TotalDays >= 1)
			{
				sb.Add($"{timeSpan.Days.ToString(format)}d");
			}

			if (timeSpan.Hours >= 1)
			{
				sb.Add($"{timeSpan.Hours.ToString(format)}h");
			}

			if (timeSpan.Minutes >= 1)
			{
				sb.Add($"{timeSpan.Minutes.ToString(format)}m");
			}

			if (timeSpan.Seconds >= 1)
			{
				sb.Add($"{timeSpan.Seconds.ToString(format)}s");
			}

			if (timeSpan.Milliseconds >= 1)
			{
				sb.Add($"{timeSpan.Milliseconds.ToString(format)}ms");
			}

			return string.Join(" ", sb);
		}
		private static Result<Duration> ParseCore(string str, IFormatProvider format)
		{
			if (str.NormalizeNull() == null)
			{
				return Err("The input string is empty");
			}

			var tokens = Regex.Split(str, @"\s+");
			var timeSpan = TimeSpan.Zero;
			var tokenExpression = new Regex(@"([-+]?[0-9]*\.?[0-9]+)((?:ms|[smhdw])?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			foreach (var token in tokens)
			{
				if (string.IsNullOrWhiteSpace(token))
				{
					continue;
				}

				var match = tokenExpression.Match(token.Trim());
				if (!match.Success)
				{
					return Err($"Syntax error at '{token}'");
				}

				double value;
				if (!double.TryParse(match.Groups[1].Value, NumberStyles.Float, format, out value))
				{
					return Err($"Error processing numeric value '{match.Groups[1].Value}' at '{token}'");
				}

				switch (match.Groups[2].Value.ToLower())
				{
					case "ms":
						timeSpan = timeSpan.Add(TimeSpan.FromMilliseconds(value));
						break;
					case "s":
						timeSpan = timeSpan.Add(TimeSpan.FromSeconds(value));
						break;
					case "m":
						timeSpan = timeSpan.Add(TimeSpan.FromMinutes(value));
						break;
					case "h":
						timeSpan = timeSpan.Add(TimeSpan.FromHours(value));
						break;
					case "d":
						timeSpan = timeSpan.Add(TimeSpan.FromDays(value));
						break;
					case "w":
						timeSpan = timeSpan.Add(TimeSpan.FromDays(value * 7));
						break;
					default:
						return Err($"Unknown unit '{match.Groups[2].Value}' at '{token}'. Valid units are: milliseconds (ms), seconds (s), minutes (m), hours (h), days (d) and weeks (w).");
				}
			}

			return new Result<Duration> { Data = new Duration(timeSpan) };
		}
		private static Result<Duration> Err(string message) => Result.CreateError<Result<Duration>>(message);
	}
}
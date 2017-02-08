using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace XyrusWorx
{
	[PublicAPI]
	public static class StringUtils
	{
		public static string Hash(this string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return new string('0', 32);
			}

			var md5 = MD5.Create();
			var inputBytes = Encoding.ASCII.GetBytes(input);
			var hash = md5.ComputeHash(inputBytes);

			var builder = new StringBuilder();
			foreach (var b in hash)
			{
				builder.Append(b.ToString("X2").ToLower());
			}
			return builder.ToString();
		}

		public static StringKey AsKey([NotNull] this string input) => new StringKey(input);
		public static Key<T> AsKey<T>(this T input) where T: struct => new Key<T>(input);

		public static StringKeySequence AsKey([NotNull] this IEnumerable<string> input) => new StringKeySequence(input.Select(x => x.AsKey()).ToArray());
		public static KeySequence<T> AsKey<T>([NotNull] this IEnumerable<T> input) where T : struct => new KeySequence<T>(input.Select(x => x.AsKey()).ToArray());

		public static object TryDeserialize(this string instance, [NotNull] Type type, IFormatProvider formatProvider = null)
		{
			return TryDeserialize(instance, type.GetTypeInfo(), formatProvider);
		}
		public static object TryDeserialize(this string instance, [NotNull] TypeInfo type, IFormatProvider formatProvider = null)
		{
			object result;
			TryDeserialize(instance, type, out result, formatProvider);
			return result;
		}
		public static object TryDeserialize(this string instance, IFormatProvider formatProvider = null)
		{
			object result;
			TryDeserialize(instance, out result, formatProvider);
			return result;
		}

		public static bool TryDeserialize(this string instance, [NotNull] Type type, out object result, IFormatProvider formatProvider = null)
		{
			return TryDeserialize(instance, type.GetTypeInfo(), out result, formatProvider);
		}
		public static bool TryDeserialize(this string instance, [NotNull] TypeInfo type, out object result, IFormatProvider formatProvider = null)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (Equals(type, typeof(string).GetTypeInfo()))
			{
				result = instance;
				return true;
			}

			object defaultValue = null;

			if (type.IsValueType)
			{
				defaultValue = Activator.CreateInstance(type.AsType());
			}

			instance = instance ?? string.Empty;
			formatProvider = formatProvider ?? CultureInfo.CurrentCulture;

			if (Equals(type, typeof(bool).GetTypeInfo()))
			{
				if (string.Equals(instance, "true", StringComparison.OrdinalIgnoreCase))
				{
					result = true;
					return true;
				}

				if (string.Equals(instance, "false", StringComparison.OrdinalIgnoreCase))
				{
					result = false;
					return true;
				}

				result = defaultValue;
				return false;
			}

			if (Equals(type, typeof(byte).GetTypeInfo()))
			{
				var parsed = byte.TryParse(instance, NumberStyles.Integer, formatProvider, out byte value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(sbyte).GetTypeInfo()))
			{
				var parsed = sbyte.TryParse(instance, NumberStyles.Integer, formatProvider, out sbyte value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(short).GetTypeInfo()))
			{
				var parsed = short.TryParse(instance, NumberStyles.Integer, formatProvider, out short value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(ushort).GetTypeInfo()))
			{
				var parsed = ushort.TryParse(instance, NumberStyles.Integer, formatProvider, out ushort value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(int).GetTypeInfo()))
			{
				var parsed = int.TryParse(instance, NumberStyles.Integer, formatProvider, out int value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(uint).GetTypeInfo()))
			{
				var parsed = uint.TryParse(instance, NumberStyles.Integer, formatProvider, out uint value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(long).GetTypeInfo()))
			{
				var parsed = long.TryParse(instance, NumberStyles.Integer, formatProvider, out long value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(ulong).GetTypeInfo()))
			{
				var parsed = ulong.TryParse(instance, NumberStyles.Integer, formatProvider, out ulong value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(float).GetTypeInfo()))
			{
				var parsed = float.TryParse(instance, NumberStyles.Float, formatProvider, out float value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(double).GetTypeInfo()))
			{
				var parsed = double.TryParse(instance, NumberStyles.Float, formatProvider, out double value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(decimal).GetTypeInfo()))
			{
				var parsed = decimal.TryParse(instance, NumberStyles.Float, formatProvider, out decimal value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(Guid).GetTypeInfo()))
			{
				var parsed = Guid.TryParse(instance, out var value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (Equals(type, typeof(DateTime).GetTypeInfo()))
			{
				var parsed = DateTime.TryParse(instance, out var value);
				result = parsed ? value : defaultValue;
				return parsed;
			}

			if (type.IsEnum)
			{
				var field = type.DeclaredFields.FirstOrDefault(x => x.IsStatic && string.Equals(x.Name, instance, StringComparison.OrdinalIgnoreCase));
				if (field == null)
				{
					result = defaultValue;
					return false;
				}

				result = field.GetValue(null);
				return true;
			}

			result = defaultValue;
			return false;
		}
		public static bool TryDeserialize(this string instance, out object result, IFormatProvider formatProvider = null)
		{
			if (string.IsNullOrEmpty(instance))
			{
				result = null;
				return false;
			}

			formatProvider = formatProvider ?? CultureInfo.CurrentCulture;

			if (int.TryParse(instance, NumberStyles.Integer, formatProvider, out var int32))
			{
				result = int32;
				return true;
			}

			if (long.TryParse(instance, NumberStyles.Integer, formatProvider, out var int64))
			{
				result = int64;
				return true;
			}

			if (double.TryParse(instance, NumberStyles.Float, formatProvider, out var float64))
			{
				result = float64;
				return true;
			}

			if (Guid.TryParse(instance, out var guid))
			{
				result = guid;
				return true;
			}

			if (DateTime.TryParse(instance, out var datetime))
			{
				result = datetime;
				return true;
			}

			result = null;
			return false;
		}

		public static T TryDeserialize<T>(this string instance, IFormatProvider formatProvider = null) where T : struct
		{
			return (T)TryDeserialize(instance, typeof(T), formatProvider);
		}
		public static T? TryDeserialize<T>(this string instance, [NotNull] TryParseFunc<T> parseFunc, T? defaultValue = null) where T : struct
		{
			if (parseFunc == null)
			{
				throw new ArgumentNullException(nameof(parseFunc));
			}

			if (string.IsNullOrEmpty(instance))
			{
				return null;
			}

			T t;
			if (!parseFunc(instance, out t))
			{
				return null;
			}

			return t;
		}

		public static void TryConsume<TIn>(this TIn? instance, [NotNull] Action<TIn> action) where TIn : struct
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (instance.HasValue)
			{
				action(instance.Value);
			}
		}
		public static void TryConsume<TIn>(this TIn instance, [NotNull] Action<TIn> action)
			where TIn : class
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (instance != null)
			{
				action(instance);
			}

			
		}

		public static TOut? TryTransform<TIn, TOut>(this TIn? instance, [NotNull] Func<TIn, TOut> transform, TOut? elseValue = null) 
			where TIn : struct 
			where TOut: struct
		{
			if (transform == null)
			{
				throw new ArgumentNullException(nameof(transform));
			}

			if (instance == null)
			{
				return elseValue;
			}

			return transform(instance.Value);
		}
		public static TOut TryTransform<TIn, TOut>(this TIn instance, [NotNull] Func<TIn, TOut> transform, TOut elseValue = default(TOut))
			where TIn : class
		{
			if (transform == null)
			{
				throw new ArgumentNullException(nameof(transform));
			}

			if (instance == null)
			{
				return elseValue;
			}

			return transform(instance);
		}

		public static string WordWrap(this string instance, int wrapRight, string prefix = null, string firstPrefix = null)
		{
			if (instance == null)
			{
				return null;
			}

			var wrapLeft = (prefix?.Length).GetValueOrDefault();
			if (wrapRight <= wrapLeft)
			{
				throw new ArgumentOutOfRangeException(nameof(wrapRight));
			}

			var currentLine = firstPrefix ?? prefix ?? string.Empty;
			var lineLength = wrapRight - wrapLeft;
			var lines = new List<string>();

			var paragraphs = instance.Replace("\r", "").Split('\n');

			foreach (var paragraph in paragraphs)
			{
				var words = paragraph.Split(' ');
				foreach (var word in words)
				{
					if (currentLine.Length + word.Length + 1 > lineLength)
					{
						lines.Add(currentLine.TrimEnd());
						currentLine = string.Empty;
					}

					currentLine += word + ' ';

					while (currentLine.Length > lineLength)
					{
						currentLine = currentLine.TrimEnd();

						var chunk = currentLine.Substring(0, lineLength);
						var rest = currentLine.Substring(lineLength);

						lines.Add(chunk.TrimEnd());
						currentLine = rest + ' ';
					}
				}

				if (!string.IsNullOrWhiteSpace(currentLine))
				{
					lines.Add(currentLine);
				}

				currentLine = string.Empty;
			}

			return string.Join(Environment.NewLine, 
				lines.Take(1).Select(x => firstPrefix + x).Concat(
				lines.Skip(1).Select(x => prefix + x)));
		}

		public static string ToSingleLine(this string instance)
		{
			if (instance == null)
			{
				return null;
			}

			var text = instance.Replace("\r", "").Replace("\n", " ").Replace("\t", " ");
			var regex = new Regex(@"(\x20)\x20+", RegexOptions.Singleline);

			return regex.Replace(text, "$1");
		}

		public static string Escape(this string instance)
		{
			if (instance == null)
			{
				return null;
			}

			instance = instance.Replace("\\", @"\\" );
			instance = instance.Replace("\r", @"\r" );
			instance = instance.Replace("\n", @"\n" );
			instance = instance.Replace("\t", @"\t" );
			instance = instance.Replace("\0", @"\0" );
			instance = instance.Replace("\"", @"\""");

			return instance;
		}
		public static string Unescape(this string instance)
		{
			if (instance == null)
			{
				return null;
			}

			instance = Regex.Replace(instance, @"([^\\])\\r",   "$1\r");
			instance = Regex.Replace(instance, @"([^\\])\\n",   "$1\n");
			instance = Regex.Replace(instance, @"([^\\])\\t",   "$1\t");
			instance = Regex.Replace(instance, @"([^\\])\\0",   "$1\0");
			instance = Regex.Replace(instance, @"([^\\])\\""",  "$1\"");
			instance = Regex.Replace(instance, @"([^\\])\\\\",  "$1\\");

			return instance;
		}

		public static string NormalizeNull(this string instance)
		{
			return string.IsNullOrWhiteSpace(instance) ? null : instance;
		}
		public static string ToRegexLiteral(this string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			foreach (var c in input)
			{
				sb.AppendFormat("\\u{0:x4}", (ushort)c);
			}
			return sb.ToString();
		}
		public static string Concat(this IEnumerable<string> strings, string separator = " ")
		{
			var resultValue = string.Empty;
			if (strings == null)
			{
				return resultValue;
			}

			separator = separator ?? string.Empty;

			foreach (var str in strings)
			{
				var currentSeparator = string.IsNullOrWhiteSpace(resultValue)
					? string.Empty
					: separator;

				if (!string.IsNullOrWhiteSpace(str))
				{
					resultValue += currentSeparator + str;
				}
			}

			if (!string.IsNullOrEmpty(separator))
			{
				var reSeparator = separator.ToRegexLiteral();
				var reMoreThanOneSubsequentSeparator = $"{reSeparator}[{reSeparator}]+";

				resultValue = Regex.Replace(resultValue, reMoreThanOneSubsequentSeparator, separator);
			}

			return resultValue?.Trim();
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public static string Unindent(this string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return null;
			}

			var lines = Regex.Split(input.Trim('\r', '\n'), @"[\r\n]+");
			var lineInfos =
				from line in lines
				let normalizedLine = line.Replace("\t", "    ")
				//where !string.IsNullOrWhiteSpace(normalizedLine)
				let countSpaces = Regex.Match(line, @"^(\s*).*$").Groups[1].Value.Length
				select new
				{
					Text = line,
					IsEmpty = string.IsNullOrWhiteSpace(normalizedLine),
					CountSpaces = countSpaces
				};
			var leastSpaceCount = lineInfos.Where(x => !x.IsEmpty).Min(x => x.CountSpaces);

			var result = new StringBuilder();

			foreach (var line in lineInfos)
			{
				var trimmedLine = leastSpaceCount < line.Text.Length ? line.Text.Substring(leastSpaceCount) : string.Empty;
				var normalizedLine = string.Empty;

				var tabCounter = 0;
				var counter = 0;

				if (line.IsEmpty)
				{
					result.AppendLine("");
					continue;
				}

				for (var i = 0; i < trimmedLine.Length && trimmedLine.ElementAtOrDefault(i) == ' '; i++)
				{
					if (tabCounter++ >= 4)
					{
						normalizedLine += '\t';
						tabCounter = 0;
					}

					counter++;
				}

				if (tabCounter > 0)
				{
					normalizedLine += new string(' ', tabCounter);
				}

				if (counter < trimmedLine.Length)
				{
					normalizedLine += trimmedLine.Substring(counter);
					result.AppendLine(normalizedLine);
				}
			}

			return result.ToString().TrimEnd();
		}

		private static byte[] GetBytes(Guid input)
		{
			var kstr = input.ToString("n");
			var kb = new byte[16];

			for (int i = 0, j = 0; i < kstr.Length && j < kb.Length; i += 2, j++)
			{
				kb[j] = byte.Parse(kstr.Substring(i, 2), NumberStyles.HexNumber);
			}

			return kb;
		}
	}
}

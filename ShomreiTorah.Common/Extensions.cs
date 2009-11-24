using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ShomreiTorah.Common {
	///<summary>Contains miscellaneous extension methods.</summary>
	public static class Extensions {
		#region Collections
		///<summary>Creates a ReadOnlyCollection that wraps the items currently an enumerable.</summary>
		///<param name="items">The items to copy into the collection.  Changes to the items will be ignored.</param>
		public static ReadOnlyCollection<T> ReadOnlyCopy<T>(this IEnumerable<T> items) { return new ReadOnlyCollection<T>(items.ToArray()); }

		///<summary>Checks whether the given enumerable has at least the given number of elements.</summary>
		///<param name="items">The enumerable to check.</param>
		///<param name="minCount">The minimum number of elements</param>
		///<returns>True if items has at least minCount elements.</returns>
		public static bool Has<T>(this IEnumerable<T> items, int minCount) {
			if (items == null) throw new ArgumentNullException("items");
			if (minCount < 0) throw new ArgumentOutOfRangeException("minCount");

			if (minCount == 0) return true;

			var collection = items as ICollection<T>;
			if (collection != null) return minCount <= collection.Count;
			var collection2 = items as ICollection;
			if (collection2 != null) return minCount <= collection2.Count;

			int i = 0;
			using (var enumerator = items.GetEnumerator()) {
				while (enumerator.MoveNext()) {
					i++;
					if (i >= minCount) return true;
				}
			}
			return false;
		}

		///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
		///<param name="items">The enumerable to search.</param>
		///<param name="predicate">The expression to test the items against.</param>
		///<returns>The index of the first matching item, or -1 if no items match.</returns>
		public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
			if (items == null) throw new ArgumentNullException("items");
			if (predicate == null) throw new ArgumentNullException("predicate");

			int retVal = 0;
			foreach (var item in items) {
				if (predicate(item)) return retVal;
				retVal++;
			}
			return -1;
		}

		///<summary>Adds zero or more items to a collection.</summary>
		public static void AddRange<TItem, TElement>(this ICollection<TElement> collection, params TItem[] items) where TItem : TElement { collection.AddRange((IEnumerable<TItem>)items); }
		///<summary>Adds zero or more items to a collection.</summary>
		public static void AddRange<TItem, TElement>(this ICollection<TElement> collection, IEnumerable<TItem> items)
			where TItem : TElement {
			if (collection == null) throw new ArgumentNullException("collection");
			if (items == null) throw new ArgumentNullException("items");

			foreach (var item in items)
				collection.Add(item);
		}
		#endregion

		#region Strings
		///<summary>Appends a list of strings to a StringBuilder, separated by a separator string.</summary>
		///<param name="builder">The StringBuilder to append to.</param>
		///<param name="strings">The strings to append.</param>
		///<param name="separator">A string to append between the strings.</param>
		public static StringBuilder AppendJoin(this StringBuilder builder, IEnumerable<string> strings, string separator) {
			if (builder == null) throw new ArgumentNullException("builder");
			if (strings == null) throw new ArgumentNullException("strings");
			if (separator == null) throw new ArgumentNullException("separator");

			bool first = true;

			foreach (var str in strings) {
				if (first)
					first = false;
				else
					builder.Append(separator);

				builder.Append(str);
			}

			return builder;
		}

		///<summary>Combines a collection of strings into a single string.</summary>
		public static string Join<T>(this IEnumerable<T> strings, string separator, Func<T, string> selector) { return strings.Select(selector).Join(separator); }
		///<summary>Combines a collection of strings into a single string.</summary>
		public static string Join(this IEnumerable<string> strings, string separator) { return new StringBuilder().AppendJoin(strings, separator).ToString(); }

		///<summary>Gets the ordinal suffix of a number.</summary>
		public static string GetSuffix(this int number) {
			if (number > 10 && number < 19)
				return "th";
			switch (number % 10) {
				case 1: return "st";
				case 2: return "nd";
				case 3: return "rd";
				default: return "th";
			}
		}
		#endregion

		#region Streams
		///<summary>Writes raw bytes to a stream.</summary>
		///<param name="stream">The stream to write to.</param>
		///<param name="data">The bytes to write.</param>
		///<remarks>This method removes the need for temporary variables to get the length of the byte array.</remarks>
		public static void WriteAllBytes(this Stream stream, byte[] data) { stream.Write(data, 0, data.Length); }

		#region Numbers
		///<summary>Writes a number to a stream.</summary>
		public static void Write(this Stream stream, short value) { stream.Write(BitConverter.GetBytes(value), 0, 2); }
		///<summary>Writes a number to a stream.</summary>
		public static void Write(this Stream stream, int value) { stream.Write(BitConverter.GetBytes(value), 0, 4); }
		///<summary>Writes a number to a stream.</summary>
		public static void Write(this Stream stream, long value) { stream.Write(BitConverter.GetBytes(value), 0, 8); }

		///<summary>Writes a number to a stream.</summary>
		[CLSCompliant(false)]
		public static void Write(this Stream stream, ushort value) { stream.Write(BitConverter.GetBytes(value), 0, 2); }
		///<summary>Writes a number to a stream.</summary>
		[CLSCompliant(false)]
		public static void Write(this Stream stream, uint value) { stream.Write(BitConverter.GetBytes(value), 0, 4); }
		///<summary>Writes a number to a stream.</summary>
		[CLSCompliant(false)]
		public static void Write(this Stream stream, ulong value) { stream.Write(BitConverter.GetBytes(value), 0, 8); }

		///<summary>Writes a number to a stream.</summary>
		public static void Write(this Stream stream, float value) { stream.Write(BitConverter.GetBytes(value), 0, 4); }
		///<summary>Writes a number to a stream.</summary>
		public static void Write(this Stream stream, double value) { stream.Write(BitConverter.GetBytes(value), 0, 8); }

		///<summary>Writes a number to a stream.</summary>
		public static void Write(this Stream stream, char value) { stream.Write(BitConverter.GetBytes(value), 0, 2); }
		///<summary>Writes a number to a stream.</summary>
		public static void Write(this Stream stream, bool value) { stream.Write(BitConverter.GetBytes(value), 0, 1); }

		[ThreadStatic]
		static byte[] convertBuffer;
		static byte[] Read(this Stream stream, int length) {
			if (convertBuffer == null) convertBuffer = new byte[16];
			stream.Read(convertBuffer, 0, length);
			return convertBuffer;
		}
		///<summary>Reads a number from a stream.</summary>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "How?")]
		public static T Read<T>(this Stream stream) {
			switch (typeof(T).Name) {
				case "Byte": return (T)(object)stream.ReadByte();
				case "Int16": return (T)(object)BitConverter.ToInt16(stream.Read(2), 0);
				case "Int32": return (T)(object)BitConverter.ToInt32(stream.Read(4), 0);
				case "Int64": return (T)(object)BitConverter.ToInt64(stream.Read(8), 0);

				case "UInt16": return (T)(object)BitConverter.ToUInt16(stream.Read(2), 0);
				case "UInt32": return (T)(object)BitConverter.ToUInt32(stream.Read(4), 0);
				case "UInt64": return (T)(object)BitConverter.ToUInt64(stream.Read(8), 0);

				case "Single": return (T)(object)BitConverter.ToSingle(stream.Read(4), 0);
				case "Double": return (T)(object)BitConverter.ToDouble(stream.Read(8), 0);

				case "Char": return (T)(object)BitConverter.ToChar(stream.Read(2), 0);
				case "Boolean": return (T)(object)BitConverter.ToBoolean(stream.Read(1), 0);
				default:
					throw new InvalidOperationException("Cannot read type " + typeof(T));
			}
		}
		static readonly Encoding stringEncoding = Encoding.UTF8;
		///<summary>Writes a Pascal string to a stream.</summary>
		///<param name="target">The stream to write to.</param>
		///<param name="text">The string to write.</param>
		public static void WriteString(this Stream target, string text) {
			var bytes = stringEncoding.GetBytes(text);
			target.Write(bytes.Length);
			target.Write(bytes, 0, bytes.Length);
		}
		///<summary>Reads a Pascal string from a stream.</summary>
		///<param name="source">The stream to read from.</param>
		public static string ReadString(this Stream source) {
			var byteCount = source.Read<int>();
			if (byteCount == 0) return String.Empty;					//WCF can't handle Read(*,*,0) and closes the stream
			var bytes = new byte[byteCount];
			source.Read(bytes, 0, byteCount);
			return stringEncoding.GetString(bytes);
		}
		#endregion


		///<summary>Copies one stream to another.</summary>
		///<param name="from">The stream to copy from.  This stream must be readable.</param>
		///<param name="to">The stream to copy to.  This stream must be writable.</param>
		///<returns>The number of bytes copied.</returns>
		public static long CopyTo(this Stream from, Stream to) { return from.CopyTo(to, null); }
		///<summary>Copies one stream to another.</summary>
		///<param name="from">The stream to copy from.  This stream must be readable.</param>
		///<param name="to">The stream to copy to.  This stream must be writable.</param>
		///<param name="progress">An IProgressReporter implementation to report the progress of the upload.</param>
		///<returns>The number of bytes copied.</returns>
		public static long CopyTo(this Stream from, Stream to, IProgressReporter progress) {
			if (from == null) throw new ArgumentNullException("from");
			if (to == null) throw new ArgumentNullException("to");

			if (!from.CanRead) throw new ArgumentException("Source stream must be readable", "from");
			if (!to.CanWrite) throw new ArgumentException("Source stream must be writable", "to");

			if (progress != null) {
				try {
					progress.Maximum = from.Length > int.MaxValue ? -1 : (int)from.Length;
				} catch (NotSupportedException) { progress.Maximum = -1; }
			}

			long retVal = 0;
			var buffer = new byte[4096];
			while (true) {
				var bytesRead = from.Read(buffer, 0, buffer.Length);

				if (progress != null && progress.Maximum > 0) progress.Progress = (int)retVal;
				if (progress != null && progress.WasCanceled) return -1;

				retVal += bytesRead;
				if (bytesRead == 0) return retVal;
				to.Write(buffer, 0, bytesRead);
			}
		}
		#endregion

		#region Cryptography
		///<summary>Encrypts data.</summary>
		///<param name="algorithm">The symetric algorithm to encrypt the data with.</param>
		///<param name="plaintext">The data to encrypt.</param>
		///<returns>The encrypted data.</returns>
		public static byte[] Encrypt(this SymmetricAlgorithm algorithm, byte[] plaintext) {
			return algorithm.CreateEncryptor().TransformBytes(plaintext);
		}

		///<summary>Decrypts data.</summary>
		///<param name="algorithm">The symetric algorithm to encrypt the data with.</param>
		///<param name="cipherText">The encrypted data.</param>
		///<returns>The decrypted data.</returns>
		public static byte[] Decrypt(this SymmetricAlgorithm algorithm, byte[] cipherText) {
			return algorithm.CreateDecryptor().TransformBytes(cipherText);
		}

		///<summary>Runs a byte array through an ICryptoTransform.</summary>
		///<param name="transform">The transform to run the data through.  This parameter will be disposed.</param>
		///<param name="data">The data to transform.</param>
		///<returns>The transformed data.</returns>
		public static byte[] TransformBytes(this ICryptoTransform transform, byte[] data) {
			using (transform)
			using (var stream = new MemoryStream(data.Length))
			using (var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write)) {
				cryptoStream.WriteAllBytes(data);
				cryptoStream.FlushFinalBlock();
				return stream.ToArray();
			}
		}

		#endregion

		#region TimeSpans
		///<summary>Returns the remainder after a TimeSpan is divided by another timespan.</summary>
		public static TimeSpan Mod(this TimeSpan dividend, TimeSpan divisor) { return TimeSpan.FromTicks(dividend.Ticks % divisor.Ticks); }

		///<summary>Rounds a TimeSpan down to a multiple of 5 minutes.</summary>
		public static TimeSpan RoundDown(this TimeSpan time) { return time.RoundDown(TimeSpan.FromMinutes(5)); }
		///<summary>Rounds a TimeSpan down.</summary>
		public static TimeSpan RoundDown(this TimeSpan time, TimeSpan roundBy) { return time - time.Mod(roundBy); }

		///<summary>Rounds a TimeSpan up to a multiple of 5 minutes.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RoundUp")]
		public static TimeSpan RoundUp(this TimeSpan time) { return time.RoundUp(TimeSpan.FromMinutes(5)); }
		///<summary>Rounds a TimeSpan up.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RoundUp")]
		public static TimeSpan RoundUp(this TimeSpan time, TimeSpan roundBy) {
			if (time.Mod(roundBy) == TimeSpan.Zero)
				return time;
			return time.RoundDown(roundBy) + roundBy;
		}
		///<summary>Converts the value of a timespan to a particular string representation.</summary>
		public static string ToString(this TimeSpan time, string format, IFormatProvider provider) { return (DateTime.MinValue + time).ToString(format, provider); }
		#endregion

		#region Hebrew Numbers
		static readonly string[] singleNumbersזכר = new[] { null, "אחד", "שנים", "שלשה", "ארבעה", "חמשה", "ששה", "שבעה", "שמונה", "תשעה", "עשרה" };
		static readonly string[] singleNumbersנקבה = new[] { null, "אחת", "שתים", "שלש", "ארבע", "חמש", "שש", "שבע", "שמונה", "תשע", "עשר" };

		static readonly string[] tensDigits = new[] { null, null, "עשרים", "שלשים", "ארבעים", "חמשים", "ששים", "שבעים", "שמנים", "תשעים" };

		///<summary>Converts a number to a Hebrew string.</summary>
		///<param name="number">The number to format.</param>
		///<param name="format">The type of string to return.</param>
		///<returns>A Hebrew string according to format.</returns>
		public static string ToHebrewString(this int number, HebrewNumberFormat format) {
			if (number <= 0) throw new ArgumentOutOfRangeException("number", "Zero and negative numbers cannot be Hebraicized.");
			switch (format) {
				case HebrewNumberFormat.Letter:
				case HebrewNumberFormat.LetterQuoted:
					//Adapted from .Net source
					#region Letter
					var retVal = new StringBuilder(5);
					char tensChar = '\0', unitsChar;               // tens and units chars
					int hundreds, tens;							   // hundreds and tens values

					hundreds = number / 100;

					if (hundreds > 0) {
						number -= hundreds * 100;
						for (int i = 0; i < (hundreds / 4); i++) {
							retVal.Append('ת');
						}

						int remains = hundreds % 4;
						if (remains > 0)
							retVal.Append((char)('צ' + remains));
					}

					tens = number / 10;
					number %= 10;
					if (tens > 0)
						tensChar = "יכלמנסעפצ"[tens - 1];
					//If tens == 0, the field initializer already set it to '\0'

					unitsChar = (char)(number > 0 ? ('א' + number - 1) : 0);

					if (unitsChar == 'ה' && tensChar == 'י') {
						unitsChar = 'ו';
						tensChar = 'ט';
					}

					if (unitsChar == 'ו' && tensChar == 'י') {
						unitsChar = 'ז';
						tensChar = 'ט';
					}

					if (tensChar != '\0')
						retVal.Append(tensChar);

					if (unitsChar != '\0')
						retVal.Append(unitsChar);

					if (format == HebrewNumberFormat.LetterQuoted) {
						if (retVal.Length > 1)
							retVal.Insert(retVal.Length - 1, '"');
						else
							retVal.Append('\'');
					}
					return retVal.ToString();
					#endregion
				case HebrewNumberFormat.Fullזכר:
				case HebrewNumberFormat.Fullנקבה:
					return number.ToFullHebrewString(format == HebrewNumberFormat.Fullזכר);
				default:
					throw new ArgumentOutOfRangeException("format");
			}
		}
		static string ToFullHebrewString(this int number, bool isזכר) {
			if (number <= 10) {
				return (isזכר ? singleNumbersזכר : singleNumbersנקבה)[number];
			} else if (number < 20) {
				return (number % 10).ToFullHebrewString(isזכר) + (isזכר ? " עשר" : " עשרה");
			} else if (number < 100) {
				if (number % 10 == 0)
					return tensDigits[number / 10];
				else
					return
						(number % 10).ToFullHebrewString(isזכר)
					  + " ו"
					  + tensDigits[number / 10];

			} else if (number == 100) {
				return "מאה";
			} else if (number < 200) {
				return "מאה ו" + (number % 100).ToFullHebrewString(isזכר);
			} else if (number == 200) {
				return "מאתים";
			} else if (number < 300) {
				return "מאתים ו" + (number % 100).ToFullHebrewString(isזכר);
			} else if (number < 1000) {
				if (number % 100 == 0)
					return singleNumbersנקבה[number / 100] + " מאות";
				else
					return singleNumbersנקבה[number / 100] + " מאות ו" + (number % 100).ToFullHebrewString(isזכר);

			} else if (number == 1000) {
				return "אלף";
			} else if (number < 2000) {
				return "אלף ו" + (number % 1000).ToFullHebrewString(isזכר);
			} else if (number == 2000) {
				return "שני אלפים";
			} else if (number < 3000) {
				return "שני אלפים ו" + (number % 1000).ToFullHebrewString(isזכר);
			} else if (number < 10000) {
				if (number % 1000 == 0)
					return singleNumbersזכר[number / 1000] + " אלפים";
				else
					return singleNumbersזכר[number / 1000] + " אלפים ו" + (number % 1000).ToFullHebrewString(isזכר);
			} else
				throw new NotImplementedException("Full numbers above 1000 aren't supported.");
		}
		#endregion

		#region Data
		///<summary>Gets the DataRows in a DataView.</summary>
		public static IEnumerable<DataRow> Rows(this DataView view) { return view.Cast<DataRowView>().Select(drv => drv.Row); }
		///<summary>Gets the typed DataRows in a DataView.</summary>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static IEnumerable<TRow> Rows<TRow>(this DataView view) where TRow : DataRow { return view.Cast<DataRowView>().Select(drv => (TRow)drv.Row); }

		///<summary>Sets the error description for the value described by a DataColumnChangeEventArgs.</summary>
		///<param name="args">The DataColumnChangeEventArgs for the value.</param>
		///<param name="error">The error description, or null to clear the error.</param>
		public static void SetError(this DataColumnChangeEventArgs args, string error) {
			args.Row.SetColumnError(args.Column, error);
		}
		#endregion

		#region Reflection
		///<summary>Gets a custom attribute defined on a member.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		///<returns>The first attribute of the type defined on the member, or null if there aren't any</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Return value")]
		public static TAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider) where TAttribute : Attribute {
			return provider.GetCustomAttribute<TAttribute>(false);
		}
		///<summary>Gets the first custom attribute defined on a member, or null if there aren't any.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		///<param name="inherit">Whether to look up the hierarchy chain for attributes.</param>
		///<returns>The first attribute of the type defined on the member, or null if there aren't any</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Return value")]
		public static TAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherit) where TAttribute : Attribute {
			return provider.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault();
		}
		///<summary>Gets the custom attributes defined on a member.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Return value")]
		public static TAttribute[] GetCustomAttributes<TAttribute>(this ICustomAttributeProvider provider) where TAttribute : Attribute {
			return provider.GetCustomAttributes<TAttribute>(false);
		}
		///<summary>Gets the custom attributes defined on a member.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		///<param name="inherit">Whether to look up the hierarchy chain for attributes.</param>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Return value")]
		public static TAttribute[] GetCustomAttributes<TAttribute>(this ICustomAttributeProvider provider, bool inherit) where TAttribute : Attribute {
			return (TAttribute[])provider.GetCustomAttributes(typeof(TAttribute), inherit);
		}
		#endregion

		///<summary>Reports nulls in an enumerable.</summary>
		///<remarks>Used for debugging purposes.</remarks>
		public static string ReportGaps<T>(this IEnumerable<T> items) where T : class {
			if (items == null) throw new ArgumentNullException("items");

			var retVal = new StringBuilder();
			int index = -1, lastIndex = -1;
			T lastItem = null;
			foreach (var item in items) {
				index++;
				if (item == null) continue;
				if (lastIndex < index - 1) {
					//We just came out of a gap
					if (retVal.Length > 0) {
						retVal.AppendLine();
						retVal.AppendLine(new string('—', 25));
					}
					retVal.AppendLine(lastIndex.ToString("#,0", CultureInfo.CurrentCulture).PadRight(6) + ": " + lastItem);

					var gapSize = index - lastIndex - 1;
					if (gapSize == 1)
						retVal.AppendLine(new string(' ', 8) + "1 null item");
					else
						retVal.AppendLine(new string(' ', 8) + gapSize + " null items");

					retVal.Append(index.ToString("#,0", CultureInfo.CurrentCulture).PadRight(6) + ": " + item);
				}

				lastItem = item;
				lastIndex = index;
			}
			return retVal.ToString();
		}

		//static class נקודStripper { public static readonly Regex Regex = new Regex(@"[ְֱֲֳִֵֶַָֹֻּֽֿׁׂ]");		}
		//static class נקודStripper { public static readonly Regex Regex = new Regex("[\05b0-\05bd\05bf\05c1\05c2]");		}
		//Includes Trup (0591 - 05af; see Ezra SIL; see also page 49 of the Unicode 5.1 standard at http://www.unicode.org/Public/5.1.0/charts/CodeCharts.pdf)
		static class נקודStripper { public static readonly Regex Regex = new Regex("[\u0591-\u05bd\u05bf\u05c1\u05c2\u05c4\u05c5]");		}
		///<summary>Strips all נקודות from a string.</summary>
		///<param name="original">The string to remove נקודות from.</param>
		///<returns>A copy of original with all נקודות removed.</returns>
		public static string Stripנקודות(this string original) { return נקודStripper.Regex.Replace(original, ""); }
	}
	///<summary>A format of Hebrew number </summary>
	public enum HebrewNumberFormat {
		///<summary>גמטריא-style without quotes (eg, א, ב, ג, י, יא, קכא, תריג).</summary>
		Letter,
		///<summary>גמטריא-style with quotes (eg, א', ב', ג', י', י"א, קכ"א, תרי"ג).</summary>
		LetterQuoted,
		///<summary>Spelled out, לשון זכר (eg, אחד, שנים, שלשה, עשרה)</summary>
		Fullזכר,
		///<summary>Spelled out, לשון נקבה (eg, אחת, שתים, שלש, עשר)</summary>
		Fullנקבה
	}
}

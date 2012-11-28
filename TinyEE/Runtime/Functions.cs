using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TinyEE
{
    /// <summary>
    /// A container for built in functions, some of which are a part of the runtime
    /// </summary>
    public static class Functions
    {
        //NOTE: currently, all the functions available to the TinyEE runtime are hard-coded here
        //NOTE: all function names MUST be in uppercase (runtime binder ignore the case of function names found inside expressions)
        #region Type Conversion
        /// <summary>
        /// Parse the input string as boolean
        /// </summary>
        public static bool BOOLEAN(string input)
        {
            return Boolean.Parse(input);
        }

        /// <summary>
        /// Convert the input to boolean
        /// </summary>
        public static bool BOOLEAN(dynamic input)
        {
            return (bool)input;
        }

        /// <summary>
        /// Parse the input string as int32
        /// </summary>
        public static int INT(string input)
        {
            return Int32.Parse(input);
        }

        /// <summary>
        /// Convert the input to int32
        /// </summary>
        public static int INT(dynamic input)
        {
            return (int)input;
        }

        /// <summary>
        /// Parse the input string as int64
        /// </summary>
        public static long LONG(string input)
        {
            return Int64.Parse(input);
        }

        /// <summary>
        /// Convert the input to int64
        /// </summary>
        public static long LONG(dynamic input)
        {
            return (long)input;
        }

        /// <summary>
        /// Parse the input string as biginteger
        /// </summary>
        public static BigInteger BIGINT(string input)
        {
            return BigInteger.Parse(input);
        }

        /// <summary>
        /// Convert the input to big integer
        /// </summary>
        public static BigInteger BIGINT(dynamic input)
        {
            return (BigInteger)input;
        }

        /// <summary>
        /// Parse the input string as double
        /// </summary>
        public static double DOUBLE(string input)
        {
            return Double.Parse(input);
        }

        /// <summary>
        /// Convert the input to double
        /// </summary>
        public static double DOUBLE(dynamic input)
        {
            return (double)input;
        }

        /// <summary>
        /// Parse the input string as decimal
        /// </summary>
        public static decimal DECIMAL(string input)
        {
            return Decimal.Parse(input);
        }

        /// <summary>
        /// Convert the input to decimal
        /// </summary>
        public static decimal DECIMAL(dynamic input)
        {
            return (decimal)input;
        }

        /// <summary>
        /// Convert the object to string. Return an empty string if the input is null
        /// </summary>
        public static string STR(dynamic input)
        {
            return input == null ? String.Empty : input.ToString();
        }

        /// <summary>
        /// Decode the input string as Base64
        /// </summary>
        public static byte[] BASE64DECODE(string input)
        {
            return Convert.FromBase64String(input);
        }

        /// <summary>
        /// Base64 encode the input bytes
        /// </summary>
        public static string BASE64ENCODE(byte[] input)
        {
            return Convert.ToBase64String(input);
        }

        /// <summary>
        /// Parse the input string as an Uri
        /// </summary>
        public static Uri URI(string input)
        {
            return new Uri(input);
        }

        /// <summary>
        /// Parse the input string as DateTime
        /// </summary>
        public static DateTime TIME(string input)
        {
            return DateTime.Parse(input, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        /// <summary>
        /// parse the input string as DateTime using the provided format
        /// </summary>
        public static DateTime TIME(string input, string format)
        {
            return DateTime.ParseExact(input, format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        /// <summary>
        /// Parse the input string as TimeSpan
        /// </summary>
        public static TimeSpan DURATION(string input)
        {
            return TimeSpan.Parse(input, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        /// <summary>
        /// Parse the input string as TimeSpan using the provided format
        /// </summary>
        public static TimeSpan DURATION(string input, string format)
        {
            return TimeSpan.ParseExact(input, format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }
        #endregion
        
        #region Generator
        /// <summary>
        /// Get current time in UTC
        /// </summary>
        public static DateTime TIME()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Get current day in UTC
        /// </summary>
        public static DateTime TODAY()
        {
            return DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Generate a random integer
        /// </summary>
        public static int RANDOM_INT()
        {
            return new Random().Next();
        }

        /// <summary>
        /// Generate a random double number in the range between 0.0 and 1.0
        /// </summary>
        public static double RANDOM_DOUBLE()
        {
            return new Random().NextDouble();
        }

        /// <summary>
        /// Generate a new Guid
        /// </summary>
        public static Guid GUID()
        {
            return Guid.NewGuid();
        }

        /// <summary>
        /// Generate a range of dates between left and right
        /// </summary>
        public static Range<DateTime> DATES(string leftStr, string rightStr)
        {
            DateTime left  = DateTime.Parse(leftStr),
                     right = DateTime.Parse(rightStr);
            return DATES(left, right);
        }

        /// <summary>
        /// Generate a range of dates between left and right
        /// </summary>
        public static Range<DateTime> DATES(DateTime left, DateTime right)
        {
            return Range<DateTime>.Dates(left, right);
        }
        #endregion

        #region Collections
        /// <summary>
        /// Get the size of the list
        /// </summary>
        public static dynamic SIZE(IEnumerable values)
        {
            return Enumerable.Count(values.Cast<object>());
        }

        /// <summary>
        /// Get the size of the list
        /// </summary>
        public static dynamic SUM(IEnumerable<dynamic> values)
        {
            return SUM(values.ToArray());
        }

        /// <summary>
        /// Get the size of the list
        /// </summary>
        public static dynamic SUM(params dynamic[] values)
        {
            return values.Aggregate(0, (i, seed) => seed + i);
        }
        #endregion
    }
}
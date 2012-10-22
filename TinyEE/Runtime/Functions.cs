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

        #region Used internally for expression rewriting
        public static IDictionary<string, object> DICTIONARY(KeyValuePair<string, object>[] kvps)
        {
            return kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        } 
        #endregion

        #region Conversion
        public static bool BOOLEAN(string input)
        {
            return Boolean.Parse(input);
        }

        public static bool BOOLEAN(dynamic input)
        {
            return (bool)input;
        }

        public static int INT(dynamic input)
        {
            return (int)input;
        }

        public static int INT(string input)
        {
            return Int32.Parse(input);
        }

        public static long LONG(string input)
        {
            return Int64.Parse(input);
        }

        public static long LONG(dynamic input)
        {
            return (long)input;
        }

        public static BigInteger BIGINT(string input)
        {
            return BigInteger.Parse(input);
        }

        public static BigInteger BIGINT(dynamic input)
        {
            return (BigInteger)input;
        }

        public static double DOUBLE(string input)
        {
            return Double.Parse(input);
        }

        public static double DOUBLE(dynamic input)
        {
            return (double)input;
        }

        public static decimal DECIMAL(string input)
        {
            return Decimal.Parse(input);
        }

        public static decimal DECIMAL(dynamic input)
        {
            return (decimal)input;
        }

        public static string STR(dynamic input)
        {
            return input == null ? String.Empty : input.ToString();
        }

        public static byte[] BASE64DECODE(string input)
        {
            return Convert.FromBase64String(input);
        }

        public static string BASE64ENCODE(byte[] input)
        {
            return Convert.ToBase64String(input);
        }

        public static Uri URI(string input)
        {
            return new Uri(input);
        }

        public static DateTime TIME(string input)
        {
            return DateTime.Parse(input, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static DateTime TIME(string input, string format)
        {
            return DateTime.ParseExact(input, format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static TimeSpan DURATION(string input)
        {
            return TimeSpan.Parse(input, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static TimeSpan DURATION(string input, string format)
        {
            return TimeSpan.ParseExact(input, format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }
        #endregion
        
        #region Generator
        public static DateTime TIME()
        {
            return DateTime.UtcNow;
        }

        public static DateTime TODAY()
        {
            return DateTime.UtcNow.Date;
        }

        public static int RANDOM_INT()
        {
            return new Random().Next();
        }

        public static double RANDOM_DOUBLE()
        {
            return new Random().NextDouble();
        }

        public static Guid GUID()
        {
            return Guid.NewGuid();
        }

        public static Range<DateTime> DATES(string leftStr, string rightStr)
        {
            DateTime left  = DateTime.Parse(leftStr),
                     right = DateTime.Parse(rightStr);
            return DATES(left, right);
        }

        public static Range<DateTime> DATES(DateTime left, DateTime right)
        {
            return Range<DateTime>.Dates(left, right);
        }
        #endregion

        #region Collections
        public static dynamic SIZE(IEnumerable values)
        {
            return Enumerable.Count(values.Cast<object>());
        }
        
        public static dynamic SUM(IEnumerable<dynamic> values)
        {
            return SUM(values.ToArray());
        }

        public static dynamic SUM(params dynamic[] values)
        {
            return values.Aggregate(0, (i, seed) => seed + i);
        }
        #endregion
    }
}